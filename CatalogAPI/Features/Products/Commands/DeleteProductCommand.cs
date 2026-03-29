using CatalogAPI.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;

namespace CatalogAPI.Features.Products.Commands;

public record DeleteProductCommand(Guid Id) : IRequest<bool>;

public class DeleteProductCommandHandler(AppDbContext dbContext) : IRequestHandler<DeleteProductCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .Include(x => x.Variants)
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (product == null)
        {
            throw new KeyNotFoundException("Invalid product id");
        }

        product.MarkDeleted();
        dbContext.Products.Remove(product);
        return await dbContext.SaveChangesAsync(cancellationToken) > 0;
    }
}
