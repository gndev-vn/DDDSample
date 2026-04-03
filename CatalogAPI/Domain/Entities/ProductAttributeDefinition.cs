using Shared.Models;

namespace CatalogAPI.Domain.Entities;

public sealed class ProductAttributeDefinition : Entity
{
    public ProductAttributeDefinition()
    {
    }

    public ProductAttributeDefinition(string name)
    {
        Rename(name);
    }

    public string Name { get; private set; } = string.Empty;

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("name required", nameof(name));
        }

        Name = name.Trim();
    }
}
