namespace AguasNico_Api.Models.DTO.Expenses;

public class GetExpensesRequest
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public string UserId { get; set; }
}

public class GetExpensesResponse
{
    public List<ExpenseItem> Items { get; set; } = [];
}