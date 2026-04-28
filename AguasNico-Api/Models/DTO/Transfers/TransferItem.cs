namespace AguasNico_Api.Models.DTO.Transfers;

public class TransferItem
{
    public long Id { get; set; }
    public long ClientId { get; set; }
    public string ClientName { get; set; }
    public string UserId { get; set; }
    public string DealerName { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; }
}



