namespace AguasNico_Api.Models.DTO.Expenses;

public class SearchExpenseByDateRequest
{
    public DateTime Date { get; set; }
}

public class SearchExpenseByDateResponse
{
    public List<ExpenseByDateItem> Items { get; set; } = [];

    public class ExpenseByDateItem
    {
        public string Dealer { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }
}