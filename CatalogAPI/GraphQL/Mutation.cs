using CatalogAPI.Features.Categories.Commands;
using CatalogAPI.Features.Categories.Models;
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