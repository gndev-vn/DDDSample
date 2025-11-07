using System.ComponentModel.DataAnnotations.Schema;
using Shared.Common;

namespace OrderingAPI.Domain.Entities;

public class ProductCache : Entity
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentPrice { get; set; }
    public string Currency { get; set; } = "VND";
    public DateTime LastUpdatedUtc { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}