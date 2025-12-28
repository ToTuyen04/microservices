
namespace BusinessLogicLayer.DTO;
public record ProductAddRequest(
    string? ProductName,
    CategoryOptions Category,
    double? UnitPrice,
    int? QuantityInStock
    )
{
    public ProductAddRequest() : this(null, default, null, null)
    {
    }
}

