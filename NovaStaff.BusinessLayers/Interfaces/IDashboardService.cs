using NovaStaff.Models.DTOs.Dashboard;

namespace NovaStaff.BusinessLayers.Interfaces;

public interface IDashboardService
{
    Task<KpiSummaryDto> GetKpiSummaryAsync(CancellationToken ct = default);
    Task<List<EmployeeTrendDto>> GetEmployeeTrendsAsync(int limit, CancellationToken ct = default);
}