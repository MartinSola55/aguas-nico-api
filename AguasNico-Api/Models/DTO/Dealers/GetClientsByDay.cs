using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO.Common;


namespace AguasNico_Api.Models.DTO.Dealers;

public class GetClientsByDayRequest
{
    public Day Day { get; set; }
    public string DealerId { get; set; }
}

public class GetClientsByDayResponse
{
    public List<ClientSummaryItem> Items { get; set; } = [];
}


