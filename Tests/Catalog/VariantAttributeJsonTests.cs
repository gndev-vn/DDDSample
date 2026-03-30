using System.Text.Json;
using Shared.ValueObjects;

namespace DDDSample.Tests.Catalog;

public sealed class VariantAttributeJsonTests
{
    [Fact]
    public void Deserialize_FromApiPayload_BindsNameAndValue()
    {
        const string json = """
                            [
                              { "name": "Color", "value": "Red" },
                              { "name": "Size", "value": "M" }
                            ]
                            """;

        var attributes = JsonSerializer.Deserialize<List<VariantAttribute>>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(attributes);
        Assert.Collection(attributes!,
            attribute =>
            {
                Assert.Equal("Color", attribute.Name);
                Assert.Equal("Red", attribute.Value);
            },
            attribute =>
            {
                Assert.Equal("Size", attribute.Name);
                Assert.Equal("M", attribute.Value);
            });
    }
}
