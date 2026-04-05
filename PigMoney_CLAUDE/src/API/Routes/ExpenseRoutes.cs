//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Expenses;
using Application.DTOs.ExpenseItems;
using Application.Services;
using Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace API.Routes;

[ApiController]
[Route("api/v1/expenses")]
public class ExpenseRoutes(IExpenseService expenseService, IExpenseItemService expenseItemService) : ControllerBase
{
    private readonly IExpenseService _expenseService = expenseService;
    private readonly IExpenseItemService _expenseItemService = expenseItemService;

    [HttpGet]
    public async Task<IActionResult> GetAllAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? accountId = null,
        [FromQuery] int? categoryId = null)
    {
        var filters = new ExpenseFilterParams(startDate, endDate, accountId, categoryId);
        var result = await _expenseService.GetAllAsync(filters, page, pageSize);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _expenseService.GetByIdAsync(id);
        if (!result.IsSuccess)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateExpenseRequest request)
    {
        var result = await _expenseService.CreateAsync(request);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return CreatedAtAction("GetById", new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateExpenseRequest request)
    {
        var result = await _expenseService.UpdateAsync(id, request);
        if (!result.IsSuccess)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _expenseService.DeleteAsync(id);
        if (!result.IsSuccess)
        {
            if (result.Error.StartsWith("Cannot delete", StringComparison.Ordinal))
                return Conflict(result.Error);

            return NotFound(result.Error);
        }

        return NoContent();
    }

    [HttpPost("{expenseId}/items")]
    public async Task<IActionResult> CreateItemAsync(int expenseId, [FromBody] CreateExpenseItemRequest request)
    {
        var result = await _expenseItemService.CreateAsync(expenseId, request);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Created($"/api/v1/expense-items/{result.Value!.Id}", result.Value);
    }

    [HttpGet("{expenseId}/items")]
    public async Task<IActionResult> GetItemsByExpenseIdAsync(
        int expenseId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _expenseItemService.GetAllByExpenseIdAsync(expenseId, page, pageSize);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
