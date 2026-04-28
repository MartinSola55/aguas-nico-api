using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO.Common;
namespace AguasNico_Api.Models.DTO.Clients;

public class GetClientRequest
{
    public long Id { get; set; }
    public bool IncludeDetails { get; set; }
}

public class GetClientResponse : ClientSummaryItem
{
    public string Observations { get; set; }
    public string Notes { get; set; }
    public InvoiceType InvoiceType { get; set; }
    public TaxCondition TaxCondition { get; set; }
    public string CUIT { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ClientProductItem> Products { get; set; } = [];
    public List<ClientAbonoItem> Abonos { get; set; } = [];
    public List<CartsTransfersHistoryItem> CartsTransfersHistory { get; set; } = [];
    public List<ProductHistoryItem> ProductsHistory { get; set; } = [];
}

public class CartsTransfersHistoryItem
{
    public DateTime Date { get; set; }
    public CartsTransfersType Type { get; set; }
    public State CartState { get; set; }
    public string AbonoName { get; set; }
    public List<PaymentAmountItem> PaymentMethods { get; set; } = [];
    public List<ProductQuantityItem> Products { get; set; } = [];
    public List<ProductQuantityItem> AbonoProducts { get; set; } = [];
    public decimal TransferAmount { get; set; }
    public decimal AbonoPrice { get; set; }
}

public class ClientAbonoItem
{
    public long AbonoId { get; set; }
    public string AbonoName { get; set; }
    public decimal Price { get; set; }
    public bool Assigned { get; set; }
}

public class ClientProductItem
{
    public long ProductId { get; set; }
    public string ProductName { get; set; }
    public ProductType Type { get; set; }
    public string TypeName { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool Assigned { get; set; }
}



