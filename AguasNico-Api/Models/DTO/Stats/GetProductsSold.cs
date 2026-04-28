namespace AguasNico_Api.Models.DTO.Stats;

public class GetProductsSoldRequest
{
    public int Year { get; set; }
    public int Month { get; set; }
}

public class GetProductsSoldResponse
{
    public List<ProductSoldItem> Items { get; set; } = [];
}

public class ProductSoldItem
{
    public string Type { get; set; }
    public int Quantity { get; set; }
}



