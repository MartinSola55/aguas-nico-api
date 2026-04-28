namespace AguasNico_Api.Models.DTO.Expenses;

public class CreateExpenseRequest
{
    public string UserId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
}

public class CreateExpenseResponse
{
    public long Id { get; set; }
}