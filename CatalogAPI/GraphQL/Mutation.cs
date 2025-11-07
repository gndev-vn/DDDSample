using CatalogAPI.Features.Category.Commands;
using CatalogAPI.Features.Category.Models;
using Mediator;

namespace CatalogAPI.GraphQL;

public class Mutation
{
    public async Task<CategoryModel> CreateCategory(CategoryModel category, IMediator mediator)
    {
        var result = await mediator.Send(new CreateCategoryCommand(category));
        return result;
    }
}