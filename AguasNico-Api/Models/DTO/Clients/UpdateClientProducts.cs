namespace AguasNico_Api.Models.DTO.Clients;

public class UpdateClientProductsRequest
{
    public long ClientId { get; set; }
    public List<ClientProductRequestItem> Products { get; set; } = [];
}



