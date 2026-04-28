namespace AguasNico_Api.Models.DTO.Stats;

public class GetAnnualProfitsRequest
{
    public int Year { get; set; }
}

public class GetAnnualProfitsResponse
{
    public List<PeriodProfitItem> Items { get; set; } = [];
}



