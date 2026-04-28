using AguasNico_Api.Models.DTO.Common;
namespace AguasNico_Api.Models.DTO.Products;

public class GetAllProductsRequest
{
    public bool ActiveOnly { get; set; }
}

public class GetAllProductsResponse
{
    public List<ProductItem> Items { get; set; } = [];
}



