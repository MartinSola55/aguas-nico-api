using AguasNico_Api.Models.Constants;
namespace AguasNico_Api.Models.DTO.Carts;

public class CartProductRequestItem
{
    public ProductType Type { get; set; }
    public int Quantity { get; set; }
}



