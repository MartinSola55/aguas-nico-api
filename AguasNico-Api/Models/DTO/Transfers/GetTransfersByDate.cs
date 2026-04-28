namespace AguasNico_Api.Models.DTO.Transfers;

public class GetTransfersByDateRequest
{
    public DateTime Date { get; set; }
}

public class GetTransfersByDateResponse
{
    public List<TransferItem> Items { get; set; } = [];
}



