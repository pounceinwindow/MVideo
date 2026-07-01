using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVideo.Api.Contracts;
using MVideo.Api.Data;
using MVideo.Api.Models;

namespace MVideo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var categories = await dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.Id)
            .Select(category => new CategoryResponse(category.Id, category.Name))
            .ToListAsync(cancellationToken);

        return Ok(categories);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create(
        CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();

        if (name.Length == 0)
        {
            return Problem(
                title: "Invalid category name",
                detail: "Category name must not consist only of whitespace.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (await dbContext.Categories.AnyAsync(
                category => category.Name == name,
                cancellationToken))
        {
            return Problem(
                title: "Category already exists",
                detail: $"Category with name '{name}' already exists.",
                statusCode: StatusCodes.Status409Conflict);
        }

        var category = new Category { Name = name };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new CategoryResponse(category.Id, category.Name);
        return StatusCode(StatusCodes.Status201Created, response);
    }
}
