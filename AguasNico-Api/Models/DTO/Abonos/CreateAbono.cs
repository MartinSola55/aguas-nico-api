using AguasNico_Api.Models.Constants;
namespace AguasNico_Api.Models.DTO.Abonos;

public class CreateAbonoRequest
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public List<CreateAbonoProductItem> Products { get; set; } = [];
}

public class CreateAbonoResponse
{
    public long Id { get; set; }
}

public class CreateAbonoProductItem
{
    public ProductType Type { get; set; }
    public int Quantity { get; set; }
}



