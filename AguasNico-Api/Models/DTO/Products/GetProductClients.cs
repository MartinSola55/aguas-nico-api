using AguasNico_Api.Models.DTO.Common;
namespace AguasNico_Api.Models.DTO.Products;

public class GetProductClientsRequest
{
    public long ProductId { get; set; }
}

public class GetProductClientsResponse
{
    public List<ClientSummaryItem> Items { get; set; } = [];
}



