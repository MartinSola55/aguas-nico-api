namespace AguasNico_Api.Models.DTO.Invoices;

public class GetInvoicesCsvRequest : GetInvoicesRequest
{
}

public class GetInvoicesCsvResponse
{
    public List<InvoiceCsvRowItem> Rows { get; set; } = [];
}

public class InvoiceCsvRowItem
{
    public string ExternalId { get; set; }
    public string ClientCuit { get; set; }
    public string InvoiceTypeId { get; set; }
    public decimal Neto { get; set; }
    public int IvaRate { get; set; }
    public decimal Total { get; set; }
    public int TaxConditionTypeId { get; set; }
    public string ClientName { get; set; }
    public string ClientAddress { get; set; }
    public string Description { get; set; }
    public string Email { get; set; }
}



