using MisFinanzas.Domain.DTOs;
using MisFinanzas.Domain.Enums;
using MisFinanzas.Infrastructure.Interfaces;
using System.Text;
using System.Text.Json;

namespace MisFinanzas.Infrastructure.Services;

/// <summary>
/// Servicio de asistente de IA usando Google Gemini API
/// </summary>
public class AIAssistantService : IAIAssistantService
{
    private readonly IConfiguration _configuration;
    private readonly IExpenseIncomeService _expenseIncomeService;
    private readonly IBudgetService _budgetService;
    private readonly IFinancialGoalService _goalService;
    private readonly ILoanService _loanService;
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private const string GEMINI_API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";





    public AIAssistantService(
        IConfiguration configuration,
        IExpenseIncomeService expenseIncomeService,
        IBudgetService budgetService,
        IFinancialGoalService goalService,
        ILoanService loanService,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _expenseIncomeService = expenseIncomeService;
        _budgetService = budgetService;
        _goalService = goalService;
        _loanService = loanService;
        _httpClient = httpClient;
        _apiKey = _configuration["GoogleGemini:ApiKey"];
    }

    public bool IsAvailable()
    {
        return !string.IsNullOrEmpty(_apiKey);
    }

    public async Task<AIMessageDto> SendMessageAsync(string userMessage, string userId, List<AIMessageDto>? conversationHistory = null)
    {
        if (!IsAvailable())
        {
            return new AIMessageDto
            {
                Role = "assistant",
                Content = "Lo siento, el asistente de IA no está configurado. Por favor, configura la API key de Google Gemini.",
                Timestamp = DateTime.Now
            };
        }

        try
        {
            // Obtener contexto financiero del usuario
            var financialContext = await GetUserFinancialContextAsync(userId);

            // Construir el prompt del sistema
            var systemPrompt = BuildSystemPrompt(financialContext);

            // Construir historial de conversación
            var messages = new List<object>();

            // Agregar contexto del sistema
            messages.Add(new
            {
                role = "user",
                parts = new[] { new { text = systemPrompt } }
            });

            messages.Add(new
            {
                role = "model",
                parts = new[] { new { text = "Entendido. Soy tu asistente financiero personal para MisFinanzas. Estoy listo para ayudarte con tus preguntas sobre tus finanzas." } }
            });

            // Agregar historial previo (Últimos 10 mensajes para ahorrar tokens)
            if (conversationHistory != null && conversationHistory.Count > 0)
            {
                // Tomamos solo los últimos 10 mensajes
                var recentHistory = conversationHistory.TakeLast(10);

                foreach (var msg in recentHistory)
                {
                    messages.Add(new
                    {
                        role = msg.Role == "user" ? "user" : "model",
                        parts = new[] { new { text = msg.Content } }
                    });
                }
            }

            // Agregar mensaje actual del usuario
            messages.Add(new
            {
                role = "user",
                parts = new[] { new { text = userMessage } }
            });

            // Preparar request para Gemini API
            var requestBody = new
            {
                contents = messages,
                generationConfig = new
                {
                    temperature = 0.7,
                    topK = 40,
                    topP = 0.95,
                    maxOutputTokens = 1024
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Llamar a Gemini API
            var response = await _httpClient.PostAsync($"{GEMINI_API_URL}?key={_apiKey}", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error de Gemini API: {response.StatusCode} - {errorContent}");

                return new AIMessageDto
                {
                    Role = "assistant",
                    Content = "Lo siento, hubo un error al comunicarme con el servicio de IA. Por favor, intenta de nuevo.",
                    Timestamp = DateTime.Now
                };
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var geminiResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);

            // Extraer la respuesta del modelo
            var responseText = geminiResponse
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "No pude generar una respuesta.";

            return new AIMessageDto
            {
                Role = "assistant",
                Content = responseText,
                Timestamp = DateTime.Now
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en AIAssistantService: {ex.Message}");
            return new AIMessageDto
            {
                Role = "assistant",
                Content = "Ocurrió un error inesperado. Por favor, intenta de nuevo más tarde.",
                Timestamp = DateTime.Now
            };
        }
    }

    private async Task<string> GetUserFinancialContextAsync(string userId)
    {
        try
        {
            var now = DateTime.Now;
            var startDate = now.AddDays(-30);
            var endDate = now;
            var currentMonth = now.Month;
            var currentYear = now.Year;

            // Obtener totales generales
            var totalIngresos = await _expenseIncomeService.GetTotalIngresosByUserAsync(userId);
            var totalGastos = await _expenseIncomeService.GetTotalGastosByUserAsync(userId);
            var balance = await _expenseIncomeService.GetBalanceByUserAsync(userId);

            // Obtener ingresos y gastos del mes actual
            var ingresosMesActual = await _expenseIncomeService.GetIngresosMesActualAsync(userId);
            var gastosMesActual = await _expenseIncomeService.GetGastosMesActualAsync(userId);

            // Obtener transacciones recientes por tipo
            var transacciones = await _expenseIncomeService.GetByUserAndDateRangeAsync(userId, startDate, endDate);
            var gastos = transacciones.Where(t => t.Type == TransactionType.Expense).ToList();
            var ingresos = transacciones.Where(t => t.Type == TransactionType.Income).ToList();

            // Agrupar gastos por categoría
            var gastosPorCategoria = gastos
                .GroupBy(g => g.CategoryTitle)
                .Select(g => new { Categoria = g.Key, Total = g.Sum(x => x.Amount) })
                .OrderByDescending(x => x.Total)
                .Take(5);

            // Agrupar ingresos por categoría
            var ingresosPorCategoria = ingresos
                .GroupBy(i => i.CategoryTitle)
                .Select(i => new { Categoria = i.Key, Total = i.Sum(x => x.Amount) })
                .OrderByDescending(x => x.Total)
                .Take(5);

            // Obtener presupuestos activos del mes actual
            var budgets = await _budgetService.GetByUserAndPeriodAsync(userId, currentMonth, currentYear);

            // Obtener metas activas
            var goals = await _goalService.GetActiveByUserAsync(userId);

            // Obtener préstamos activos
            var loans = await _loanService.GetActiveByUserAsync(userId);
            var totalPrestado = await _loanService.GetTotalBorrowedAsync(userId);
            var totalPorPagar = await _loanService.GetTotalToPayAsync(userId);
            var totalRestante = await _loanService.GetTotalRemainingAsync(userId);
            var pagosMensuales = await _loanService.GetMonthlyPaymentsTotalAsync(userId);

            // Construir contexto
            var context = new StringBuilder();
            context.AppendLine("=== RESUMEN FINANCIERO ===");
            context.AppendLine($"Balance total: RD${balance:N2}");
            context.AppendLine($"Total ingresos históricos: RD${totalIngresos:N2}");
            context.AppendLine($"Total gastos históricos: RD${totalGastos:N2}");
            context.AppendLine();

            context.AppendLine("=== MES ACTUAL ===");
            context.AppendLine($"Ingresos este mes: RD${ingresosMesActual:N2}");
            context.AppendLine($"Gastos este mes: RD${gastosMesActual:N2}");
            context.AppendLine($"Balance del mes: RD${(ingresosMesActual - gastosMesActual):N2}");
            context.AppendLine();

            if (gastosPorCategoria.Any())
            {
                context.AppendLine("=== GASTOS POR CATEGORÍA (Últimos 30 días - Top 5) ===");
                foreach (var gasto in gastosPorCategoria)
                {
                    context.AppendLine($"  • {gasto.Categoria}: RD${gasto.Total:N2}");
                }
                context.AppendLine();
            }

            if (ingresosPorCategoria.Any())
            {
                context.AppendLine("=== INGRESOS POR CATEGORÍA (Últimos 30 días - Top 5) ===");
                foreach (var ingreso in ingresosPorCategoria)
                {
                    context.AppendLine($"  • {ingreso.Categoria}: RD${ingreso.Total:N2}");
                }
                context.AppendLine();
            }

            if (budgets.Any())
            {
                context.AppendLine("=== PRESUPUESTOS ACTIVOS (Mes actual) ===");
                foreach (var budget in budgets.Take(5))
                {
                    var percentage = budget.AssignedAmount > 0
                        ? (budget.SpentAmount / budget.AssignedAmount * 100)
                        : 0;
                    var estado = percentage >= 100 ? "⚠️ EXCEDIDO" : percentage >= 80 ? "⚡ Cerca del límite" : "✅ Normal";
                    context.AppendLine($"  • {budget.CategoryTitle}: RD${budget.SpentAmount:N2} / RD${budget.AssignedAmount:N2} ({percentage:F0}%) - {estado}");
                }
                context.AppendLine();
            }

            if (goals.Any())
            {
                context.AppendLine("=== METAS FINANCIERAS ACTIVAS ===");
                foreach (var goal in goals.Take(5))
                {
                    var progress = goal.TargetAmount > 0
                        ? (goal.CurrentAmount / goal.TargetAmount * 100)
                        : 0;
                    var diasRestantes = (goal.TargetDate - DateTime.Now).Days;
                    context.AppendLine($"  • {goal.Title}: RD${goal.CurrentAmount:N2} / RD${goal.TargetAmount:N2} ({progress:F0}%) - {diasRestantes} días restantes");
                }
                context.AppendLine();
            }

            if (loans.Any())
            {
                context.AppendLine("=== PRÉSTAMOS ACTIVOS ===");
                context.AppendLine($"Total prestado: RD${totalPrestado:N2}");
                context.AppendLine($"Total a pagar: RD${totalPorPagar:N2}");
                context.AppendLine($"Total restante: RD${totalRestante:N2}");
                context.AppendLine($"Pagos mensuales totales: RD${pagosMensuales:N2}");
                context.AppendLine();

                context.AppendLine("Detalle de préstamos:");
                foreach (var loan in loans.Take(5))
                {
                    var cuotasRestantes = loan.NumberOfInstallments - loan.InstallmentsPaid;
                    context.AppendLine($"  • {loan.Title}:");
                    context.AppendLine($"    - Cuota: RD${loan.InstallmentAmount:N2}");
                    context.AppendLine($"    - Cuotas pagadas: {loan.InstallmentsPaid} / {loan.NumberOfInstallments}");
                    context.AppendLine($"    - Cuotas restantes: {cuotasRestantes}");
                    context.AppendLine($"    - Saldo actual: RD${loan.CurrentBalance:N2}");
                }
                context.AppendLine();
            }

            return context.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error obteniendo contexto financiero: {ex.Message}");
            return "No se pudo obtener el contexto financiero del usuario.";
        }
    }

    private string BuildSystemPrompt(string financialContext)
    {
        return $@"Eres un asistente financiero personal inteligente para la aplicación 'MisFinanzas'. 

Tu rol es ayudar al usuario a gestionar sus finanzas personales de manera efectiva, respondiendo preguntas sobre sus gastos, ingresos, presupuestos, metas financieras y préstamos.

CONTEXTO FINANCIERO DEL USUARIO:
{financialContext}

INSTRUCCIONES:
- Responde en español (Español Dominicano preferiblemente)
- Usa el símbolo de moneda RD$ (Peso Dominicano)
- Sé amigable, claro y conciso
- Proporciona consejos financieros prácticos cuando sea apropiado
- Si el usuario pregunta sobre datos que no tienes, indícalo amablemente
- Mantén las respuestas enfocadas en finanzas personales
- Usa emojis ocasionalmente para hacer las respuestas más amigables (sin exagerar)
- Si detectas problemas financieros (sobregasto, metas en riesgo, préstamos altos, etc.), menciónalos constructivamente
- Cuando hables de préstamos, considera el impacto de los intereses y cuotas mensuales en el presupuesto

EJEMPLOS DE PREGUNTAS QUE PUEDES RESPONDER:
- ¿Cuánto gasté este mes?
- ¿En qué categoría gasto más?
- ¿Cómo voy con mis presupuestos?
- ¿Puedo alcanzar mi meta de ahorro?
- ¿Cuánto debo en préstamos?
- ¿Cuánto pago mensualmente en cuotas?
- Dame consejos para ahorrar más
- ¿Estoy gastando más que el mes pasado?
- ¿Mis préstamos son manejables?

Recuerda: Tu objetivo es ayudar al usuario a tomar mejores decisiones financieras y mantener un control saludable de sus finanzas.";
    }
}

