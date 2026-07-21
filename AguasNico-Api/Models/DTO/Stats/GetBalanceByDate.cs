namespace AguasNico_Api.Models.DTO.Stats;

public class GetBalanceByDateRequest
{
    public DateTime Date { get; set; }
}

public class GetBalanceByDateResponse
{
    public decimal Total { get; set; }
    public decimal Cash { get; set; }
    public decimal MercadoPago { get; set; }
    public decimal Transfers { get; set; }
    public decimal Expenses { get; set; }
    public decimal DispenserPrice { get; set; }
}



