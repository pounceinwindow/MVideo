using System.ComponentModel.DataAnnotations;
using MVideo.Api.Models;

namespace MVideo.Api.Contracts;

public class CreateCategoryRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}

public class CreateProductRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(64)]
    public string Sku { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}

public class UpdateProductStatusRequest
{
    [Required]
    public ProductStatus? Status { get; set; }
}

public record CategoryResponse(int Id, string Name);

public record ProductResponse(
    int Id,
    string Name,
    string Sku,
    int CategoryId,
    string CategoryName,
    ProductStatus Status);
