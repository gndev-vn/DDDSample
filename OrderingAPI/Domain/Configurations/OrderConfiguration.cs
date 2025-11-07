using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderingAPI.Domain.Entities;

namespace OrderingAPI.Domain.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    private const string TableName = "Orders";
    private const string SchemaName = "ordering";
    private const int AddressLineMaxLength = 200;
    private const int AddressWardMaxLength = 100;
    private const int AddressDistrictMaxLength = 100;
    private const int AddressCityMaxLength = 100;
    private const int AddressProvinceMaxLength = 100;

    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable(TableName, SchemaName);
        builder.HasKey(x => x.Id);

        // Status and OrderDate
        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>();

        // Shipping Address value object
        builder.OwnsOne(x => x.ShippingAddress, m =>
        {
            m.Property(p => p.Line1)
                .HasColumnName("ShippingAddressLine1")
                .HasMaxLength(AddressLineMaxLength)
                .IsRequired();

            m.Property(p => p.Line2)
                .HasColumnName("ShippingAddressLine2")
                .HasMaxLength(AddressLineMaxLength);

            m.Property(p => p.Ward)
                .HasColumnName("ShippingAddressWard")
                .HasMaxLength(AddressWardMaxLength);

            m.Property(p => p.District)
                .HasColumnName("ShippingAddressDistrict")
                .HasMaxLength(AddressDistrictMaxLength)
                .IsRequired();

            m.Property(p => p.City)
                .HasColumnName("ShippingAddressCity")
                .HasMaxLength(AddressCityMaxLength)
                .IsRequired();

            m.Property(p => p.Province)
                .HasColumnName("ShippingAddressProvince")
                .HasMaxLength(AddressProvinceMaxLength)
                .IsRequired();
        });

        // Billing Address value object (optional)
        builder.OwnsOne(x => x.BillingAddress, m =>
        {
            m.Property(p => p.Line1)
                .HasColumnName("BillingAddressLine1")
                .HasMaxLength(AddressLineMaxLength)
                .IsRequired();

            m.Property(p => p.Line2)
                .HasColumnName("BillingAddressLine2")
                .HasMaxLength(AddressLineMaxLength);

            m.Property(p => p.Ward)
                .HasColumnName("BillingAddressWard")
                .HasMaxLength(AddressWardMaxLength);

            m.Property(p => p.District)
                .HasColumnName("BillingAddressDistrict")
                .HasMaxLength(AddressDistrictMaxLength)
                .IsRequired();

            m.Property(p => p.City)
                .HasColumnName("BillingAddressCity")
                .HasMaxLength(AddressCityMaxLength)
                .IsRequired();

            m.Property(p => p.Province)
                .HasColumnName("BillingAddressProvince")
                .HasMaxLength(AddressProvinceMaxLength)
                .IsRequired();
        });

        // Navigation and relationship configuration
        builder.Metadata
            .FindNavigation(nameof(Order.Lines))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder
            .HasMany(x => x.Lines)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for common queries
        builder.HasIndex(x => x.Status);
    }
}