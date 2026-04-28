using AguasNico_Api.Models.DTO.Common;


namespace AguasNico_Api.Models.DTO.Dealers;

public class GetDealerRequest
{
    public string Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
}

public class GetDealerResponse
{
    public DealerItem Dealer { get; set; }
    public int TotalCarts { get; set; }
    public int CompletedCarts { get; set; }
    public int PendingCarts { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalDebt { get; set; }
    public List<ClientStockItem> ClientsStock { get; set; } = [];
}

public class ClientStockItem
{
    public string Product { get; set; }
    public int Stock { get; set; }
}



