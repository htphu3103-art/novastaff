using NovaStaff.Models.DTOs.LeaveRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStaff.BusinessLayers.Interfaces
{
    public interface ILeaveRequestService
    {
        Task<IEnumerable<LeaveRequestDto>> GetMyRequestsAsync(CancellationToken ct = default);

        Task<IEnumerable<LeaveRequestDto>> GetByEmployeeAsync(int employeeId, CancellationToken ct = default);

        Task<IEnumerable<LeaveRequestDto>> GetPendingAsync(int? departmentId, CancellationToken ct = default);

        Task<double> GetApprovedDaysAsync(int employeeId, int year, CancellationToken ct = default);

        Task<LeaveRequestDto> CreateAsync(CreateLeaveRequest request, CancellationToken ct = default);

        Task ApproveAsync(int requestId, CancellationToken ct = default);

        Task RejectAsync(int requestId, string? reason, CancellationToken ct = default);
    }
}
