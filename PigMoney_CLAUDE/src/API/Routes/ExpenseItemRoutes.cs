//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.ExpenseItems;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Routes;

[ApiController]
[Route("api/v1/expense-items")]
public class ExpenseItemRoutes(IExpenseItemService expenseItemService) : ControllerBase
{
    private readonly IExpenseItemService _expenseItemService = expenseItemService;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetItemByIdAsync(int id)
    {
        var result = await _expenseItemService.GetByIdAsync(id);
        if (!result.IsSuccess)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItemAsync(int id, [FromBody] UpdateExpenseItemRequest request)
    {
        var result = await _expenseItemService.UpdateAsync(id, request);
        if (!result.IsSuccess)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItemAsync(int id)
    {
        var result = await _expenseItemService.DeleteAsync(id);
        if (!result.IsSuccess)
            return NotFound(result.Error);

        return NoContent();
    }
}
