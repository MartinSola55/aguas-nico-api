using AguasNico_Api.Models.DTO.Common;


namespace AguasNico_Api.Models.DTO.Abonos;

public class GetAbonoClientsRequest
{
    public long AbonoId { get; set; }
}

public class GetAbonoClientsResponse
{
    public List<ClientSummaryItem> Items { get; set; } = [];
}



