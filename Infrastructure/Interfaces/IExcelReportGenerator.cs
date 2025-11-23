using MisFinanzas.Domain.DTOs;

namespace MisFinanzas.Infrastructure.Interfaces
{
    public interface IExcelReportGenerator
    {
        /// <summary>
        /// Genera un archivo Excel a partir de los datos del reporte
        /// </summary>
        /// <returns>Bytes del archivo Excel generado</returns>
        byte[] GenerateExcel(ReportDataDto reportData, string logoPath);
    }
}
