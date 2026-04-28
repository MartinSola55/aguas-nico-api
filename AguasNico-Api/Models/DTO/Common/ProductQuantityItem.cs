using AguasNico_Api.Models.Constants;

namespace AguasNico_Api.Models.DTO.Common;

public class ProductQuantityItem
{
    public ProductType Type { get; set; }
    public string TypeName { get; set; }
    public int Quantity { get; set; }
    public decimal SettedPrice { get; set; }
}


