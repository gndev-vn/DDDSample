using OrderingAPI.Features.Orders.Models;
using Shared.Models;

namespace DDDSample.Tests.Ordering;

public sealed class CreateOrderModelValidatorTests
{
    [Fact]
    public void Validate_WithMissingShippingAddress_Fails()
    {
        var validator = new CreateOrderModelValidator();
        var model = new CreateOrderModel
        {
            CustomerId = Guid.NewGuid(),
            ShippingAddress = null!,
            Lines =
            [
                new OrderLineModel
                {
                    Sku = "SKU-1",
                    Quantity = 1,
                    UnitPrice = 10m,
                    Currency = "USD"
                }
            ]
        };

        var result = validator.Validate(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "ShippingAddress");
    }
}
