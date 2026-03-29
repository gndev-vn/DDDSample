using FluentValidation;

namespace CatalogAPI.Features.Products.Models;

public sealed class ProductVariantUpdateRequestValidator : AbstractValidator<ProductVariantUpdateRequest>
{
    public ProductVariantUpdateRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(4000);

        RuleFor(x => x.ParentId)
            .NotEmpty();

        RuleFor(x => x.OverridePrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3);
    }
}
