using System.Text.Json;
using Microservices.Shared.Contracts.Products;

namespace Microservices.Shared.Contracts.UnitTests;

public class ProductDtoTests
{
    // Other services read this JSON, so the property names are part of the contract.
    [Fact]
    public void Serializes_to_the_json_property_names_other_services_expect()
    {
        var dto = new ProductDto { Id = 1, Name = "Laptop", Price = 65000m, StockQuantity = 25 };

        var json = JsonSerializer.Serialize(dto, JsonSerializerOptions.Web);
        using var document = JsonDocument.Parse(json);
        var names = document.RootElement.EnumerateObject().Select(p => p.Name).ToArray();

        Assert.Equal(["id", "name", "description", "price", "stockQuantity", "createdAt", "updatedAt"], names);
    }

    [Fact]
    public void Deserializes_the_json_returned_by_ProductService()
    {
        const string json = """
            {"id":1,"name":"Laptop","description":null,"price":65000.00,"stockQuantity":25,
             "createdAt":"2026-10-15T18:41:11.0929027Z","updatedAt":null}
            """;

        var dto = JsonSerializer.Deserialize<ProductDto>(json, JsonSerializerOptions.Web);

        Assert.NotNull(dto);
        Assert.Equal(1, dto.Id);
        Assert.Equal("Laptop", dto.Name);
        Assert.Equal(65000.00m, dto.Price);
        Assert.Null(dto.UpdatedAt);
    }
}
