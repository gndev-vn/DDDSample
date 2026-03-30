using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain.Entities;
using Shared.Extensions;

namespace OrderingAPI.Domain;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<ProductCache> ProductCaches => Set<ProductCache>();
    public DbSet<CategoryCache> CategoryCaches => Set<CategoryCache>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        builder.IgnoreDomainEvents();
    }
}