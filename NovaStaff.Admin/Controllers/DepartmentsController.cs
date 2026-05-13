using Microsoft.AspNetCore.Mvc;
using NovaStaff.BusinessLayers.DTOs.Departments;
using NovaStaff.Models.DTOs.Department;
using NovaStaff.Models.Common;
using NovaStaff.Services.Interfaces;

namespace NovaStaff.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    // GET: api/departments
    [HttpGet]
    public async Task<ActionResult<PagedResult<DepartmentDto>>> GetAll(
        [FromQuery] DepartmentDescendantQuery query,
        CancellationToken ct)
    {
        // C?n thêm hàm GetAllAsync ho?c GetRootsAsync ? t?ng Service
        var result = await _departmentService.GetRootsAsync(query, ct);
        return Ok(result);
    }

    // =========================================================
    // GET BY ID
    // =========================================================
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DepartmentDto>> GetById(int id, CancellationToken ct)
    {
        try
        {
            var result = await _departmentService.GetByIdAsync(id, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // =========================================================
    // GET DESCENDANTS
    // =========================================================
    [HttpGet("{id:int}/descendants")]
    public async Task<ActionResult<PagedResult<DepartmentDto>>> GetDescendants(
        int id,
        [FromQuery] DepartmentDescendantQuery query,
        CancellationToken ct)
    {
        try
        {
            var result = await _departmentService.GetDescendantsAsync(id, query, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // =========================================================
    // CREATE
    // =========================================================
    [HttpPost]
    public async Task<ActionResult<DepartmentDto>> Create(
        [FromBody] CreateDepartmentRequest request,
        CancellationToken ct)
    {
        try
        {
            var result = await _departmentService.CreateAsync(request, ct);

            // REST chu?n: tr? v? 201 + location
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // =========================================================
    // UPDATE
    // =========================================================
    [HttpPut("{id:int}")]
    public async Task<ActionResult<DepartmentDto>> Update(
        int id,
        [FromBody] UpdateDepartmentRequest request,
        CancellationToken ct)
    {
        try
        {
            var result = await _departmentService.UpdateAsync(id, request, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // =========================================================
    // DELETE
    // =========================================================
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            await _departmentService.DeleteAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:int}/children")]
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetChildren(
    int id,
    CancellationToken ct)
    {
        try
        {
            var result = await _departmentService.GetChildrenAsync(id, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ? B?t ðúng exception, tr? message v? FE ð? hi?n toast
    [HttpPut("{id:int}/move")]
    public async Task<IActionResult> Move(
        int id,
        [FromQuery] int? newParentId,
        CancellationToken ct)
    {
        try
        {
            await _departmentService.MoveAsync(id, newParentId, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}



