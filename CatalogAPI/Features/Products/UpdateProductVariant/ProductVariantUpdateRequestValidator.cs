using FluentValidation;

namespace CatalogAPI.Features.Products.UpdateProductVariant;

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
            .GreaterThanOrEqualTo(0)
            .When(x => x.OverridePrice.HasValue);
        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3);
        RuleFor(x => x.Attributes)
            .NotNull();
        RuleForEach(x => x.Attributes)
            .ChildRules(attribute =>
            {
                attribute.RuleFor(x => x.AttributeId)
                    .NotEmpty();
                attribute.RuleFor(x => x.Value)
                    .NotEmpty()
                    .MaximumLength(500);
            });
        RuleFor(x => x.Attributes)
            .Must(attributes => attributes.Select(attribute => attribute.AttributeId).Distinct().Count() == attributes.Count)
            .When(x => x.Attributes is not null)
            .WithMessage("Variant attribute definitions must be unique.");
    }
}
