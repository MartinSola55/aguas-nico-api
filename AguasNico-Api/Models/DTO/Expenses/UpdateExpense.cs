namespace AguasNico_Api.Models.DTO.Expenses;

public class UpdateExpenseRequest : CreateExpenseRequest
{
    public long Id { get; set; }
}

public class UpdateExpenseResponse
{
    public long Id { get; set; }
}



