using MisFinanzas.Domain.DTOs;

namespace MisFinanzas.Infrastructure.Interfaces
{
    public interface IPdfReportGenerator
    {
        /// <summary>
        /// Genera un archivo PDF a partir de los datos del reporte
        /// </summary>
        /// <returns>Bytes del archivo PDF generado</returns>
        byte[] GeneratePdf(ReportDataDto reportData);
    }
}