using AguasNico_Api.Models.DTO.Common;


namespace AguasNico_Api.Models.DTO.Carts;

public class GetReturnedProductsRequest
{
    public long CartId { get; set; }
}

public class GetReturnedProductsResponse
{
    public List<ProductQuantityItem> Items { get; set; } = [];
}



