namespace AguasNico_Api.Models.DTO.Transfers;

public class GetTransfersRequest
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public string UserId { get; set; }
}

public class GetTransfersResponse
{
    public List<TransferItem> Items { get; set; } = [];
}



