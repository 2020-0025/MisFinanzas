using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using MisFinanzas.Domain.DTOs;
using MisFinanzas.Domain.Enums;
using MisFinanzas.Infrastructure.Interfaces;

namespace MisFinanzas.Infrastructure.Services
{
    public class PdfReportGenerator : IPdfReportGenerator
    {
        // Método auxiliar para limpiar emojis y textos automáticos
        private string CleanText(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            // Elimina el emoji de dinero y espacios extra
            return text.Replace("💰", "").Trim();
        }
        public byte[] GeneratePdf(ReportDataDto reportData, string logoPath)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            // Cargar fuentes desde archivos embebidos - con múltiples rutas posibles
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var possiblePaths = new[]
            {
                Path.Combine(basePath, "wwwroot", "fonts"),
                Path.Combine(basePath, "..", "wwwroot", "fonts"),
                Path.Combine("/app", "wwwroot", "fonts"),
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts")
            };

            string? fontPath = null;
            foreach (var path in possiblePaths)
            {
                Console.WriteLine($"Intentando ruta de fuentes: {path}");
                if (Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path, "*.ttf");
                    Console.WriteLine($"  ✓ Directorio existe. Archivos encontrados: {files.Length}");
                    if (files.Length > 0)
                    {
                        fontPath = path;
                        Console.WriteLine($"  ✓ Usando ruta: {fontPath}");
                        break;
                    }
                }
                else
                {
                    Console.WriteLine($"  ✗ Directorio no existe");
                }
            }

            if (fontPath == null)
            {
                throw new DirectoryNotFoundException(
                    $"No se encontró el directorio de fuentes. Intentado en: {string.Join(", ", possiblePaths)}");
            }

            var regularFontPath = Path.Combine(fontPath, "LiberationSans-Regular.ttf");
            var boldFontPath = Path.Combine(fontPath, "LiberationSans-Bold.ttf");

            // Verificar que las fuentes existan
            if (!File.Exists(regularFontPath))
            {
                throw new FileNotFoundException($"Font file not found: {regularFontPath}");
            }

            Console.WriteLine($"Cargando fuentes desde: {fontPath}");
            Console.WriteLine($"  - Regular: {regularFontPath} (existe: {File.Exists(regularFontPath)})");
            Console.WriteLine($"  - Bold: {boldFontPath} (existe: {File.Exists(boldFontPath)})");

            var fontOptions = new XPdfFontOptions(PdfFontEncoding.Unicode);
            var fontTitle = new XFont("Liberation Sans", 20, XFontStyle.Bold, fontOptions);
            var fontSubtitle = new XFont("Liberation Sans", 12, XFontStyle.Regular, fontOptions);
            var fontHeader = new XFont("Liberation Sans", 14, XFontStyle.Bold, fontOptions);
            var fontNormal = new XFont("Liberation Sans", 10, XFontStyle.Regular, fontOptions);
            var fontSmall = new XFont("Liberation Sans", 8, XFontStyle.Regular, fontOptions);
            var fontBold = new XFont("Liberation Sans", 10, XFontStyle.Bold, fontOptions);

            double yPosition = 40;

