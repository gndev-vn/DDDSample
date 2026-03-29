using CatalogAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogAPI.Domain.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    private const string TableName = "Categories";
    private const int NameMaxLength = 200;
    private const int SlugMaxLength = 200;
    private const int DescriptionMaxLength = 1000;

    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // Table name and schema
        builder.ToTable(TableName);

        // Primary Key
        builder.HasKey(c => c.Id);

        // Properties
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(NameMaxLength);

        builder.Property(c => c.Description)
            .HasMaxLength(DescriptionMaxLength);

        builder.Property(c => c.Slug)
            .IsRequired()
            .HasMaxLength(SlugMaxLength);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Indexes for performance and constraints
        builder.HasIndex(c => c.Slug)
            .IsUnique()
            .HasFilter($"{nameof(Category.IsActive)} = 1"); // Only enforce uniqueness for active categories

        builder.HasIndex(c => c.Name)
            .HasFilter($"{nameof(Category.IsActive)} = 1"); // Index for searching by name

        builder.HasIndex(c => c.ParentId)
            .HasFilter($"{nameof(Category.IsActive)} = 1"); // Index for hierarchy queries

        // Self-referencing relationship for parent-child categories
        builder.HasOne(c => c.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

        // Products relationship
        builder.HasMany(c => c.Products)
            .WithOne(p => p.Category)
            .HasForeignKey("CategoryId")
            .IsRequired(false)  // Product can exist without a category
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete of products when category is deleted

        // Navigation property configurations
        builder.Metadata
            .FindNavigation(nameof(Category.Products))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(Category.Children))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Ignore domain events collection for DB mapping
        builder.Ignore(c => c.DomainEvents);

        // Query filters
        builder.HasQueryFilter(c => c.IsActive); // Soft delete - only get active categories by default
    }
}