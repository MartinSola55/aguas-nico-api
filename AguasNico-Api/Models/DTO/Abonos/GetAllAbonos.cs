using AguasNico_Api.Models.Constants;
namespace AguasNico_Api.Models.DTO.Abonos;

public class GetAllAbonosResponse
{
    public List<AbonoItem> Items { get; set; } = [];
}

public class AbonoItem
{
    public long Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public List<AbonoProductItem> Products { get; set; } = [];
}

public class AbonoProductItem
{
    public ProductType Type { get; set; }
    public string TypeName { get; set; }
    public int Quantity { get; set; }
}



