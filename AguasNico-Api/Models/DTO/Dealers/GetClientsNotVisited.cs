using AguasNico_Api.Models.DTO.Common;


namespace AguasNico_Api.Models.DTO.Dealers;

public class GetClientsNotVisitedRequest
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public string DealerId { get; set; }
}

public class GetClientsNotVisitedResponse
{
    public int TotalClients { get; set; }
    public int TotalNotVisited { get; set; }
    public List<ClientSummaryItem> Clients { get; set; } = [];
}



