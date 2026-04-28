using AguasNico_Api.Models.Constants;
namespace AguasNico_Api.Models.DTO.Clients;

public class GetProductsAndAbonoRequest
{
    public long Id { get; set; }
}

public class GetProductsAndAbonoResponse
{
    public List<ProductOptionItem> Products { get; set; } = [];
    public List<AbonoProductOptionItem> AbonoProducts { get; set; } = [];

    public class ProductOptionItem
    {
        public ProductType Type { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class AbonoProductOptionItem
    {
        public ProductType Type { get; set; }
        public string Name { get; set; }
        public int Available { get; set; }
    }
}



