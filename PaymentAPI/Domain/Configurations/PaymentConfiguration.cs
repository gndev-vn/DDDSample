using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentAPI.Domain.Entities;

namespace PaymentAPI.Domain.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    private const string TableName = "Payments";
    private const int CurrencyMaxLength = 3;
    private const int TransactionReferenceMaxLength = 200;
    private const int FailureReasonMaxLength = 500;

    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.OrderId)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(x => x.TransactionReference)
            .HasMaxLength(TransactionReferenceMaxLength);

        builder.Property(x => x.FailureReason)
            .HasMaxLength(FailureReasonMaxLength);

        builder.Property(x => x.CompletedAtUtc);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();

        builder.OwnsOne(x => x.Amount, amount =>
        {
            amount.Property(p => p.Amount)
                .HasColumnName("Amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            amount.Property(p => p.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(CurrencyMaxLength)
                .IsRequired();
        }).Navigation(x => x.Amount).IsRequired();

        builder.HasIndex(x => x.OrderId)
            .IsUnique();
        builder.HasIndex(x => x.Status);
    }
}
