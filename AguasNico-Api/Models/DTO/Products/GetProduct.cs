using AguasNico_Api.Models.DTO.Common;
namespace AguasNico_Api.Models.DTO.Products;

public class GetProductRequest
{
    public long Id { get; set; }
}

public class GetProductResponse : ProductItem
{
}



