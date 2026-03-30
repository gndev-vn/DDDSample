using Microsoft.EntityFrameworkCore;
using PaymentAPI.Domain.Entities;
using Shared.Extensions;

namespace PaymentAPI.Domain;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        builder.IgnoreDomainEvents();
    }
}
