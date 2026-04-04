using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderingAPI.Domain.Entities;

namespace OrderingAPI.Domain.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    private const string TableName = "Customers";

    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.DisplayName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(32);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();
    }
}
