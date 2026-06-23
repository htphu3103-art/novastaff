using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces.Repositories;
using NovaStaff.Models.DTOs.Dashboard;

namespace NovaStaff.BusinessLayers.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepo;

    public DashboardService(IDashboardRepository dashboardRepo)
    {
        _dashboardRepo = dashboardRepo;
    }

    public Task<KpiSummaryDto> GetKpiSummaryAsync(CancellationToken ct = default)
        => _dashboardRepo.GetKpiSummaryAsync(ct);

    public Task<List<EmployeeTrendDto>> GetEmployeeTrendsAsync(int limit, CancellationToken ct = default) // 👈 thêm
        => _dashboardRepo.GetEmployeeTrendsAsync(limit, ct);
}