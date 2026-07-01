namespace MVideo.Api.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public ProductStatus Status { get; set; }
    public Category Category { get; set; } = null!;
}
