using CatalogAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogAPI.Domain.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    private const string TableName = "Products";
    private const int NameMaxLength = 200;
    private const int SlugMaxLength = 200;
    private const int CurrencyMaxLength = 3;
    private const int ImageUrlMaxLength = 2048;

    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Table and key
        builder.ToTable(TableName);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        // Required simple properties
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(NameMaxLength);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(SlugMaxLength);

        builder.HasIndex(x => x.Slug).IsUnique();

        builder.Property(x => x.Description)
            .HasMaxLength(int.MaxValue); // or choose a bounded length appropriate for your DB

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(ImageUrlMaxLength);

        builder.Property(x => x.IsActive).IsRequired();

        // Owned value object: BasePrice
        builder.OwnsOne(x => x.BasePrice, price =>
        {
            price.Property(p => p.Amount)
                .HasColumnName("BasePrice")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            price.Property(p => p.Currency)
                .HasConversion<string>()
                .HasColumnName("BasePriceCurrency")
                .HasMaxLength(CurrencyMaxLength)
                .IsRequired();
        }).Navigation(x => x.BasePrice).IsRequired();

        // Backing field for Variants and relationship
        builder.Navigation(x => x.Variants).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Variants)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}