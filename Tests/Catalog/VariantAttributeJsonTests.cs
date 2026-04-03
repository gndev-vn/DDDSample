using System.Text.Json;
using Shared.ValueObjects;

namespace DDDSample.Tests.Catalog;

public sealed class VariantAttributeJsonTests
{
    [Fact]
    public void Deserialize_FromApiPayload_BindsAttributeIdNameAndValue()
    {
        var colorId = Guid.NewGuid();
        var sizeId = Guid.NewGuid();
        var json = $$"""
                     [
                       { "attributeId": "{{colorId}}", "name": "Color", "value": "Red" },
                       { "attributeId": "{{sizeId}}", "name": "Size", "value": "M" }
                     ]
                     """;

        var attributes = JsonSerializer.Deserialize<List<VariantAttribute>>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(attributes);
        Assert.Collection(attributes!,
            attribute =>
            {
                Assert.Equal(colorId, attribute.AttributeId);
                Assert.Equal("Color", attribute.Name);
                Assert.Equal("Red", attribute.Value);
            },
            attribute =>
            {
                Assert.Equal(sizeId, attribute.AttributeId);
                Assert.Equal("Size", attribute.Name);
                Assert.Equal("M", attribute.Value);
            });
    }
}
