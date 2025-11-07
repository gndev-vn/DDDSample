using CatalogAPI.Features.Category.Models;
using CatalogAPI.Features.Category.Queries;
using Mediator;

namespace CatalogAPI.GraphQL;

public class Query
{
    public async Task<IEnumerable<CategoryModel>> GetCategories(IMediator mediator)
    {
        var result = await mediator.Send(new GetCategoriesQuery());
        return result;
    }
}