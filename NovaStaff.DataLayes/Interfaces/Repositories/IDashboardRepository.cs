using NovaStaff.Models.DTOs.Dashboard;

namespace NovaStaff.DataLayers.Interfaces.Repositories;

public interface IDashboardRepository
{
    Task<KpiSummaryDto> GetKpiSummaryAsync(CancellationToken ct = default);
    Task<List<EmployeeTrendDto>> GetEmployeeTrendsAsync(int limit, CancellationToken ct = default); // 👈 thêm
}