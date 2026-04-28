namespace AguasNico_Api.Models.DTO.Expenses;

public class ExpenseItem
{
    public long Id { get; set; }
    public string UserId { get; set; }
    public string DealerName { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
}