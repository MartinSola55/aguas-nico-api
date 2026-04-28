namespace AguasNico_Api.Models.DTO.Stats;

public class GetMonthlyProfitsRequest
{
    public int Year { get; set; }
    public int Month { get; set; }
}

public class GetMonthlyProfitsResponse
{
    public decimal Total { get; set; }
    public List<PeriodProfitItem> Daily { get; set; } = [];
}



