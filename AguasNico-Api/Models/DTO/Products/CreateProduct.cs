using AguasNico_Api.Models.Constants;
namespace AguasNico_Api.Models.DTO.Products;

public class CreateProductRequest
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public ProductType Type { get; set; }
    public int SortOrder { get; set; }
}

public class CreateProductResponse
{
    public long Id { get; set; }
}



