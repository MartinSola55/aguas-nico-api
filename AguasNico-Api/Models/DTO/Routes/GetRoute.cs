using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO.Carts;
using AguasNico_Api.Models.DTO.Clients;
using AguasNico_Api.Models.DTO.Common;


namespace AguasNico_Api.Models.DTO.Routes;

public class GetRouteRequest
{
    public long Id { get; set; }
}

public class GetRouteResponse : RouteItem
{
    public List<RouteCartItem> Carts { get; set; } = [];
    public List<PaymentAmountItem> Payments { get; set; } = [];
    public List<TransferItem> Transfers { get; set; } = [];
    public decimal TotalSold { get; set; }
    public decimal TotalExpenses { get; set; }
}

public class RouteCartItem
{
    public long Id { get; set; }
    public long ClientId { get; set; }
    public string ClientName { get; set; }
    public string ClientAddress { get; set; }
    public decimal ClientDebt { get; set; }
    public int Priority { get; set; }
    public State State { get; set; }
    public decimal Collected { get; set; }
    public List<ProductQuantityItem> Products { get; set; } = [];
    public List<ProductQuantityItem> AbonoProducts { get; set; } = [];
    public List<GetCartForEditResponse.PaymentMethodOptionItem> PaymentMethods { get; set; } = [];
    public List<GetProductsAndAbonoResponse.ProductOptionItem> AvailableProducts { get; set; } = [];
    public List<GetProductsAndAbonoResponse.AbonoProductOptionItem> AvailableAbonoProducts { get; set; } = [];
}

public class TransferItem
{
    public long Id { get; set; }
    public long ClientId { get; set; }
    public string ClientName { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}


