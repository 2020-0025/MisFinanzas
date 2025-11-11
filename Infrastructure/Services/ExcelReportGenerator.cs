using ClosedXML.Excel;
using MisFinanzas.Domain.DTOs;
using MisFinanzas.Domain.Enums;
using MisFinanzas.Infrastructure.Interfaces;

namespace MisFinanzas.Infrastructure.Services
{
    public class ExcelReportGenerator : IExcelReportGenerator
    {
        /// Genera un archivo Excel del reporte y lo devuelve como array de bytes
        public byte[] GenerateExcel(ReportDataDto reportData)
        {
            using (var workbook = new XLWorkbook())

            {

                // Crear hojas

                CreateSummarySheet(workbook, reportData);

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

        private void CreateSummarySheet(XLWorkbook workbook, ReportDataDto reportData)
        {
            var worksheet = workbook.Worksheets.Add("Resumen");

            int row = 1;

            // Título
            var titleRange = worksheet.Range(row, 1, row, 2);
            titleRange.Merge();
            worksheet.Cell(row, 1).Value = "REPORTE DE MIS FINANZAS";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 1).Style.Font.FontSize = 16;
            worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.White;
            worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            titleRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            titleRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            row += 2;

            // Información del reporte
            int infoStartRow = row;
            worksheet.Cell(row, 1).Value = "Usuario:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Value = reportData.UserName;
            row++;

            worksheet.Cell(row, 1).Value = "Período:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Value = reportData.PeriodDescription;
            row++;

            worksheet.Cell(row, 1).Value = "Desde:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Value = reportData.StartDate.ToString("dd/MM/yyyy");
            row++;

            worksheet.Cell(row, 1).Value = "Hasta:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Value = reportData.EndDate.ToString("dd/MM/yyyy");
            row++;

            worksheet.Cell(row, 1).Value = "Generado:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Value = reportData.GeneratedAt.ToString("dd/MM/yyyy HH:mm");

            // Bordes para la información del reporte
            var infoRange = worksheet.Range(infoStartRow, 1, row, 2);
            infoRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            infoRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            infoRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#F9F9F9");
            row += 2;

            // Resumen general
            worksheet.Cell(row, 1).Value = "RESUMEN GENERAL";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 1).Style.Font.FontSize = 14;
            worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.White;
            worksheet.Range(row, 1, row, 2).Style.Fill.BackgroundColor = XLColor.DarkBlue;
            worksheet.Range(row, 1, row, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            row++;

            int summaryStartRow = row;

            // Total Ingresos
            worksheet.Cell(row, 1).Value = "Total Ingresos:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Value = reportData.Summary.TotalIncome;
            worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(row, 2).Style.Font.FontColor = XLColor.DarkGreen;
            worksheet.Cell(row, 2).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            row++;

            // Total Gastos
            worksheet.Cell(row, 1).Value = "Total Gastos:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Value = reportData.Summary.TotalExpense;
            worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(row, 2).Style.Font.FontColor = XLColor.DarkRed;
            worksheet.Cell(row, 2).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            row++;

            // Balance
            worksheet.Cell(row, 1).Value = "Balance:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Value = reportData.Summary.Balance;
            worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(row, 2).Style.Font.FontColor = reportData.Summary.Balance >= 0
                ? XLColor.DarkBlue
                : XLColor.DarkRed;
            worksheet.Cell(row, 2).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            row++;

            // Promedio diario
            worksheet.Cell(row, 1).Value = "Promedio diario de gastos:";
            worksheet.Cell(row, 2).Value = reportData.Summary.AverageDailyExpense;
            worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            row++;

            // Total transacciones
            worksheet.Cell(row, 1).Value = "Total de transacciones:";
            worksheet.Cell(row, 2).Value = reportData.Summary.TotalTransactions;
            worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            // Bordes para el resumen general
            var summaryRange = worksheet.Range(summaryStartRow, 1, row, 2);
            summaryRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            summaryRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            row += 2;

            // Comparativa (si existe)
            if (reportData.Comparison != null)
            {
                worksheet.Cell(row, 1).Value = "COMPARACIÓN CON PERÍODO ANTERIOR";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 1).Style.Font.FontSize = 12;
                worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.White;
                worksheet.Range(row, 1, row, 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#4682B4");
                worksheet.Range(row, 1, row, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                row++;

                int compStartRow = row;

                worksheet.Cell(row, 1).Value = "Cambio en Ingresos:";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 2).Value = reportData.Comparison.IncomeChangeDisplay;
                row++;

                worksheet.Cell(row, 1).Value = "Cambio en Gastos:";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 2).Value = reportData.Comparison.ExpenseChangeDisplay;
                row++;

                worksheet.Cell(row, 1).Value = "Cambio en Balance:";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 2).Value = reportData.Comparison.BalanceChangeDisplay;

                // Bordes para la comparación
                var compRange = worksheet.Range(compStartRow, 1, row, 2);
                compRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                compRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                compRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#E6F2FF");

                row++;
            }

            // Ajustar anchos de columna
            worksheet.Column(1).Width = 30;
            worksheet.Column(2).Width = 20;
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

                worksheet.Cell(row, 2).Value = transaction.Type == TransactionType.Income ? "Ingreso" : "Gasto";
                worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 3).Value = transaction.CategoryTitle;

                worksheet.Cell(row, 4).Value = transaction.Description ?? "-";

                worksheet.Cell(row, 5).Value = transaction.Amount;
                worksheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                // Color según tipo
                if (transaction.Type == TransactionType.Income)
                {
                    worksheet.Cell(row, 5).Style.Font.FontColor = XLColor.DarkGreen;
                    worksheet.Cell(row, 5).Style.Font.Bold = true;
                }
                else
                {
                    worksheet.Cell(row, 5).Style.Font.FontColor = XLColor.DarkRed;
                    worksheet.Cell(row, 5).Style.Font.Bold = true;
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