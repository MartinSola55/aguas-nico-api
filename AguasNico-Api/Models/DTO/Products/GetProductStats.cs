using AguasNico_Api.Models.DTO.Common;
namespace AguasNico_Api.Models.DTO.Products;

public class GetProductStatsRequest
{
    public long Id { get; set; }
    public int Year { get; set; }
}

public class GetProductStatsResponse
{
    public ProductItem Product { get; set; }
    public int ClientStock { get; set; }
    public decimal TotalSold { get; set; }
    public int[] AnnualSales { get; set; } = new int[12];
}



