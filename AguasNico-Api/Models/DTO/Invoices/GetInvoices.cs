using AguasNico_Api.Models.Constants;
namespace AguasNico_Api.Models.DTO.Invoices;

public class GetInvoicesRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Day InvoiceDay { get; set; }
    public string InvoiceDealer { get; set; }
}

public class GetInvoicesResponse
{
    public List<InvoiceItem> Items { get; set; } = [];
}

public class InvoiceItem
{
    public long ClientId { get; set; }
    public string ClientName { get; set; }
    public string ClientAddress { get; set; }
    public string ClientCuit { get; set; }
    public List<InvoiceProductItem> Products { get; set; } = [];
}

public class InvoiceProductItem
{
    public string Type { get; set; }
    public int Quantity { get; set; }
    public decimal Total { get; set; }
}



