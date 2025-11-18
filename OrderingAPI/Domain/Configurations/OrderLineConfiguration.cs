using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderingAPI.Domain.Entities;

namespace OrderingAPI.Domain.Configurations;

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    private const string TableName = "OrderLines";
    private const string SchemaName = "ordering";
    private const int SkuMaxLength = 100;
    
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable(TableName, SchemaName);
        builder.HasKey(x => x.Id);

        // Quantity value object
        builder.OwnsOne(x => x.Quantity, m =>
        {
            m.Property(p => p.Value)
                .HasColumnName("Quantity")
                .IsRequired()
                .HasColumnType("int");
        });

        // SKU value object
        builder.OwnsOne(x => x.Sku, m =>
        {
            m.Property(p => p.Value)
                .HasColumnName("SkuValue")
                .IsRequired()
                .HasMaxLength(SkuMaxLength);
        }).Navigation(x => x.Sku).IsRequired();

        // Unit Price value object
        builder.OwnsOne(x => x.Total, m =>
        {
            m.Property(p => p.Amount)
                .HasColumnName("PriceAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            m.Property(p => p.Currency)
                .HasConversion<string>()
                .HasColumnName("PriceCurrency")
                .HasMaxLength(3)
                .IsRequired();
        }).Navigation(x => x.Total).IsRequired();

        // Navigation and relationship configuration
        builder.Metadata
            .FindNavigation(nameof(OrderLine.Order))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder
            .HasOne(x => x.Order)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}