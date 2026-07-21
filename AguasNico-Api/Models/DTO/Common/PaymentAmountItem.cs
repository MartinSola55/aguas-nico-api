namespace AguasNico_Api.Models.DTO.Common;

public class PaymentAmountItem
{
    public short PaymentMethodId { get; set; }
    public string PaymentMethodName { get; set; }
    public string Code { get; set; }
    public decimal Amount { get; set; }
}



