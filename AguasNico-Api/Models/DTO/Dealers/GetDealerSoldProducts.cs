using AguasNico_Api.Models.DTO.Common;


namespace AguasNico_Api.Models.DTO.Dealers;

public class GetDealerSoldProductsRequest
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public string DealerId { get; set; }
}

public class GetDealerSoldProductsResponse
{
    public List<SoldProductsItem> Items { get; set; } = [];
}



