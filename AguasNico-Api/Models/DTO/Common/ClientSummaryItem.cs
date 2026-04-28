using AguasNico_Api.Models.Constants;

namespace AguasNico_Api.Models.DTO.Common;

public class ClientSummaryItem
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string DealerId { get; set; }
    public string DealerName { get; set; }
    public Day DeliveryDay { get; set; }
    public decimal Debt { get; set; }
    public bool HasInvoice { get; set; }
    public bool OnlyAbonos { get; set; }
    public bool IsActive { get; set; }
}


