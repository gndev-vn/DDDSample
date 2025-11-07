using Microsoft.EntityFrameworkCore;
using Shared.Common;

namespace Shared.Extensions;

public static class ModelBuilderDomainEventExtensions
{
    public static void IgnoreDomainEvents(this ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<DomainEvent>();

        foreach (var et in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IEntityWithEvents).IsAssignableFrom(et.ClrType))
            {
                modelBuilder.Entity(et.ClrType).Ignore(nameof(IEntityWithEvents.DomainEvents)); // "DomainEvents"
            }
        }
    }
}