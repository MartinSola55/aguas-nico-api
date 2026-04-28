namespace AguasNico_Api.Models.DTO.Carts;

public class ReturnProductsRequest
{
    public long CartId { get; set; }
    public List<ReturnedProductRequestItem> Products { get; set; } = [];
}



