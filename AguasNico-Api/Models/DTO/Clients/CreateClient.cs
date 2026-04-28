using AguasNico_Api.Models.Constants;
namespace AguasNico_Api.Models.DTO.Clients;

public class CreateClientRequest
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Observations { get; set; }
    public string Notes { get; set; }
    public decimal Debt { get; set; }
    public string DealerId { get; set; }
    public bool HasInvoice { get; set; }
    public bool OnlyAbonos { get; set; }
    public InvoiceType InvoiceType { get; set; }
    public TaxCondition TaxCondition { get; set; }
    public string CUIT { get; set; }
    public Day DeliveryDay { get; set; }
    public List<ClientProductRequestItem> Products { get; set; } = [];
    public List<long> AbonoIds { get; set; } = [];
}

public class CreateClientResponse
{
    public long Id { get; set; }
}



