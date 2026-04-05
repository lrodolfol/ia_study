//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Accounts;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Routes;

[ApiController]
[Route("api/v1/accounts")]
public class AccountRoutes(IAccountService accountService) : ControllerBase
{
    private readonly IAccountService _accountService = accountService;

    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _accountService.GetAllAsync(page, pageSize);
        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _accountService.GetByIdAsync(id);
        if (!result.IsSuccess)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateAccountRequest request)
    {
        var result = await _accountService.CreateAsync(request);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return CreatedAtAction("GetById", new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateAccountRequest request)
    {
        var result = await _accountService.UpdateAsync(id, request);
        if (!result.IsSuccess)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _accountService.DeleteAsync(id);
        if (!result.IsSuccess)
        {
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(result.Error);

            return Conflict(result.Error);
        }

        return NoContent();
    }
}
