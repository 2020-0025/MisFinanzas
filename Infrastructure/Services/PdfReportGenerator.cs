using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using MisFinanzas.Domain.DTOs;
using MisFinanzas.Domain.Enums;
using MisFinanzas.Infrastructure.Interfaces;

namespace MisFinanzas.Infrastructure.Services
{
    public class PdfReportGenerator : IPdfReportGenerator
    {
        public byte[] GeneratePdf(ReportDataDto reportData)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            var fontTitle = new XFont("Times New Roman", 20, XFontStyle.Bold);
            var fontSubtitle = new XFont("Times New Roman", 12, XFontStyle.Regular);
            var fontHeader = new XFont("Times New Roman", 14, XFontStyle.Bold);
            var fontNormal = new XFont("Times New Roman", 10, XFontStyle.Regular);
            var fontSmall = new XFont("Times New Roman", 8, XFontStyle.Regular);
            var fontBold = new XFont("Times New Roman", 10, XFontStyle.Bold);

            double yPosition = 40;

            // Encabezado
            gfx.DrawString("MIS FINANZAS", fontTitle, XBrushes.Blue, new XPoint(40, yPosition));
            yPosition += 25;
            gfx.DrawString("Reporte Financiero", fontSubtitle, XBrushes.Gray, new XPoint(40, yPosition));
            yPosition += 30;

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
                yPosition += 20;

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
                yPosition += 10;
            }

            // Ingresos por categoría
            if (reportData.IncomesByCategory.Any())
            {
                yPosition = CheckNewPage(document, ref page, ref gfx, yPosition, fontTitle, fontSubtitle, fontHeader, fontNormal, fontSmall, fontBold);

                gfx.DrawString("INGRESOS POR CATEGORÍA", fontHeader, XBrushes.Blue, new XPoint(40, yPosition));
                yPosition += 20;

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
                yPosition += 10;
            }

            // Detalle de transacciones (primeras 30)
            if (reportData.Transactions.Any())
            {
                yPosition = CheckNewPage(document, ref page, ref gfx, yPosition, fontTitle, fontSubtitle, fontHeader, fontNormal, fontSmall, fontBold);

                gfx.DrawString("DETALLE DE TRANSACCIONES", fontHeader, XBrushes.Blue, new XPoint(40, yPosition));
                yPosition += 20;

                // Encabezados de tabla
                gfx.DrawRectangle(XBrushes.LightGray, 40, yPosition, page.Width - 80, 18);
                gfx.DrawString("Fecha", fontBold, XBrushes.Black, new XPoint(45, yPosition + 12));
                gfx.DrawString("Tipo", fontBold, XBrushes.Black, new XPoint(120, yPosition + 12));
                gfx.DrawString("Categoría", fontBold, XBrushes.Black, new XPoint(180, yPosition + 12));
                gfx.DrawString("Descripción", fontBold, XBrushes.Black, new XPoint(280, yPosition + 12));
                gfx.DrawString("Monto", fontBold, XBrushes.Black, new XPoint(450, yPosition + 12));
                yPosition += 22;

                foreach (var transaction in reportData.Transactions.Take(30))
                {
                    yPosition = CheckNewPage(document, ref page, ref gfx, yPosition, fontTitle, fontSubtitle, fontHeader, fontNormal, fontSmall, fontBold);

                    var typeText = transaction.Type == TransactionType.Income ? "Ingreso" : "Gasto";
                    var amountColor = transaction.Type == TransactionType.Income ? XBrushes.Green : XBrushes.Red;

                    gfx.DrawString(transaction.Date.ToString("dd/MM/yy"), fontSmall, XBrushes.Black, new XPoint(45, yPosition + 8));
                    gfx.DrawString(typeText, fontSmall, XBrushes.Black, new XPoint(120, yPosition + 8));
                    gfx.DrawString(TruncateString(transaction.CategoryTitle, 12), fontSmall, XBrushes.Black, new XPoint(180, yPosition + 8));
                    gfx.DrawString(TruncateString(transaction.Description ?? "-", 20), fontSmall, XBrushes.Black, new XPoint(280, yPosition + 8));
                    gfx.DrawString($"{transaction.Amount:C}", fontSmall, amountColor, new XPoint(450, yPosition + 8));
                    yPosition += 16;
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

        private string TruncateString(string? text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text.Length <= maxLength ? text : text.Substring(0, maxLength - 3) + "...";
        }
    }
}