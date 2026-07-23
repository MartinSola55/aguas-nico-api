using System.ComponentModel.DataAnnotations;

namespace AguasNico_Api.Models;

public class PaymentMethod
{
    [Key]
    public short ID { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }

    public virtual List<CartPaymentMethod> Carts { get; set; } = [];
}
