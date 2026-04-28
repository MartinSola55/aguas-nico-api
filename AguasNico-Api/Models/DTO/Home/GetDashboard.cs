using AguasNico_Api.Models.DTO.Common;
using AguasNico_Api.Models.DTO.Expenses;
using AguasNico_Api.Models.DTO.Routes;


namespace AguasNico_Api.Models.DTO.Home;

public class GetDashboardResponse
{
    public string Role { get; set; }
    public List<SoldProductsItem> SoldProducts { get; set; } = [];
    public List<ExpenseItem> Expenses { get; set; } = [];
    public List<PaymentAmountItem> Payments { get; set; } = [];
    public decimal TotalSold { get; set; }
    public List<TransferItem> Transfers { get; set; } = [];
    public decimal Dispensers { get; set; }
    public List<RouteItem> DealerRoutes { get; set; } = [];
    public List<DealerItem> Dealers { get; set; } = [];
}


