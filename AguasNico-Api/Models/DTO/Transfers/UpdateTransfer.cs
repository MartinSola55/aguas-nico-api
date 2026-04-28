namespace AguasNico_Api.Models.DTO.Transfers;

public class UpdateTransferRequest : CreateTransferRequest
{
    public long Id { get; set; }
    public bool UpdateDate { get; set; }
}

public class UpdateTransferResponse
{
    public long Id { get; set; }
}



