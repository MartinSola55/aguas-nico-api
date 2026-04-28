using AguasNico_Api.Models.Constants;
namespace AguasNico_Api.Models.DTO.Routes;

public class UpdateDispatchedRequest
{
    public long RouteId { get; set; }
    public List<DispatchedProductRequestItem> Products { get; set; } = [];
}

public class DispatchedProductRequestItem
{
    public ProductType Type { get; set; }
    public int Quantity { get; set; }
}



