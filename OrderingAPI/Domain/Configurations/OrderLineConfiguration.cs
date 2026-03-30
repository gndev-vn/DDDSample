using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderingAPI.Domain.Entities;

namespace OrderingAPI.Domain.Configurations;

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    private const string TableName = "OrderLines";
    private const int SkuMaxLength = 100;
    
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

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
        // Note: OrderLine.Order has no backing field, so Field access mode must not be used here.
        // The relationship is also configured from the Order side in OrderConfiguration;
        // this side sets the FK and delete behavior only.
        builder
            .HasOne(x => x.Order)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