            // LOGO - Mejor alineación con el título
            try
            {
                if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
                {
                    using var logoImage = XImage.FromFile(logoPath);
                    // Logo más arriba para alinearse con el centro del título
                    gfx.DrawImage(logoImage, 40, yPosition - 7, 80, 80); //  Cambio: yPosition + 5
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading logo: {ex.Message}");
            }

            // Ajustar posición del texto para que quede al lado del logo
            double textStartX = 130;

            // Encabezado - centrado verticalmente con el logo
            gfx.DrawString("MIS FINANZAS", fontTitle, XBrushes.Blue, new XPoint(textStartX, yPosition + 28)); // 🔥 Ajustado
            gfx.DrawString("Reporte Financiero", fontSubtitle, XBrushes.Gray, new XPoint(textStartX, yPosition + 48)); // 🔥 Ajustado

            yPosition += 80; // Mantener igual

            // Información del período
            gfx.DrawRectangle(XBrushes.LightGray, 40, yPosition, page.Width - 80, 60);
            yPosition += 15;
            gfx.DrawString($"Período: {reportData.PeriodDescription}", fontBold, XBrushes.Black, new XPoint(50, yPosition));
            yPosition += 15;
            gfx.DrawString($"Desde: {reportData.StartDate:dd/MM/yyyy}  Hasta: {reportData.EndDate:dd/MM/yyyy}", fontNormal, XBrushes.Black, new XPoint(50, yPosition));
            yPosition += 15;
            gfx.DrawString($"Usuario: {reportData.UserName}", fontSmall, XBrushes.Black, new XPoint(50, yPosition));
            yPosition += 30;

            // Resumen General
            gfx.DrawString("RESUMEN GENERAL", fontHeader, XBrushes.Blue, new XPoint(40, yPosition));
            yPosition += 20;

            gfx.DrawString("Total Ingresos:", fontBold, XBrushes.Black, new XPoint(40, yPosition));
            gfx.DrawString($"{reportData.Summary.TotalIncome:C}", fontBold, XBrushes.Green, new XPoint(400, yPosition));
            yPosition += 15;

            gfx.DrawString("Total Gastos:", fontBold, XBrushes.Black, new XPoint(40, yPosition));
            gfx.DrawString($"{reportData.Summary.TotalExpense:C}", fontBold, XBrushes.Red, new XPoint(400, yPosition));
            yPosition += 15;

            // --- NUEVO BLOQUE ---
            if (reportData.Summary.TotalAdjustments > 0)
            {
                gfx.DrawString("Adquirido en préstamos:", fontBold, XBrushes.Black, new XPoint(40, yPosition));
                // Usamos Azul (Info) para diferenciarlo de ingresos (Verde)
                gfx.DrawString($"{reportData.Summary.TotalAdjustments:C}", fontBold, XBrushes.SteelBlue, new XPoint(400, yPosition));
                yPosition += 15;
            }

            gfx.DrawString("Balance:", fontBold, XBrushes.Black, new XPoint(40, yPosition));
            var balanceColor = reportData.Summary.Balance >= 0 ? XBrushes.Blue : XBrushes.Red;
            gfx.DrawString($"{reportData.Summary.Balance:C}", fontBold, balanceColor, new XPoint(400, yPosition));
            yPosition += 15;

            gfx.DrawString("Promedio diario de gastos:", fontNormal, XBrushes.Black, new XPoint(40, yPosition));
            gfx.DrawString($"{reportData.Summary.AverageDailyExpense:C}", fontNormal, XBrushes.Black, new XPoint(400, yPosition));
            yPosition += 15;

            gfx.DrawString("Total de transacciones:", fontNormal, XBrushes.Black, new XPoint(40, yPosition));
            gfx.DrawString(reportData.Summary.TotalTransactions.ToString(), fontNormal, XBrushes.Black, new XPoint(400, yPosition));
            yPosition += 25;

            // Comparación (si existe)
            if (reportData.Comparison != null)
            {
                gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(173, 216, 230)), 40, yPosition, page.Width - 80, 40);
                yPosition += 15;
                gfx.DrawString("Comparación con período anterior", fontBold, XBrushes.Black, new XPoint(50, yPosition));
                yPosition += 15;
                gfx.DrawString($"Ingresos: {reportData.Comparison.IncomeChangeDisplay}  Gastos: {reportData.Comparison.ExpenseChangeDisplay}  Balance: {reportData.Comparison.BalanceChangeDisplay}",
                    fontSmall, XBrushes.Black, new XPoint(50, yPosition));
                yPosition += 25;
            }

            // Gastos por categoría
            if (reportData.ExpensesByCategory.Any())
            {
                yPosition = CheckNewPage(document, ref page, ref gfx, yPosition, fontTitle, fontSubtitle, fontHeader, fontNormal, fontSmall, fontBold);

                gfx.DrawString("GASTOS POR CATEGORÍA", fontHeader, XBrushes.Blue, new XPoint(40, yPosition));
                yPosition += 15; //  AUMENTADO de 20 a 25

                // Encabezados de tabla
                gfx.DrawRectangle(XBrushes.LightGray, 40, yPosition, page.Width - 80, 18);
                gfx.DrawString("Categoría", fontBold, XBrushes.Black, new XPoint(45, yPosition + 12));
                gfx.DrawString("Monto", fontBold, XBrushes.Black, new XPoint(300, yPosition + 12));
                gfx.DrawString("%", fontBold, XBrushes.Black, new XPoint(420, yPosition + 12));
                gfx.DrawString("Cant.", fontBold, XBrushes.Black, new XPoint(480, yPosition + 12));
                yPosition += 22;

                foreach (var category in reportData.ExpensesByCategory.Take(15))
                {
                    yPosition = CheckNewPage(document, ref page, ref gfx, yPosition, fontTitle, fontSubtitle, fontHeader, fontNormal, fontSmall, fontBold);

                    gfx.DrawString(TruncateString(category.CategoryName, 30), fontNormal, XBrushes.Black, new XPoint(45, yPosition + 10));
                    gfx.DrawString($"{category.TotalAmount:C}", fontNormal, XBrushes.Red, new XPoint(300, yPosition + 10));
                    gfx.DrawString($"{category.Percentage:F1}%", fontNormal, XBrushes.Black, new XPoint(420, yPosition + 10));
                    gfx.DrawString(category.TransactionCount.ToString(), fontNormal, XBrushes.Black, new XPoint(480, yPosition + 10));
                    yPosition += 18;
                }
                yPosition += 20; //  AUMENTADO de 10 a 15 (espacio después de la sección)
            }

            // Ingresos por categoría
            if (reportData.IncomesByCategory.Any())
            {
                yPosition = CheckNewPage(document, ref page, ref gfx, yPosition, fontTitle, fontSubtitle, fontHeader, fontNormal, fontSmall, fontBold);

                gfx.DrawString("INGRESOS POR CATEGORÍA", fontHeader, XBrushes.Blue, new XPoint(40, yPosition));
                yPosition += 15; //  AUMENTADO de 20 a 25

                // Encabezados de tabla
                gfx.DrawRectangle(XBrushes.LightGray, 40, yPosition, page.Width - 80, 18);
                gfx.DrawString("Categoría", fontBold, XBrushes.Black, new XPoint(45, yPosition + 12));
                gfx.DrawString("Monto", fontBold, XBrushes.Black, new XPoint(300, yPosition + 12));
                gfx.DrawString("%", fontBold, XBrushes.Black, new XPoint(420, yPosition + 12));
                gfx.DrawString("Cant.", fontBold, XBrushes.Black, new XPoint(480, yPosition + 12));
                yPosition += 22;

                foreach (var category in reportData.IncomesByCategory.Take(15))
                {
                    yPosition = CheckNewPage(document, ref page, ref gfx, yPosition, fontTitle, fontSubtitle, fontHeader, fontNormal, fontSmall, fontBold);

                    gfx.DrawString(TruncateString(category.CategoryName, 30), fontNormal, XBrushes.Black, new XPoint(45, yPosition + 10));
                    gfx.DrawString($"{category.TotalAmount:C}", fontNormal, XBrushes.Green, new XPoint(300, yPosition + 10));
                    gfx.DrawString($"{category.Percentage:F1}%", fontNormal, XBrushes.Black, new XPoint(420, yPosition + 10));
                    gfx.DrawString(category.TransactionCount.ToString(), fontNormal, XBrushes.Black, new XPoint(480, yPosition + 10));
                    yPosition += 18;
                }
                yPosition += 20; //  AUMENTADO de 10 a 15 (espacio después de la sección)
            }

            // Detalle de transacciones (primeras 30)
            if (reportData.Transactions.Any())
            {
                yPosition = CheckNewPage(document, ref page, ref gfx, yPosition, fontTitle, fontSubtitle, fontHeader, fontNormal, fontSmall, fontBold);

                gfx.DrawString("DETALLE DE TRANSACCIONES", fontHeader, XBrushes.Blue, new XPoint(40, yPosition));
                yPosition += 15; // AUMENTADO de 20 a 25

                // Encabezados de tabla
                gfx.DrawRectangle(XBrushes.LightGray, 40, yPosition, page.Width - 80, 18);
                gfx.DrawString("Fecha", fontBold, XBrushes.Black, new XPoint(45, yPosition + 12));
                gfx.DrawString("Tipo", fontBold, XBrushes.Black, new XPoint(105, yPosition + 12));
                gfx.DrawString("Categoría", fontBold, XBrushes.Black, new XPoint(180, yPosition + 12));
                gfx.DrawString("Descripción", fontBold, XBrushes.Black, new XPoint(280, yPosition + 12));
                gfx.DrawString("Monto", fontBold, XBrushes.Black, new XPoint(480, yPosition + 12));
                yPosition += 22;

                foreach (var transaction in reportData.Transactions.Take(50))
                {

                    yPosition = CheckNewPage(document, ref page, ref gfx, yPosition, fontTitle, fontSubtitle, fontHeader, fontNormal, fontSmall, fontBold);

                    // 1. Limpieza de texto (Quitar Emoji)
                    string descriptionClean = CleanText(transaction.Description ?? "-");
                    string categoryClean = transaction.CategoryTitle;

                    // 2. Definir anchos máximos para las columnas de texto largo
                    double categoryMaxWidth = 100; // Ancho disponible para categoría
                    double descMaxWidth = 200;     // Ancho disponible para descripción

                    // 3. Calcular altura necesaria (Simulamos el wrapping)
                    var categoryLines = SplitTextToFit(gfx, fontSmall, categoryClean, categoryMaxWidth);
                    var descLines = SplitTextToFit(gfx, fontSmall, descriptionClean, descMaxWidth);

                    // La altura de la fila será la del que tenga más líneas (mínimo 1 línea)
                    int maxLines = Math.Max(categoryLines.Count, descLines.Count);
                    double lineHeight = 10; // Altura por línea de texto
                    double rowHeight = (maxLines * lineHeight) + 8; // +8 de padding

                    // 4. Verificar salto de página con la altura dinámica
                    if (yPosition + rowHeight > page.Height - 40)
                    {
                        page = document.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        yPosition = 40;

                        // Volver a dibujar la cabecera de la tabla ---

                        // 1. Fondo Gris
                        gfx.DrawRectangle(XBrushes.LightGray, 40, yPosition, page.Width - 80, 18);

                        // 2. Textos de las columnas (Mismas coordenadas X que al principio)
                        gfx.DrawString("Fecha", fontBold, XBrushes.Black, new XPoint(45, yPosition + 12));
                        gfx.DrawString("Tipo", fontBold, XBrushes.Black, new XPoint(105, yPosition + 12));
                        gfx.DrawString("Categoría", fontBold, XBrushes.Black, new XPoint(160, yPosition + 12));
                        gfx.DrawString("Descripción", fontBold, XBrushes.Black, new XPoint(270, yPosition + 12));
                        gfx.DrawString("Monto", fontBold, XBrushes.Black, new XPoint(480, yPosition + 12));

                        // 3. Empujar la posición Y hacia abajo para que los datos no queden encima
                        yPosition += 22;
                    }

                    // LÓGICA MEJORADA PARA 3 TIPOS
                    string typeText;
                    XBrush amountColor;

                    switch (transaction.Type)
                    {
                        case TransactionType.Income:
                            typeText = "Ingreso";
                            amountColor = XBrushes.Green;
                            break;
                        case TransactionType.Expense:
                            typeText = "Gasto";
                            amountColor = XBrushes.Red;
                            break;
                        default: // Adjustment
                            typeText = "Ajuste";
                            amountColor = XBrushes.SteelBlue; // O XBrushes.Blue
                            break;
                    }

                    // Columna Fecha
                    gfx.DrawString(transaction.Date.ToString("dd/MM/yy"), fontSmall, XBrushes.Black, new XPoint(45, yPosition + 10));

                    // Columna Tipo
                    gfx.DrawString(typeText, fontSmall, XBrushes.Black, new XPoint(105, yPosition + 10));

                    // Columna Categoría (Multilínea)
                    DrawLines(gfx, fontSmall, XBrushes.Black, categoryLines, 160, yPosition + 10, lineHeight);

                    // Columna Descripción (Multilínea)
                    DrawLines(gfx, fontSmall, XBrushes.Black, descLines, 270, yPosition + 10, lineHeight);

                    // Columna Monto
                    gfx.DrawString($"{transaction.Amount:C}", fontSmall, amountColor, new XPoint(480, yPosition + 10));

                    // (Opcional) Línea separadora gris tenue
                    gfx.DrawLine(XPens.LightGray, 40, yPosition + rowHeight, page.Width - 40, yPosition + rowHeight);

                    // Avanzar cursor
                    yPosition += rowHeight;
                }

                if (reportData.Transactions.Count > 30)
                {
                    yPosition += 5;
                    gfx.DrawString($"* Mostrando las primeras 30 transacciones de {reportData.Transactions.Count} totales",
                        fontSmall, XBrushes.Gray, new XPoint(45, yPosition));
                }
            }

            // Pie de página
            gfx.DrawString($"Generado: {reportData.GeneratedAt:dd/MM/yyyy HH:mm}", fontSmall, XBrushes.Gray,
                new XPoint(40, page.Height - 30));

            using var stream = new MemoryStream();
            document.Save(stream, false);
            return stream.ToArray();
        }

        private double CheckNewPage(PdfDocument document, ref PdfPage page, ref XGraphics gfx, double yPosition,
            XFont fontTitle, XFont fontSubtitle, XFont fontHeader, XFont fontNormal, XFont fontSmall, XFont fontBold)
        {
            if (yPosition > page.Height - 100)
            {
                page = document.AddPage();
                gfx = XGraphics.FromPdfPage(page);
                return 40;
            }
            return yPosition;
        }

        private string TruncateString(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Length <= maxLength ? text : text.Substring(0, maxLength - 3) + "...";
        }


        // ==========================================
        // MÉTODOS AYUDANTES (NUEVOS)
        // ==========================================

        // Divide un texto en varias líneas para que quepa en un ancho específico
        private List<string> SplitTextToFit(XGraphics gfx, XFont font, string text, double maxWidth)
        {
            var lines = new List<string>();
            if (string.IsNullOrEmpty(text)) return lines;

            var words = text.Split(' ');
            var currentLine = "";

            foreach (var word in words)
            {
                var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                var size = gfx.MeasureString(testLine, font);

                if (size.Width > maxWidth)
                {
                    if (!string.IsNullOrEmpty(currentLine)) lines.Add(currentLine);
                    currentLine = word;
                }
                else
                {
                    currentLine = testLine;
                }
            }
            if (!string.IsNullOrEmpty(currentLine)) lines.Add(currentLine);

            // Manejo de palabras extremadamente largas (raro, pero posible)
            if (lines.Count == 0 && !string.IsNullOrEmpty(text)) lines.Add(text);

            return lines;
        }

        // Dibuja las líneas calculadas una debajo de otra
        private void DrawLines(XGraphics gfx, XFont font, XBrush brush, List<string> lines, double x, double startY, double lineHeight)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                gfx.DrawString(lines[i], font, brush, new XPoint(x, startY + (i * lineHeight)));
            }
        }
    }
}