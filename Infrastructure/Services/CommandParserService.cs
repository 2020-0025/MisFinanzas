using MisFinanzas.Domain.DTOs;
using MisFinanzas.Infrastructure.Interfaces;
using MisFinanzas.Domain.Enums;
using System.Text;
using System.Text.Json;

namespace MisFinanzas.Infrastructure.Services;

// Servicio que usa IA para detectar comandos en lenguaje natural
public class CommandParserService : ICommandParserService
{
    private readonly IConfiguration _configuration;
    private readonly ICategoryService _categoryService;
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private const string GEMINI_API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    public CommandParserService(
        IConfiguration configuration,
        ICategoryService categoryService,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _categoryService = categoryService;
        _httpClient = httpClient;
        _apiKey = _configuration["GoogleGemini:ApiKey"];
    }

    public async Task<CommandDto?> ParseCommandAsync(string message, string userId)
    {
        if (string.IsNullOrEmpty(_apiKey))
            return null;

        try
        {
            // Obtener categorías del usuario para ayudar en la detección
            var categories = await _categoryService.GetAllByUserAsync(userId);
            var categoryList = string.Join(", ", categories.Select(c => $"{c.Icon} {c.Title}"));

            var prompt = BuildCommandDetectionPrompt(message, categoryList);

            // Preparar request para Gemini
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = prompt } }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.1, // Baja temperatura para respuestas más consistentes
                    topK = 1,
                    topP = 0.95,
                    maxOutputTokens = 1024
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{GEMINI_API_URL}?key={_apiKey}", content);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error en CommandParser: {response.StatusCode}");
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var geminiResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);

            var responseText = geminiResponse
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrEmpty(responseText))
                return null;

            // Parsear la respuesta JSON de Gemini
            return ParseGeminiResponse(responseText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en ParseCommandAsync: {ex.Message}");
            return null;
        }
    }

    private string BuildCommandDetectionPrompt(string message, string categoriesList)
    {
        return $@"Eres un detector de comandos financieros. Analiza el siguiente mensaje y determina si es un COMANDO EJECUTABLE o solo una CONSULTA/CONVERSACIÓN.

MENSAJE DEL USUARIO:
""{message}""

CATEGORÍAS DISPONIBLES:
{categoriesList}

COMANDOS SOPORTADOS:

=== GASTOS E INGRESOS ===
1. CREATE_EXPENSE: Crear un gasto
   Variaciones: ""crea un gasto"", ""registra un gasto"", ""gasto de"", ""gasté"", ""compré"", ""pagué""
   Parámetros: amount (decimal), category (string), date (string opcional), description (string opcional)

2. CREATE_INCOME: Crear un ingreso
   Variaciones: ""crea un ingreso"", ""registra un ingreso"", ""ingreso de"", ""recibí"", ""me pagaron""
   Parámetros: amount (decimal), category (string), date (string opcional), description (string opcional)

3. DELETE_LAST_EXPENSE: Eliminar último gasto
   Variaciones: ""elimina mi último gasto"", ""borra el último gasto"", ""quita el último gasto""
   Parámetros: ninguno

4. DELETE_LAST_INCOME: Eliminar último ingreso
   Variaciones: ""elimina mi último ingreso"", ""borra el último ingreso""
   Parámetros: ninguno

5. GET_BALANCE: Consultar balance actual
   Variaciones: ""cuál es mi balance"", ""saldo actual"", ""balance"", ""tengo dinero"", ""mi balance""
   Parámetros: ninguno

6. GET_EXPENSES_BY_CATEGORY: Consultar gastos por categoría
   Variaciones: ""cuánto gasté en"", ""gastos de"", ""gastos en categoría""
   Parámetros: category (string), period (string opcional: ""este mes"", ""esta semana"")

7. GET_INCOMES_BY_CATEGORY: Consultar ingresos por categoría
   Variaciones: ""cuánto gané en"", ""ingresos de"", ""ingresos en categoría""
   Parámetros: category (string), period (string opcional)

8. GET_RECENT_TRANSACTIONS: Últimas transacciones
   Variaciones: ""mis últimas transacciones"", ""últimos movimientos"", ""transacciones recientes""
   Parámetros: count (int opcional, default: 10)

=== PRESUPUESTOS ===
9. CREATE_BUDGET: Crear presupuesto
   Variaciones: ""crea un presupuesto"", ""establece presupuesto"", ""presupuesto de""
   Parámetros: amount (decimal), category (string), month (int opcional), year (int opcional)

10. UPDATE_BUDGET: Actualizar presupuesto
    Variaciones: ""aumenta presupuesto"", ""reduce presupuesto"", ""cambia presupuesto""
    Parámetros: category (string), newAmount (decimal)

11. DELETE_BUDGET: Eliminar presupuesto
    Variaciones: ""elimina presupuesto"", ""borra presupuesto""
    Parámetros: category (string)

12. GET_BUDGET_STATUS: Estado de un presupuesto
    Variaciones: ""cómo va mi presupuesto de"", ""estado del presupuesto""
    Parámetros: category (string)

13. GET_ALL_BUDGETS: Todos los presupuestos
    Variaciones: ""mis presupuestos"", ""todos los presupuestos"", ""lista de presupuestos""
    Parámetros: ninguno

14. GET_BUDGETS_BY_STATUS: Presupuestos por estado
    Variaciones: ""presupuestos excedidos"", ""presupuestos cerca del límite""
    Parámetros: status (string: ""excedidos"", ""cerca del límite"", ""normales"")

=== METAS FINANCIERAS ===
15. CREATE_GOAL: Crear meta de ahorro
    Variaciones: ""crea una meta"", ""nueva meta de ahorro"", ""quiero ahorrar para""
    Parámetros: title (string), targetAmount (decimal), targetDate (string), currentAmount (decimal opcional)

16. ADD_TO_GOAL: Agregar dinero a una meta
    Variaciones: ""agrega a mi meta"", ""aporta a la meta"", ""añade dinero a""
    Parámetros: goalTitle (string), amount (decimal)

17. WITHDRAW_FROM_GOAL: Retirar dinero de una meta
    Variaciones: ""retira de mi meta"", ""saca dinero de"", ""quita de la meta""
    Parámetros: goalTitle (string), amount (decimal)

18. COMPLETE_GOAL: Marcar meta como completada
    Variaciones: ""marca meta como completada"", ""completar meta"", ""meta cumplida""
    Parámetros: goalTitle (string)

19. CANCEL_GOAL: Cancelar meta
    Variaciones: ""cancela meta"", ""elimina meta""
    Parámetros: goalTitle (string)

20. GET_GOAL_PROGRESS: Progreso de una meta
    Variaciones: ""cómo va mi meta de"", ""progreso de la meta""
    Parámetros: goalTitle (string)

21. GET_ALL_GOALS: Todas las metas activas
    Variaciones: ""mis metas"", ""todas las metas"", ""lista de metas""
    Parámetros: ninguno

=== PRÉSTAMOS ===
22. CREATE_LOAN: Crear préstamo
    Variaciones: ""registra un préstamo"", ""tengo un préstamo de""
    Parámetros: title (string), principalAmount (decimal), installmentAmount (decimal), numberOfInstallments (int), dueDay (int)

23. REGISTER_LOAN_PAYMENT: Registrar pago de préstamo
    Variaciones: ""pagar cuota"", ""registra pago de préstamo"", ""pagué mi préstamo""
    Parámetros: loanTitle (string)

24. UNDO_LOAN_PAYMENT: Deshacer último pago
    Variaciones: ""deshacer pago"", ""quitar último pago""
    Parámetros: loanTitle (string)

25. GET_LOAN_STATUS: Estado de un préstamo
    Variaciones: ""cómo va mi préstamo"", ""estado del préstamo""
    Parámetros: loanTitle (string)

26. GET_ALL_LOANS: Todos los préstamos activos
    Variaciones: ""mis préstamos"", ""lista de préstamos"", ""todos los préstamos""
    Parámetros: ninguno

27. GET_UPCOMING_PAYMENTS: Próximos pagos
    Variaciones: ""próximos pagos"", ""cuándo pago"", ""próximas cuotas""
    Parámetros: daysAhead (int opcional, default: 7)

28. GET_TOTAL_DEBT: Total de deuda
    Variaciones: ""cuánto debo"", ""total de deudas"", ""mi deuda total""
    Parámetros: ninguno

=== CATEGORÍAS ===
29. CREATE_CATEGORY: Crear categoría
    Variaciones: ""crea una categoría"", ""nueva categoría""
    Parámetros: title (string), icon (string), type (string: ""Expense"" o ""Income"")

30. GET_CATEGORIES: Listar categorías
    Variaciones: ""mis categorías"", ""lista de categorías"", ""qué categorías tengo""
    Parámetros: type (string opcional: ""Expense"", ""Income"", ""All"")

=== ANÁLISIS Y REPORTES ===
31. GET_MONTH_SUMMARY: Resumen del mes
    Variaciones: ""resumen del mes"", ""cómo me fue este mes"", ""estadísticas del mes""
    Parámetros: month (int opcional), year (int opcional)

32. COMPARE_MONTHS: Comparar dos meses
    Variaciones: ""compara este mes con el anterior"", ""diferencia entre meses""
    Parámetros: month1 (string opcional), month2 (string opcional)

33. GET_TOP_EXPENSE_CATEGORIES: Top categorías de gasto
    Variaciones: ""en qué gasto más"", ""categorías con más gastos"", ""top gastos""
    Parámetros: limit (int opcional, default: 5), period (string opcional)

34. GET_SPENDING_TREND: Tendencia de gastos
    Variaciones: ""tendencia de gastos"", ""estoy gastando más o menos""
    Parámetros: months (int opcional, default: 3)

35. GET_SAVINGS_RATE: Tasa de ahorro
    Variaciones: ""cuánto ahorro"", ""mi tasa de ahorro"", ""porcentaje de ahorro""
    Parámetros: ninguno

INSTRUCCIONES:
- Si es un comando ejecutable, responde SOLO con JSON en este formato:
{{
  ""isCommand"": true,
  ""commandType"": ""CREATE_EXPENSE"",
  ""parameters"": {{
    ""amount"": 500,
    ""category"": ""Comida"",
    ""date"": ""hoy"",
    ""description"": ""Almuerzo""
  }},
  ""createCategoryIfMissing"": false,  // <--- NUEVO CAMPO (true si la categoría no existe en la lista)
  ""suggestedIcon"": ""📁"",           // <--- NUEVO CAMPO (Emoji sugerido si se va a crear)
  ""confirmationMessage"": ""¿Confirmas crear un gasto de RD$500 en Comida?""
}}

- Si NO es un comando (es conversación normal), responde:
{{
  ""isCommand"": false
}}

REGLAS:
- La categoría debe coincidir con una de las disponibles (sin emoji)
- SI LA CATEGORÍA NO EXISTE EN LA LISTA:
  1. Asigna el nombre tal cual lo dijo el usuario en el parámetro ""category"".
  2. Establece ""createCategoryIfMissing"": true.
  3. En ""suggestedIcon"", elige un emoji que represente esa nueva categoría (Ej: ""Sushi"" -> 🍣, ""Gimnasio"" -> 💪).
  4. En ""confirmationMessage"", menciona explícitamente que se creará la categoría (Ej: ""La categoría 'Sushi' no existe. ¿Creo la categoría y registro el gasto?"").
- Los montos deben ser números positivos
- Si detectas una fecha relativa (ej: ""ayer"", ""el lunes pasado"", ""el 15""), conviértela SIEMPRE al formato YYYY-MM-DD basándote en que hoy es {DateTime.Now:yyyy-MM-dd}.
- Si falta información crítica, marca isCommand: false
- Solo devuelve JSON válido, sin texto adicional
- El confirmationMessage debe ser claro y específico sobre la acción a realizar

Analiza el mensaje y responde:";
    }


    private CommandDto? ParseGeminiResponse(string responseText)
    {
        try
        {
            // Limpiar respuesta (quitar markdown si existe)
            responseText = responseText.Trim();
            if (responseText.StartsWith("```json"))
                responseText = responseText.Substring(7);
            if (responseText.StartsWith("```"))
                responseText = responseText.Substring(3);
            if (responseText.EndsWith("```"))
                responseText = responseText.Substring(0, responseText.Length - 3);
            responseText = responseText.Trim();

            Console.WriteLine($"[CommandParser] Respuesta de Gemini: {responseText}");

            var jsonDoc = JsonDocument.Parse(responseText);
            var root = jsonDoc.RootElement;

            // Verificar si tiene la propiedad isCommand
            if (!root.TryGetProperty("isCommand", out var isCommandElement))
            {
                Console.WriteLine("[CommandParser] No se encontró 'isCommand' en la respuesta");
                return null;
            }

            if (!isCommandElement.GetBoolean())
            {
                Console.WriteLine("[CommandParser] isCommand es false");
                return null;
            }

            // Verificar si tiene commandType
            if (!root.TryGetProperty("commandType", out var commandTypeElement))
            {
                Console.WriteLine("[CommandParser] No se encontró 'commandType' en la respuesta");
                return null;
            }

            var commandTypeStr = commandTypeElement.GetString();
            if (string.IsNullOrEmpty(commandTypeStr))
            {
                Console.WriteLine("[CommandParser] commandType está vacío");
                return null;
            }

            // Convertir de SNAKE_CASE a PascalCase (ej: CREATE_EXPENSE -> CreateExpense)
            var commandTypePascal = ConvertToPascalCase(commandTypeStr);

            if (!Enum.TryParse<CommandType>(commandTypePascal, out var commandType))
            {
                Console.WriteLine($"[CommandParser] commandType inválido: {commandTypeStr} (convertido a: {commandTypePascal})");
                return null;
            }


            var parameters = new Dictionary<string, object>();
            if (root.TryGetProperty("parameters", out var paramsElement))
            {
                foreach (var param in paramsElement.EnumerateObject())
                {
                    parameters[param.Name] = param.Value.ValueKind switch
                    {
                        JsonValueKind.String => param.Value.GetString() ?? "",
                        JsonValueKind.Number => param.Value.GetDecimal(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        _ => param.Value.ToString()
                    };
                }
            }

            var confirmationMessage = root.TryGetProperty("confirmationMessage", out var msgElement)
                ? msgElement.GetString() ?? ""
                : "";

            var createCategoryIfMissing = false;
            if (root.TryGetProperty("createCategoryIfMissing", out var createElem))
            {
                createCategoryIfMissing = createElem.GetBoolean();
            }

            var suggestedIcon = "📁";
            if (root.TryGetProperty("suggestedIcon", out var iconElem))
            {
                suggestedIcon = iconElem.GetString() ?? "📁";
            }

            Console.WriteLine($"[CommandParser] Comando detectado: {commandType}");
            Console.WriteLine($"[CommandParser] Parámetros: {string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}"))}");

            return new CommandDto
            {
                Type = commandType,
                Parameters = parameters,
                RequiresConfirmation = true,
                ConfirmationMessage = confirmationMessage,
                // --- MAPEAR NUEVAS PROPIEDADES ---
                CreateCategoryIfMissing = createCategoryIfMissing,
                SuggestedIcon = suggestedIcon
            };
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[CommandParser] Error de JSON: {ex.Message}");
            Console.WriteLine($"[CommandParser] Respuesta: {responseText}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CommandParser] Error parseando respuesta: {ex.Message}");
            Console.WriteLine($"[CommandParser] Respuesta: {responseText}");
            return null;
        }
    }

    private string ConvertToPascalCase(string snakeCase)
    {
        if (string.IsNullOrEmpty(snakeCase))
            return snakeCase;

        // Convertir SNAKE_CASE o snake_case a PascalCase
        var words = snakeCase.Split('_');
        var result = string.Join("", words.Select(w =>
            char.ToUpper(w[0]) + w.Substring(1).ToLower()));

        return result;
    }


}

