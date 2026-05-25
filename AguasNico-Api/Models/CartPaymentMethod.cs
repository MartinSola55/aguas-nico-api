using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AguasNico_Api.Models;

public class CartPaymentMethod : AuditableEntity
{
    [Required]
    public long CartID { get; set; }

    [Required]
    public short PaymentMethodID { get; set; }

    [Required(ErrorMessage = "Debes ingresar un monto")]
    [Column(TypeName = "numeric(18,2)")]
    [Range(0, 1000000, ErrorMessage = "Debes ingresar un monto entre $0 y $1.000.000")]
    public decimal Amount { get; set; }

    public virtual Cart Cart { get; set; }
    public virtual PaymentMethod PaymentMethod { get; set; }
}
