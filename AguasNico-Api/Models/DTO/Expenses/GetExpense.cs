namespace AguasNico_Api.Models.DTO.Expenses;

public class GetExpenseRequest
{
    public long Id { get; set; }
}

public class GetExpenseResponse : ExpenseItem
{
}