//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Incomes;
using Application.Services;
using Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace API.Routes;

[ApiController]
[Route("api/v1/incomes")]
public class IncomeRoutes(IIncomeService incomeService) : ControllerBase
{
    private readonly IIncomeService _incomeService = incomeService;

    [HttpGet]
    public async Task<IActionResult> GetAllAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? accountId = null)
    {
        var filters = new IncomeFilterParams(startDate, endDate, accountId);
        var result = await _incomeService.GetAllAsync(filters, page, pageSize);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _incomeService.GetByIdAsync(id);
        if (!result.IsSuccess)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateIncomeRequest request)
    {
        var result = await _incomeService.CreateAsync(request);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return CreatedAtAction("GetById", new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateIncomeRequest request)
    {
        var result = await _incomeService.UpdateAsync(id, request);
        if (!result.IsSuccess)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _incomeService.DeleteAsync(id);
        if (!result.IsSuccess)
            return NotFound(result.Error);

        return NoContent();
    }
}
