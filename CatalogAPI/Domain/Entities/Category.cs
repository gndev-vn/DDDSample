using CatalogAPI.Domain.Events;
using Shared.Exceptions;
using Shared.Models;

namespace CatalogAPI.Domain.Entities;

public sealed class Category : EntityWithEvents
{
    private readonly List<Product> _products = [];

    public Category()
    {
    }

    public Category(string name, string description, string slug, bool isActive = true, Guid? parentId = null)
    {
        Name = NormalizeRequired(name, nameof(name));
        Description = NormalizeDescription(description);
        Slug = NormalizeRequired(slug, nameof(slug));
        IsActive = isActive;
        ParentId = parentId;

        AddDomainEvent(new CategoryCreatedDomainEvent
        {
            Id = Id,
            Name = Name,
            Slug = Slug,
            ParentId = ParentId ?? Guid.Empty
        });
    }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Guid? ParentId { get; set; }
    public Category? Parent { get; set; }
    public IReadOnlyCollection<Product> Products => _products;
    public IEnumerable<Category>? Children { get; set; }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        RaiseUpdatedDomainEvent();
    }

    public void Activate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        RaiseUpdatedDomainEvent();
    }

    public void ChangeSlug(string newSlug)
    {
        Slug = NormalizeRequired(newSlug, nameof(newSlug));
        RaiseUpdatedDomainEvent();
    }

    public void Rename(string newName)
    {
        Name = NormalizeRequired(newName, nameof(newName));
        RaiseUpdatedDomainEvent();
    }

    public void UpdateDescription(string description)
    {
        Description = NormalizeDescription(description);
        RaiseUpdatedDomainEvent();
    }

    public void AddProduct(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        _products.Add(product);
        RaiseUpdatedDomainEvent();
    }

    public void AddProducts(List<Product> products)
    {
        ArgumentNullException.ThrowIfNull(products);
        _products.AddRange(products);
        RaiseUpdatedDomainEvent();
    }

    public void RemoveProduct(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        if (_products.Remove(product))
        {
            RaiseUpdatedDomainEvent();
        }
    }

    public void ChangeParent(Guid? newParentId)
    {
        if (newParentId == Id)
        {
            throw new BusinessRuleException("Category cannot be its own parent.");
        }

        ParentId = newParentId;
        RaiseUpdatedDomainEvent();
    }

    public void MarkDeleted() => AddDomainEvent(new CategoryDeletedDomainEvent { Id = Id });

    public static Category Create(string name, string description, string slug, bool isActive = true,
        Guid? parentId = null)
        => new(name, description, slug, isActive, parentId);

    public void Update(string name, string description, string slug, bool isActive = true, Guid? parentId = null)
    {
        if (parentId == Id)
        {
            throw new BusinessRuleException("Category cannot be its own parent.");
        }

        Name = NormalizeRequired(name, nameof(name));
        Description = NormalizeDescription(description);
        Slug = NormalizeRequired(slug, nameof(slug));
        IsActive = isActive;
        ParentId = parentId;
        RaiseUpdatedDomainEvent();
    }

    private void RaiseUpdatedDomainEvent()
    {
        AddDomainEvent(new CategoryUpdatedDomainEvent
        {
            Id = Id,
            Name = Name,
            Slug = Slug,
            ParentId = ParentId,
            IsActive = IsActive,
            Description = Description
        });
    }

    private static string NormalizeRequired(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} required", paramName);
        }

        return value.Trim();
    }

    private static string NormalizeDescription(string? value) => value?.Trim() ?? string.Empty;
}
