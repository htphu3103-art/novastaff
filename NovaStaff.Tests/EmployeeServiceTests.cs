using FluentAssertions;
using Moq;
using NovaStaff.BusinessLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces;
using NovaStaff.DataLayers.Interfaces.Repositories;
using NovaStaff.Models.Entities;
using NovaStaff.Services.Interfaces;
using NovaStaff.Shared.Activation;
using NovaStaff.Shared.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using NovaStaff.BusinessLayers.Services;
using NovaStaff.Models.DTOs.Employees;

namespace NovaStaff.Tests;

public class EmployeeServiceTests
{
    // ===== MOCK TẤT CẢ DEPENDENCY =====
    private readonly Mock<IEmployeeRepository> _employeeRepoMock = new();
    private readonly Mock<IDepartmentRepository> _deptRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IDateTimeService> _dateTimeMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IActivationTokenService> _activationTokenMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<IConfiguration> _configMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextMock = new();

    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        // Tạo service với tất cả mock inject vào
        _service = new EmployeeService(
            _employeeRepoMock.Object,
            _deptRepoMock.Object,
            _userRepoMock.Object,
            _uowMock.Object,
            _dateTimeMock.Object,
            _currentUserMock.Object,
            _activationTokenMock.Object,
            _emailServiceMock.Object,
            _configMock.Object,
            _httpContextMock.Object
        );
    }

    // ===== TEST 1: GetByIdAsync - tìm thấy nhân viên =====
    [Fact]
    public async Task GetByIdAsync_WhenEmployeeExists_ReturnsDto()
    {
        // Arrange - chuẩn bị dữ liệu giả
        var fakeEmployee = new Employee
        {
            EmployeeID = 1,
            EmployeeCode = "EMP001",
            FullName = "Nguyen Van A",
            Email = "a@novastaff.com",
            Gender = NovaStaff.Models.Enums.GenderType.Male,
            Status = NovaStaff.Models.Enums.EmployeeStatus.Active,
            BaseSalary = 10_000_000
        };

        _employeeRepoMock
            .Setup(r => r.GetDetailByIdAsync(1, default))
            .ReturnsAsync(fakeEmployee);

        // Act - gọi function cần test
        var result = await _service.GetByIdAsync(1);

        // Assert - kiểm tra kết quả
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.FullName.Should().Be("Nguyen Van A");
        result.EmployeeCode.Should().Be("EMP001");
    }

    // ===== TEST 2: GetByIdAsync - không tìm thấy nhân viên =====
    [Fact]
    public async Task GetByIdAsync_WhenEmployeeNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange - mock trả về null
        _employeeRepoMock
            .Setup(r => r.GetDetailByIdAsync(99, default))
            .ReturnsAsync((Employee?)null);

        // Act & Assert - expect exception
        await _service.Invoking(s => s.GetByIdAsync(99))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*99*");
    }
    // ===== TEST: CreateAsync - tên rỗng =====
[Fact]
public async Task CreateAsync_WhenFullNameEmpty_ThrowsArgumentException()
{
    // Arrange
    var request = new CreateEmployeeRequest
    {
        FullName = "",
        EmployeeCode = "EMP002",
        Email = "b@novastaff.com"
    };

    // Act & Assert
    await _service.Invoking(s => s.CreateAsync(request))
        .Should().ThrowAsync<ArgumentException>()
        .WithMessage("*Tên*");
}

// ===== TEST: CreateAsync - mã NV đã tồn tại =====
[Fact]
public async Task CreateAsync_WhenCodeDuplicated_ThrowsInvalidOperationException()
{
    // Arrange
    var request = new CreateEmployeeRequest
    {
        FullName = "Nguyen Van B",
        EmployeeCode = "EMP001",
        Email = "b@novastaff.com"
    };

    _employeeRepoMock
        .Setup(r => r.IsCodeUniqueAsync("EMP001", null, default))
        .ReturnsAsync(false); // false = đã tồn tại

    // Act & Assert
    await _service.Invoking(s => s.CreateAsync(request))
        .Should().ThrowAsync<InvalidOperationException>()
        .WithMessage("*EMP001*");
}

// ===== TEST: CreateAsync - phòng ban không tồn tại =====
[Fact]
public async Task CreateAsync_WhenDepartmentNotFound_ThrowsKeyNotFoundException()
{
    // Arrange
    var request = new CreateEmployeeRequest
    {
        FullName = "Nguyen Van C",
        EmployeeCode = "EMP003",
        Email = "c@novastaff.com",
        DepartmentId = 99
    };

    _employeeRepoMock
        .Setup(r => r.IsCodeUniqueAsync("EMP003", null, default))
        .ReturnsAsync(true);

    _employeeRepoMock
        .Setup(r => r.IsEmailUniqueAsync("c@novastaff.com", null, default))
        .ReturnsAsync(true);

    _deptRepoMock
        .Setup(r => r.ExistsAsync(99, default))
        .ReturnsAsync(false); // false = không tồn tại

    // Act & Assert
    await _service.Invoking(s => s.CreateAsync(request))
        .Should().ThrowAsync<KeyNotFoundException>()
        .WithMessage("*Phòng ban*");
}
}