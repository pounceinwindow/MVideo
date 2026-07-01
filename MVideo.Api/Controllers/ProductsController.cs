using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVideo.Api.Contracts;
using MVideo.Api.Data;
using MVideo.Api.Models;

namespace MVideo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetAll(
        [FromQuery] ProductStatus? status,
        [FromQuery] int? categoryId,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Products.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(product => product.Status == status.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(product => product.CategoryId == categoryId.Value);
        }

        var products = await query
            .OrderBy(product => product.Id)
            .Select(product => new ProductResponse(
                product.Id,
                product.Name,
                product.Sku,
                product.CategoryId,
                product.Category.Name,
                product.Status))
            .ToListAsync(cancellationToken);

        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .Where(product => product.Id == id)
            .Select(product => new ProductResponse(
                product.Id,
                product.Name,
                product.Sku,
                product.CategoryId,
                product.Category.Name,
                product.Status))
            .SingleOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            return ProductNotFound(id);
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var sku = request.Sku.Trim();

        if (name.Length == 0)
        {
            return Problem(
                title: "Invalid product name",
                detail: "Product name must not consist only of whitespace.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (sku.Length == 0)
        {
            return Problem(
                title: "Invalid SKU",
                detail: "SKU must not consist only of whitespace.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var category = await dbContext.Categories
            .AsNoTracking()
            .SingleOrDefaultAsync(
                category => category.Id == request.CategoryId,
                cancellationToken);

        if (category is null)
        {
            return Problem(
                title: "Category not found",
                detail: $"Category with ID {request.CategoryId} does not exist.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (await dbContext.Products.AnyAsync(product => product.Sku == sku, cancellationToken))
        {
            return Problem(
                title: "SKU already exists",
                detail: $"Product with SKU '{sku}' already exists.",
                statusCode: StatusCodes.Status409Conflict);
        }

        var product = new Product
        {
            Name = name,
            Sku = sku,
            CategoryId = category.Id,
            Status = ProductStatus.Active
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = ToResponse(product, category.Name);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, response);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<ProductResponse>> UpdateStatus(
        int id,
        UpdateProductStatusRequest request,
        CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .Include(product => product.Category)
            .SingleOrDefaultAsync(product => product.Id == id, cancellationToken);

        if (product is null)
        {
            return ProductNotFound(id);
        }

        var newStatus = request.Status!.Value;

        if (!IsAllowedTransition(product.Status, newStatus))
        {
            return Problem(
                title: "Status transition is not allowed",
                detail: $"Product status cannot be changed from '{product.Status}' to '{newStatus}'.",
                statusCode: StatusCodes.Status409Conflict);
        }

        product.Status = newStatus;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(product, product.Category.Name));
    }

    private static bool IsAllowedTransition(ProductStatus currentStatus, ProductStatus newStatus)
    {
        return (currentStatus, newStatus) switch
        {
            (ProductStatus.Active, ProductStatus.Defective) => true,
            (ProductStatus.Defective, ProductStatus.WriteOff) => true,
            _ => false
        };
    }

    private static ProductResponse ToResponse(Product product, string categoryName)
    {
        return new ProductResponse(
            product.Id,
            product.Name,
            product.Sku,
            product.CategoryId,
            categoryName,
            product.Status);
    }

    private ObjectResult ProductNotFound(int id)
    {
        return Problem(
            title: "Product not found",
            detail: $"Product with ID {id} was not found.",
            statusCode: StatusCodes.Status404NotFound);
    }
}
