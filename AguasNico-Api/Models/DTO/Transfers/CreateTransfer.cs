namespace AguasNico_Api.Models.DTO.Transfers;

public class CreateTransferRequest
{
    public long ClientId { get; set; }
    public string UserId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}

public class CreateTransferResponse
{
    public long Id { get; set; }
}



