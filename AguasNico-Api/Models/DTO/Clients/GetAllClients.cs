using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO.Common;
namespace AguasNico_Api.Models.DTO.Clients;

public class GetAllClientsRequest
{
    public string Search { get; set; }
    public bool ActiveOnly { get; set; }
    public string DealerId { get; set; }
    public Day DeliveryDay { get; set; }
}

public class GetAllClientsResponse
{
    public List<ClientSummaryItem> Items { get; set; } = [];
}



