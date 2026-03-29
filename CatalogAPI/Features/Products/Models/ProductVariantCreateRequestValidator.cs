using FluentValidation;

namespace CatalogAPI.Features.Products.Models;

public sealed class ProductVariantCreateRequestValidator : AbstractValidator<ProductVariantCreateRequest>
{
    public ProductVariantCreateRequestValidator()
    {
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

        RuleFor(x => x.Attributes)
            .NotNull();

        RuleForEach(x => x.Attributes)
            .Must(attribute => !string.IsNullOrWhiteSpace(attribute.Name))
            .WithMessage("Variant attribute name is required.")
            .Must(attribute => !string.IsNullOrWhiteSpace(attribute.Value))
            .WithMessage("Variant attribute value is required.");

        RuleFor(x => x.Attributes)
            .Must(attributes => attributes.Select(attribute => attribute.Name.Trim().ToUpperInvariant()).Distinct().Count() == attributes.Count)
            .When(x => x.Attributes is not null)
            .WithMessage("Variant attribute names must be unique.");
    }
}
