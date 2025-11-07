using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderingAPI.Domain.Entities;

namespace OrderingAPI.Domain.Configurations;

public class ProductCacheConfiguration : IEntityTypeConfiguration<ProductCache>
{
    private const string TableName = "ProductCaches";
    private const string SchemaName = "ordering";
    private const int SkuMaxLength = 100;
    private const int NameMaxLength = 300;
    
    public void Configure(EntityTypeBuilder<ProductCache> builder)
    {
        builder.ToTable(TableName, SchemaName);
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Sku)
            .HasMaxLength(SkuMaxLength)
            .IsRequired();
        
        builder.Property(x => x.Name)
            .HasMaxLength(NameMaxLength)
            .IsRequired();

        // Add LastUpdated for cache invalidation
        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        // Indexes for efficient lookups
        builder.HasIndex(x => x.Sku)
            .IsUnique();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.UpdatedAtUtc);
    }
}