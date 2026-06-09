using Microsoft.AspNetCore.Mvc;
using NovaStaff.Models.DTOs.Employees;
using NovaStaff.Models.Filters;
using NovaStaff.Services.Interfaces;

namespace NovaStaff.API.Controllers;

[ApiController]
[Route("api/employees")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    // GET api/employees?pageIndex=1&pageSize=20&nameContains=...
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] EmployeeFilter filter,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _employeeService.GetPagedAsync(filter, pageIndex, pageSize, ct);
        return Ok(result);
    }

    // GET api/employees/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var emp = await _employeeService.GetByIdAsync(id, ct);
        return Ok(emp);
    }

    // GET api/employees/code/NV1001
    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(string code, CancellationToken ct = default)
    {
        var emp = await _employeeService.GetByCodeAsync(code, ct);
        return Ok(emp);
    }

    // GET api/employees/department/3
    [HttpGet("department/{departmentId:int}")]
    public async Task<IActionResult> GetByDepartment(int departmentId, CancellationToken ct = default)
    {
        var result = await _employeeService.GetByDepartmentAsync(departmentId, ct);
        return Ok(result);
    }

    // GET api/employees/5/subordinates
    [HttpGet("{id:int}/subordinates")]
    public async Task<IActionResult> GetSubordinates(int id, CancellationToken ct = default)
    {
        var result = await _employeeService.GetSubordinatesAsync(id, ct);
        return Ok(result);
    }

    // POST api/employees
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateEmployeeRequest request,
        CancellationToken ct = default)
    {
        var emp = await _employeeService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = emp.Id }, emp);
    }

    // PUT api/employees/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateEmployeeRequest request,
        CancellationToken ct = default)
    {
        var emp = await _employeeService.UpdateAsync(id, request, ct);
        return Ok(emp);
    }

    // DELETE api/employees/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        await _employeeService.DeleteAsync(id, ct);
        return NoContent();
    }

    // PUT api/employees/5/transfer
    [HttpPut("{id:int}/transfer")]
    public async Task<IActionResult> Transfer(
        int id,
        [FromBody] TransferDepartmentRequest request,
        CancellationToken ct = default)
    {
        await _employeeService.TransferDepartmentAsync(id, request.NewDepartmentId, ct);
        return NoContent();
    }
    [HttpGet("managers")]
    [ProducesResponseType(typeof(IEnumerable<EmployeeManagerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetManagers(CancellationToken ct)
    {
        var result = await _employeeService.GetManagersAsync(ct);
        return Ok(result);
    }
    /// <summary>
    /// Thay đổi trạng thái nhân viên
    /// </summary>
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(
        int id,
        [FromBody] ChangeEmployeeStatusRequest request,
        CancellationToken ct)
    {
        await _employeeService.ChangeStatusAsync(
            id,
            request.Status,
            ct);

        return NoContent();
    }
}