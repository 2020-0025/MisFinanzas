using ClosedXML.Excel;
using MisFinanzas.Domain.DTOs;
using MisFinanzas.Domain.Enums;
using MisFinanzas.Infrastructure.Interfaces;

namespace MisFinanzas.Infrastructure.Services
{
    public class ExcelReportGenerator : IExcelReportGenerator
    {
        /// Genera un archivo Excel del reporte y lo devuelve como array de bytes
        public byte[] GenerateExcel(ReportDataDto reportData, string logoPath)
        {
            using (var workbook = new XLWorkbook())
            {
                // Crear hojas
                CreateSummarySheet(workbook, reportData, logoPath);
                CreateExpensesByCategorySheet(workbook, reportData);
                CreateIncomesByCategorySheet(workbook, reportData);
                CreateTransactionsDetailSheet(workbook, reportData);

                // Convertir a bytes
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return stream.ToArray();
                }
            }
        }

        #region Summary Sheet

        private void CreateSummarySheet(XLWorkbook workbook, ReportDataDto reportData, string logoPath)
        {
            var worksheet = workbook.Worksheets.Add("Resumen");

            //  CONFIGURAR ANCHOS DE COLUMNA PRIMERO
            worksheet.Column(1).Width = 30.09; // Columna A - Logo
            worksheet.Column(2).Width = 20.09; // Columna B - Labels
            worksheet.Column(3).Width = 29.09; // Columna C - Valores
            worksheet.Column(4).Width = 3;     // Columna D - Espacio
            worksheet.Column(5).Width = 22.55; // Columna E - Comparación Labels
            worksheet.Column(6).Width = 23.09; // Columna F - Comparación Valores

            //  CONFIGURAR ALTURAS DE FILA
            worksheet.Row(1).Height = 60;   // Fila 1 - Logo y Título
            worksheet.Row(8).Height = 19;   // Fila 8 - Título Resumen

            //  COMBINAR CELDAS A1:A6 PARA EL LOGO
            worksheet.Range("A1:A6").Merge();

            //  LOGO EN A1:A6
            try
            {
                if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
                {
                    var picture = worksheet.AddPicture(logoPath);
                    picture.MoveTo(worksheet.Cell(1, 1)); // A1
                    picture.Scale(0.23); // Escala exacta
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading logo in Excel: {ex.Message}");
            }

            //  TÍTULO EN B1:L1
            var titleRange = worksheet.Range("B1:L1");
            titleRange.Merge();
            worksheet.Cell(1, 2).Value = "REPORTE DE MIS FINANZAS";
            worksheet.Cell(1, 2).Style.Font.Bold = true;
            worksheet.Cell(1, 2).Style.Font.FontSize = 16;
            worksheet.Cell(1, 2).Style.Font.FontColor = XLColor.White;
            worksheet.Cell(1, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(1, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            titleRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            titleRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;

            //  INFORMACIÓN DEL REPORTE (B2:C6)
            worksheet.Cell(2, 2).Value = "Usuario:";
            worksheet.Cell(2, 2).Style.Font.Bold = true;
            worksheet.Cell(2, 3).Value = reportData.UserName;

            worksheet.Cell(3, 2).Value = "Período:";
            worksheet.Cell(3, 2).Style.Font.Bold = true;
            worksheet.Cell(3, 3).Value = reportData.PeriodDescription;

            worksheet.Cell(4, 2).Value = "Desde:";
            worksheet.Cell(4, 2).Style.Font.Bold = true;
            worksheet.Cell(4, 3).Value = reportData.StartDate.ToString("dd/MM/yyyy");

            worksheet.Cell(5, 2).Value = "Hasta:";
            worksheet.Cell(5, 2).Style.Font.Bold = true;
            worksheet.Cell(5, 3).Value = reportData.EndDate.ToString("dd/MM/yyyy");

            worksheet.Cell(6, 2).Value = "Generado:";
            worksheet.Cell(6, 2).Style.Font.Bold = true;
            worksheet.Cell(6, 3).Value = reportData.GeneratedAt.ToString("dd/MM/yyyy HH:mm");

            // Bordes para información del reporte
            var infoRange = worksheet.Range("B2:C6");
            infoRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            infoRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            infoRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#F9F9F9");

            //  COMPARACIÓN CON PERÍODO ANTERIOR (E3:F6) - Al lado de la info
            if (reportData.Comparison != null)
            {
                // Título de comparación (E2:F2)
                var compTitleRange = worksheet.Range("E2:F2");
                compTitleRange.Merge();
                worksheet.Cell(2, 5).Value = "COMPARACIÓN CON PERÍODO ANTERIOR";
                worksheet.Cell(2, 5).Style.Font.Bold = true;
                worksheet.Cell(2, 5).Style.Font.FontSize = 11;
                worksheet.Cell(2, 5).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(2, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                compTitleRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#4682B4");
                compTitleRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;

                // Datos de comparación
                worksheet.Cell(3, 5).Value = "Cambio en Ingresos:";
                worksheet.Cell(3, 5).Style.Font.Bold = true;
                worksheet.Cell(3, 6).Value = reportData.Comparison.IncomeChangeDisplay;

                worksheet.Cell(4, 5).Value = "Cambio en Gastos:";
                worksheet.Cell(4, 5).Style.Font.Bold = true;
                worksheet.Cell(4, 6).Value = reportData.Comparison.ExpenseChangeDisplay;

                worksheet.Cell(5, 5).Value = "Cambio en Balance:";
                worksheet.Cell(5, 5).Style.Font.Bold = true;
                worksheet.Cell(5, 6).Value = reportData.Comparison.BalanceChangeDisplay;

                // Bordes para comparación
                var compRange = worksheet.Range("E3:F5");
                compRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                compRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                compRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#E6F2FF");
            }

            //  RESUMEN GENERAL (B8:C13)
            // Título
            var summaryTitleRange = worksheet.Range("B8:C8");
            summaryTitleRange.Merge();
            worksheet.Cell(8, 2).Value = "RESUMEN GENERAL";
            worksheet.Cell(8, 2).Style.Font.Bold = true;
            worksheet.Cell(8, 2).Style.Font.FontSize = 14;
            worksheet.Cell(8, 2).Style.Font.FontColor = XLColor.White;
            worksheet.Cell(8, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            summaryTitleRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            summaryTitleRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;

            // Datos del resumen
            worksheet.Cell(9, 2).Value = "Total Ingresos:";
            worksheet.Cell(9, 2).Style.Font.Bold = true;
            worksheet.Cell(9, 3).Value = reportData.Summary.TotalIncome;
            worksheet.Cell(9, 3).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(9, 3).Style.Font.FontColor = XLColor.DarkGreen;
            worksheet.Cell(9, 3).Style.Font.Bold = true;
            worksheet.Cell(9, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Cell(10, 2).Value = "Total Gastos:";
            worksheet.Cell(10, 2).Style.Font.Bold = true;
            worksheet.Cell(10, 3).Value = reportData.Summary.TotalExpense;
            worksheet.Cell(10, 3).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(10, 3).Style.Font.FontColor = XLColor.DarkRed;
            worksheet.Cell(10, 3).Style.Font.Bold = true;
            worksheet.Cell(10, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            int currentRow = 11; // Empezamos en la 11

            // --- NUEVO: Adquirido en préstamos ---
            if (reportData.Summary.TotalAdjustments > 0)
            {
                worksheet.Cell(currentRow, 2).Value = "Adquirido en préstamos:";
                worksheet.Cell(currentRow, 2).Style.Font.Bold = true;

                worksheet.Cell(currentRow, 3).Value = reportData.Summary.TotalAdjustments;
                worksheet.Cell(currentRow, 3).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(currentRow, 3).Style.Font.FontColor = XLColor.SteelBlue;
                worksheet.Cell(currentRow, 3).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                currentRow++; // Avanzamos una fila si hubo préstamos
            }

            // --- BALANCE (Ahora usa variable currentRow) ---
            worksheet.Cell(currentRow, 2).Value = "Balance:";
            worksheet.Cell(currentRow, 2).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 3).Value = reportData.Summary.Balance;
            worksheet.Cell(currentRow, 3).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(currentRow, 3).Style.Font.FontColor = reportData.Summary.Balance >= 0
                ? XLColor.DarkBlue
                : XLColor.DarkRed;
            worksheet.Cell(currentRow, 3).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            currentRow++;

            // --- PROMEDIO DIARIO ---
            worksheet.Cell(currentRow, 2).Value = "Promedio diario de gastos:";
            worksheet.Cell(currentRow, 3).Value = reportData.Summary.AverageDailyExpense;
            worksheet.Cell(currentRow, 3).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            currentRow++;

            // --- TOTAL TRANSACCIONES ---
            worksheet.Cell(currentRow, 2).Value = "Total de transacciones:";
            worksheet.Cell(currentRow, 3).Value = reportData.Summary.TotalTransactions;
            worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            // Ajustar el borde final al nuevo rango
            var summaryRange = worksheet.Range(9, 2, currentRow, 3); // De B9 hasta la última fila usada
            summaryRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            summaryRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        }

        #endregion

        #region Expenses By Category Sheet

        private void CreateExpensesByCategorySheet(XLWorkbook workbook, ReportDataDto reportData)
        {
            var worksheet = workbook.Worksheets.Add("Gastos por Categoría");

            // Encabezados
            worksheet.Cell(1, 1).Value = "Categoría";
            worksheet.Cell(1, 2).Value = "Monto Total";
            worksheet.Cell(1, 3).Value = "Porcentaje";
            worksheet.Cell(1, 4).Value = "Cantidad";

            // Estilo de encabezados
            var headerRange = worksheet.Range(1, 1, 1, 4);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Datos
            int row = 2;
            int startDataRow = row;
            foreach (var category in reportData.ExpensesByCategory)
            {
                worksheet.Cell(row, 1).Value = category.CategoryName;
                worksheet.Cell(row, 2).Value = category.TotalAmount;
                worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell(row, 3).Value = category.Percentage / 100;
                worksheet.Cell(row, 3).Style.NumberFormat.Format = "0.0%";
                worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 4).Value = category.TransactionCount;
                worksheet.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Filas alternas
                if ((row - startDataRow) % 2 == 1)
                {
                    worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#F0F0F0");
                }

                row++;
            }

            // Total
            if (reportData.ExpensesByCategory.Any())
            {
                var totalRange = worksheet.Range(row, 1, row, 4);
                totalRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#FFE6E6");
                totalRange.Style.Border.TopBorder = XLBorderStyleValues.Medium;

                worksheet.Cell(row, 1).Value = "TOTAL";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 2).Value = reportData.ExpensesByCategory.Sum(c => c.TotalAmount);
                worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 2).Style.Font.Bold = true;
                worksheet.Cell(row, 2).Style.Font.FontColor = XLColor.DarkRed;
                worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            }

            // Bordes a todos los datos
            if (reportData.ExpensesByCategory.Any())
            {
                var dataRange = worksheet.Range(startDataRow, 1, row, 4);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            // Ajustar anchos
            worksheet.Column(1).Width = 30;
            worksheet.Column(2).Width = 15;
            worksheet.Column(3).Width = 12;
            worksheet.Column(4).Width = 10;
        }

        #endregion

        #region Incomes By Category Sheet

        private void CreateIncomesByCategorySheet(XLWorkbook workbook, ReportDataDto reportData)
        {
            var worksheet = workbook.Worksheets.Add("Ingresos por Categoría");

            // Encabezados
            worksheet.Cell(1, 1).Value = "Categoría";
            worksheet.Cell(1, 2).Value = "Monto Total";
            worksheet.Cell(1, 3).Value = "Porcentaje";
            worksheet.Cell(1, 4).Value = "Cantidad";

            // Estilo de encabezados
            var headerRange = worksheet.Range(1, 1, 1, 4);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.DarkGreen;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Datos
            int row = 2;
            int startDataRow = row;
            foreach (var category in reportData.IncomesByCategory)
            {
                worksheet.Cell(row, 1).Value = category.CategoryName;
                worksheet.Cell(row, 2).Value = category.TotalAmount;
                worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell(row, 3).Value = category.Percentage / 100;
                worksheet.Cell(row, 3).Style.NumberFormat.Format = "0.0%";
                worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 4).Value = category.TransactionCount;
                worksheet.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Filas alternas
                if ((row - startDataRow) % 2 == 1)
                {
                    worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#F0F0F0");
                }

                row++;
            }

            // Total
            if (reportData.IncomesByCategory.Any())
            {
                var totalRange = worksheet.Range(row, 1, row, 4);
                totalRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#E6FFE6");
                totalRange.Style.Border.TopBorder = XLBorderStyleValues.Medium;

                worksheet.Cell(row, 1).Value = "TOTAL";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 2).Value = reportData.IncomesByCategory.Sum(c => c.TotalAmount);
                worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 2).Style.Font.Bold = true;
                worksheet.Cell(row, 2).Style.Font.FontColor = XLColor.DarkGreen;
                worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            }

            // Bordes a todos los datos
            if (reportData.IncomesByCategory.Any())
            {
                var dataRange = worksheet.Range(startDataRow, 1, row, 4);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            // Ajustar anchos
            worksheet.Column(1).Width = 30;
            worksheet.Column(2).Width = 15;
            worksheet.Column(3).Width = 12;
            worksheet.Column(4).Width = 10;
        }
        #endregion

        #region Transactions Detail Sheet

        private void CreateTransactionsDetailSheet(XLWorkbook workbook, ReportDataDto reportData)
        {
            var worksheet = workbook.Worksheets.Add("Detalle Transacciones");

            // Encabezados
            worksheet.Cell(1, 1).Value = "Fecha";
            worksheet.Cell(1, 2).Value = "Tipo";
            worksheet.Cell(1, 3).Value = "Categoría";
            worksheet.Cell(1, 4).Value = "Descripción";
            worksheet.Cell(1, 5).Value = "Monto";

            // Estilo de encabezados
            var headerRange = worksheet.Range(1, 1, 1, 5);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.DarkGray;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Datos
            int row = 2;
            int startDataRow = row;
            foreach (var transaction in reportData.Transactions)
            {
                worksheet.Cell(row, 1).Value = transaction.Date.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Columna 2: TIPO
                string typeText = transaction.Type switch
                {
                    TransactionType.Income => "Ingreso",
                    TransactionType.Expense => "Gasto",
                    _ => "Ajuste"
                };
                worksheet.Cell(row, 2).Value = typeText;
                worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 3).Value = transaction.CategoryTitle;

                // CAMBIO: Limpiar el emoji antes de asignar
                var descriptionClean = transaction.Description?.Replace("💰", "").Trim() ?? "-";
                worksheet.Cell(row, 4).Value = descriptionClean;
                // Activar "Wrap Text" en Excel para que también se ajuste si es largo
                worksheet.Cell(row, 4).Style.Alignment.WrapText = true;

                worksheet.Cell(row, 5).Value = transaction.Amount;
                worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                // Color según tipo
                if (transaction.Type == TransactionType.Income)
                {
                    worksheet.Cell(row, 5).Style.Font.FontColor = XLColor.DarkGreen;
                }
                else if (transaction.Type == TransactionType.Expense)
                {
                    worksheet.Cell(row, 5).Style.Font.FontColor = XLColor.DarkRed;
                }
                else // Ajuste
                {
                    worksheet.Cell(row, 5).Style.Font.FontColor = XLColor.SteelBlue;
                }

                // Filas alternas
                if ((row - startDataRow) % 2 == 1)
                {
                    worksheet.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#F0F0F0");
                }

                row++;
            }

            // Bordes a todos los datos
            if (reportData.Transactions.Any())
            {
                var dataRange = worksheet.Range(startDataRow, 1, row - 1, 5);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            // Ajustar anchos
            worksheet.Column(1).Width = 12;
            worksheet.Column(2).Width = 10;
            worksheet.Column(3).Width = 25;
            worksheet.Column(4).Width = 40;
            worksheet.Column(5).Width = 15;

            // Aplicar filtros
            var tableRange = worksheet.Range(1, 1, row - 1, 5);
            tableRange.SetAutoFilter();
        }

        #endregion
    }
}