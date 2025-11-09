using CatalogAPI.Features.Categories.Models;
using CatalogAPI.Features.Categories.Queries;
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