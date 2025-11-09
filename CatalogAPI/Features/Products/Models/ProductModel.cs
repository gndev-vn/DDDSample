using Shared.Models;

namespace CatalogAPI.Features.Products.Models;

public class ProductModel : ModelBase
{
    public MoneyModel? BasePrice { get; set; }

    public bool IsActive { get; set; } = true;

    public string Slug { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}