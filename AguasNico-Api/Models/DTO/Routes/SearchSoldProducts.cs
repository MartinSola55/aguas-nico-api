using AguasNico_Api.Models.DTO.Common;


namespace AguasNico_Api.Models.DTO.Routes;

public class SearchSoldProductsRequest
{
    public DateTime Date { get; set; }
    public long RouteId { get; set; }
}

public class SearchSoldProductsResponse
{
    public List<SoldProductsItem> Items { get; set; } = [];
}



