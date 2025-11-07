using CatalogAPI.Features.Category.Commands;
using CatalogAPI.Features.Category.Models;
using CatalogAPI.Features.Category.Queries;
using Grpc.Core;
using Mapster;
using Mediator;
using Shared.Extensions;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;

namespace CatalogAPI.Services.Grpc;

public class CategoryGrpcService(IMediator mediator, ILogger<CategoryGrpcService> logger)
    : CategorySvc.CategorySvcBase
{
    public override async Task<GetCategoriesResponse> GetCategories(GetCategoriesRequest request,
        ServerCallContext context)
    {
        var categories = await mediator.Send(
            new GetCategoriesQuery(request.SearchName, request.SearchSlug, request.Page, request.PageSize),
            context.CancellationToken);
        return new GetCategoriesResponse
        {
            Categories =
            {
                categories.Adapt<List<CategoryDto>>()
            }
        };
    }

    public override async Task<GetCategoryResponse?> GetById(GetCategoryRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.Id, out var id))
            {
                throw new ArgumentException("Invalid category id");
            }

            var category = await mediator.Send(new GetCategoryByIdQuery(id),
                context.CancellationToken);
            if (category == null)
            {
                throw new KeyNotFoundException("Category not found");
            }

            return new GetCategoryResponse
            {
                Category = category.Adapt<CategoryDto>()
            };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "GetById failed for {Id}", request.Id);
            throw ex.ToRpcException("catalog.category.get_by_id_failed");
        }
    }

    public override async Task<CreateCategoryResponse?> Create(CreateCategoryRequest request, ServerCallContext context)
    {
        var category =
            await mediator.Send(new CreateCategoryCommand(request.Adapt<CategoryModel>()),
                context.CancellationToken);

        return new CreateCategoryResponse
        {
            Id = category.Id.ToString()
        };
    }

    public override async Task<UpdateCategoryResponse> Update(UpdateCategoryRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out _))
        {
            throw new ArgumentException("Invalid category id");
        }

        var category = await mediator.Send(new GetCategoryByIdQuery(Guid.Parse(request.Id)),
            context.CancellationToken);
        if (category == null)
        {
            throw new KeyNotFoundException("Category not found");
        }

        var cmd = new UpdateCategoryCommand(request.Adapt<CategoryModel>());

        var updated = await mediator.Send(cmd, context.CancellationToken);
        return new UpdateCategoryResponse { Id = updated.Id.ToString() };
    }

    public override async Task<DeleteCategoryResponse> Delete(DeleteCategoryRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out _))
        {
            throw new ArgumentException("Invalid category id");
        }

        var result = await mediator.Send(new DeleteCategoryCommand(Guid.Parse(request.Id)),
            context.CancellationToken);

        return new DeleteCategoryResponse
        {
            Success = result
        };
    }
}