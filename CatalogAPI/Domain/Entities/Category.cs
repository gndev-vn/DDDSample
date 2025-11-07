using CatalogAPI.Domain.Events;
using Shared.Common;

namespace CatalogAPI.Domain.Entities;

public class Category : EntityWithEvents
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Slug { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? ParentId { get; set; }
    public Category? Parent { get; set; }

    private readonly List<Product> _products = [];
    public IReadOnlyCollection<Product> Products => _products;
    public IEnumerable<Category>? Children { get; set; }

    public Category(string name, string description, string slug, bool isActive = true, Guid? parentId = null)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ArgumentException("slug required");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("name required");
        }

        Name = name;
        Description = description;
        Slug = slug;
        IsActive = isActive;
        ParentId = parentId;
        AddDomainEvent(new CategoryCreatedDomainEvent { Id = Id, Name = name });
    }

    public Category()
    {
    }

    public void Deactivate()
    {
        IsActive = false;
        AddDomainEvent(new CategoryUpdatedDomainEvent { Id = Id, IsActive = false });
    }

    public void ChangeSlug(string newSlug)
    {
        Slug = string.IsNullOrWhiteSpace(newSlug) ? Name : newSlug.Trim();
        AddDomainEvent(new CategoryUpdatedDomainEvent { Id = Id, Slug = newSlug });
    }

    public void Rename(string newName)
    {
        Name = string.IsNullOrWhiteSpace(newName) ? Name : newName.Trim();
        AddDomainEvent(new CategoryUpdatedDomainEvent { Id = Id, Name = newName });
    }

    public void UpdateDescription(string description)
    {
        Description = description;
        AddDomainEvent((new CategoryUpdatedDomainEvent { Id = Id, Description = description }));
    }

    public void AddProduct(Product product)
    {
        _products.Add(product);
        AddDomainEvent(new CategoryUpdatedDomainEvent { Id = Id });
    }

    public void AddProducts(List<Product> products)
    {
        _products.AddRange(products);
        AddDomainEvent(new CategoryUpdatedDomainEvent { Id = Id });
    }

    public void RemoveProduct(Product product)
    {
        if (_products.Remove(product))
        {
            AddDomainEvent(new CategoryUpdatedDomainEvent { Id = Id });
        }
    }

    public void ChangeParent(Guid? newParentId)
    {
        ParentId = newParentId;
        AddDomainEvent(new CategoryUpdatedDomainEvent { Id = Id, ParentId = newParentId });
    }

    public static Category Create(string name, string description, string slug, bool isActive = true,
        Guid? parentId = null)
        => new(name, description, slug, isActive, parentId);

    public void Update(string name, string description, string slug, bool isActive = true, Guid? parentId = null)
    {
        Name = name;
        Description = description;
        Slug = slug;
        IsActive = isActive;
        ParentId = parentId;
        AddDomainEvent(new CategoryUpdatedDomainEvent { Id = Id });
    }
}