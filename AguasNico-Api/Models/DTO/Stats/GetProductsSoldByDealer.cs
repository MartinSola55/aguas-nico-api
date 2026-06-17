namespace AguasNico_Api.Models.DTO.Stats;

public class GetProductsSoldByDealerRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string DealerId { get; set; }
}

public class GetProductsSoldByDealerResponse
{
    public List<DealerProductsSoldItem> Items { get; set; } = [];
}

public class DealerProductsSoldItem
{
    public string DealerId { get; set; }
    public string DealerName { get; set; }
    public int Quantity { get; set; }
    public List<ProductSoldItem> Products { get; set; } = [];
}
