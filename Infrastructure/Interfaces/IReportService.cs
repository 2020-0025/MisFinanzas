using MisFinanzas.Domain.DTOs;

namespace MisFinanzas.Infrastructure.Interfaces
{
    public interface IReportService
    {
        /// Genera los datos completos del reporte según los filtros
        Task<ReportDataDto> GenerateReportDataAsync(ReportFilterDto filter);
    }
}