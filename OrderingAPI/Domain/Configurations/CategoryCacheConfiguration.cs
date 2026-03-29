using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderingAPI.Domain.Entities;

namespace OrderingAPI.Domain.Configurations;

public class CategoryCacheConfiguration : IEntityTypeConfiguration<CategoryCache>
{
    private const string TableName = "CategoryCaches";
    private const int NameMaxLength = 300;

    public void Configure(EntityTypeBuilder<CategoryCache> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(NameMaxLength)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.ParentId);
    }
}