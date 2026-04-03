using FluentValidation;

namespace CatalogAPI.Features.ProductAttributes.CreateProductAttribute;

public sealed class ProductAttributeCreateRequestValidator : AbstractValidator<ProductAttributeCreateRequest>
{
    public ProductAttributeCreateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
