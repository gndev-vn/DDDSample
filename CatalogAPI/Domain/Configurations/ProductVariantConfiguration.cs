using CatalogAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogAPI.Domain.Configurations;

public sealed class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    private const string TableName = "ProductVariants";
    private const int NameMaxLength = 200;
    private const int SkuMaxLength = 100;
    private const int AttributeNameMaxLength = 100;
    private const int AttributeValueMaxLength = 500;

    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(NameMaxLength);
        builder.Property(x => x.Sku)
            .IsRequired()
            .HasMaxLength(SkuMaxLength);

        builder.Property(x => x.Description)
            .HasMaxLength(int.MaxValue);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasIndex(x => new { x.ProductId, x.Sku }).IsUnique();

        builder.HasOne(x => x.Product)
            .WithMany(x => x.Variants)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(x => x.OverridePrice, price =>
        {
            price.Property(p => p.Amount)
                .HasColumnName("OverridePriceAmount")
                .HasColumnType("numeric(18,2)");

            price.Property(p => p.Currency)
                .HasConversion<string>()
                .HasColumnName("OverridePriceCurrency")
                .HasMaxLength(3);
        });
        builder.Navigation(x => x.OverridePrice).IsRequired(false);

        builder.OwnsMany(x => x.Attributes, attributes =>
        {
            attributes.ToTable("ProductVariantAttributes");
            attributes.WithOwner().HasForeignKey("ProductVariantId");
            attributes.HasKey("Id");

            attributes.Property(p => p.AttributeId)
                .IsRequired();

            attributes.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(AttributeNameMaxLength);

            attributes.Property(p => p.Value)
                .IsRequired()
                .HasMaxLength(AttributeValueMaxLength);

            attributes.HasIndex("ProductVariantId", "AttributeId").IsUnique();
        });

        builder.Navigation(x => x.Attributes).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
