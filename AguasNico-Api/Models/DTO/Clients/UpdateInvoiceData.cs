using AguasNico_Api.Models.Constants;
namespace AguasNico_Api.Models.DTO.Clients;

public class UpdateInvoiceDataRequest
{
    public long Id { get; set; }
    public InvoiceType InvoiceType { get; set; }
    public TaxCondition TaxCondition { get; set; }
    public string CUIT { get; set; }
}



