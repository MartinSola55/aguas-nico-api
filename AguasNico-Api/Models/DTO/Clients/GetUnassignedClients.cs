using AguasNico_Api.Models.DTO.Common;
namespace AguasNico_Api.Models.DTO.Clients;

public class GetUnassignedClientsResponse
{
    public List<ClientSummaryItem> Items { get; set; } = [];
}



