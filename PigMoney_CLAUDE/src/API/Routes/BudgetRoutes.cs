//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Budgets;
using Application.Services;
using Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace API.Routes;

[ApiController]
[Route("api/v1/budgets")]
public class BudgetRoutes(IBudgetService budgetService) : ControllerBase
{
    private readonly IBudgetService _budgetService = budgetService;

    [HttpGet]
    public async Task<IActionResult> GetAllAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? categoryId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var filters = new BudgetFilterParams(categoryId, startDate, endDate);
        var result = await _budgetService.GetAllAsync(filters, page, pageSize);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _budgetService.GetByIdAsync(id);
        if (!result.IsSuccess)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateBudgetRequest request)
    {
        var result = await _budgetService.CreateAsync(request);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return CreatedAtAction("GetById", new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateBudgetRequest request)
    {
        var result = await _budgetService.UpdateAsync(id, request);
        if (!result.IsSuccess)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _budgetService.DeleteAsync(id);
        if (!result.IsSuccess)
            return NotFound(result.Error);

        return NoContent();
    }
}
