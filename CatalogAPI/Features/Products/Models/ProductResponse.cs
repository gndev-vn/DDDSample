using Shared.Models;

namespace CatalogAPI.Features.Products.Models;

public class ProductResponse : ModelBase
{
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}