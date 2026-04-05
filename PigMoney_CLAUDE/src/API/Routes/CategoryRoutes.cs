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
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateCategoryRequest request)
    {
        var result = await _categoryService.CreateAsync(request);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return CreatedAtAction("GetById", new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateCategoryRequest request)
    {
        var result = await _categoryService.UpdateAsync(id, request);
        if (!result.IsSuccess)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _categoryService.DeleteAsync(id);
        if (!result.IsSuccess)
        {
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(result.Error);

            return Conflict(result.Error);
        }

        return NoContent();
    }
}
