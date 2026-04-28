namespace AguasNico_Api.Models.DTO.Clients;

public class GetProductsHistoryRequest
{
    public long Id { get; set; }
}

public class GetProductsHistoryResponse
{
    public List<ProductHistoryItem> Items { get; set; } = [];
}



