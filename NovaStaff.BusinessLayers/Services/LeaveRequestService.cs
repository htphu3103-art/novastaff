using Microsoft.EntityFrameworkCore;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces.Repositories;
using NovaStaff.Models.DTOs.LeaveRequest;
using NovaStaff.Models.Entities;
using NovaStaff.Models.Enums;

namespace NovaStaff.BusinessLayers.Services;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly ILeaveRequestRepository _repo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ILeaveCalculator _calculator;

    public LeaveRequestService(
        ILeaveRequestRepository repo,
        IEmployeeRepository employeeRepo,
        IUserRepository userRepo,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        ILeaveCalculator calculator)
    {
        _repo = repo;
        _employeeRepo = employeeRepo;
        _userRepo = userRepo;
        _uow = uow;
        _currentUser = currentUser;
        _calculator = calculator;
    }

    // =========================================================
    // MAPPER
    // =========================================================

    private static LeaveRequestDto Map(LeaveRequest x) => new()
    {
        RequestId = x.RequestID,
        EmployeeId = x.EmployeeID,
        FromDate = x.FromDate,
        ToDate = x.ToDate,
        TotalDays = x.TotalDays,
        Status = x.Status,
        LeaveType = x.LeaveType,
        ApprovedBy = x.ApprovedBy,
        ApprovedDate = x.ApprovedDate,
        Reason = x.Reason,
        EmployeeName = x.Employee?.FullName,
        EmployeeCode = x.Employee?.EmployeeCode
        
    };

    // =========================================================
    // READ
    // =========================================================

    public async Task<IEnumerable<LeaveRequestDto>> GetMyRequestsAsync(CancellationToken ct = default)
    {
        var employeeId = await GetCurrentEmployeeIdAsync(ct);

        var data = await _repo.GetByEmployeeAsync(employeeId, ct);
        return data.Select(Map);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetByEmployeeAsync(int employeeId, CancellationToken ct = default)
    {
        await EnsureEmployeeExists(employeeId, ct);

        var data = await _repo.GetByEmployeeAsync(employeeId, ct);
        return data.Select(Map);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetPendingAsync(int? departmentId, CancellationToken ct = default)
    {
        var data = await _repo.GetPendingAsync(departmentId, ct);
        return data.Select(Map);
    }

    public async Task<double> GetApprovedDaysAsync(int employeeId, int year, CancellationToken ct = default)
    {
        await EnsureEmployeeExists(employeeId, ct);
        return await _repo.CountApprovedDaysAsync(employeeId, year, ct);
    }

    // =========================================================
    // CREATE
    // =========================================================

    public async Task<LeaveRequestDto> CreateAsync(
    CreateLeaveRequest request,
    CancellationToken ct = default)
    {
        var currentUserId = _currentUser.GetUserId()
    ?? throw new UnauthorizedAccessException("Unauthenticated");

        var employeeId = await _userRepo.GetEmployeeIdByUserIdAsync(currentUserId, ct)
            ?? throw new InvalidOperationException("Không tìm thấy user");

        request.EmployeeId = employeeId;
        // override từ server
        request.EmployeeId = employeeId;

        await EnsureEmployeeExists(request.EmployeeId, ct);

        if (request.ToDate < request.FromDate)
            throw new ArgumentException("Khoảng thời gian không hợp lệ");

        // check overlap
        var overlap = await _repo.ExistsAsync(x =>
            x.EmployeeID == request.EmployeeId &&
            x.Status != LeaveRequestStatus.Rejected &&
            x.FromDate <= request.ToDate &&
            x.ToDate >= request.FromDate,
            ct);

        if (overlap)
            throw new InvalidOperationException("Đơn nghỉ bị trùng thời gian");

        // tính total days
        var totalDays = _calculator.CalculateTotalDays(
            request.FromDate,
            request.ToDate,
            request.IsHalfDayStart,
            request.IsHalfDayEnd);

        var entity = new LeaveRequest
        {
            EmployeeID = request.EmployeeId,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            LeaveType = request.LeaveType,
            TotalDays = totalDays,
            Status = LeaveRequestStatus.Pending,
            Reason = request.Reason
        };

        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return Map(entity);
    }

    // =========================================================
    // APPROVAL FLOW
    // =========================================================

    public async Task ApproveAsync(int requestId, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(requestId, true, ct: ct);

        if (entity == null)
            throw new KeyNotFoundException("Không tìm thấy đơn");

        if (entity.Status != LeaveRequestStatus.Pending)
            throw new InvalidOperationException("Đơn không còn ở trạng thái Pending");

        var approverId = await GetCurrentEmployeeIdAsync(ct);

        entity.Status = LeaveRequestStatus.Approved;
        entity.ApprovedBy = approverId;
        entity.ApprovedDate = DateTime.UtcNow;

        _repo.Update(entity);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task RejectAsync(int requestId, string? reason, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(requestId, true, ct: ct);

        if (entity == null)
            throw new KeyNotFoundException("Không tìm thấy đơn");

        if (entity.Status != LeaveRequestStatus.Pending)
            throw new InvalidOperationException("Đơn không còn ở trạng thái Pending");

        var approverId = await GetCurrentEmployeeIdAsync(ct);

        entity.Status = LeaveRequestStatus.Rejected;
        entity.ApprovedBy = approverId;
        entity.ApprovedDate = DateTime.UtcNow;
        entity.Reason = reason;

        _repo.Update(entity);
        await _uow.SaveChangesAsync(ct);
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private async Task EnsureEmployeeExists(int employeeId, CancellationToken ct)
    {
        if (!await _employeeRepo.ExistsAsync(employeeId, ct))
            throw new KeyNotFoundException($"Không tìm thấy employee {employeeId}");
    }

    private async Task<int> GetCurrentEmployeeIdAsync(CancellationToken ct)
    {
        var userId = _currentUser.GetUserId()
            ?? throw new UnauthorizedAccessException();

        var empId = await _userRepo.GetEmployeeIdByUserIdAsync(userId, ct);

        if (empId == null)
            throw new InvalidOperationException("User chưa map employee");

        return empId.Value;
    }
}