//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Categories;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Routes;

[ApiController]
[Route("api/v1/categories")]
public class CategoryRoutes(ICategoryService categoryService) : ControllerBase
{
    private readonly ICategoryService _categoryService = categoryService;

    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _categoryService.GetAllAsync(page, pageSize);
        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        if (!result.IsSuccess)
            return NotFound(new { statusCode = 404, message = result.Error, error = Array.Empty<string>() });

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateCategoryRequest request)
    {
        var result = await _categoryService.CreateAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { statusCode = 400, message = result.Error, error = Array.Empty<string>() });

        return CreatedAtAction("GetById", new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateCategoryRequest request)
    {
        var result = await _categoryService.UpdateAsync(id, request);
        if (!result.IsSuccess)
            return NotFound(new { statusCode = 404, message = result.Error, error = Array.Empty<string>() });

        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _categoryService.DeleteAsync(id);
        if (!result.IsSuccess)
        {
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { statusCode = 404, message = result.Error, error = Array.Empty<string>() });

            return Conflict(new { statusCode = 409, message = result.Error, error = Array.Empty<string>() });
        }

        return NoContent();
    }
}
