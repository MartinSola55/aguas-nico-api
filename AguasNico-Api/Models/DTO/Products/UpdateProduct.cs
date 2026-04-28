namespace AguasNico_Api.Models.DTO.Products;

public class UpdateProductRequest : CreateProductRequest
{
    public long Id { get; set; }
}

public class UpdateProductResponse
{
    public long Id { get; set; }
}



