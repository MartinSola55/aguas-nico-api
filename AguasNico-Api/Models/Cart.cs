using AguasNico_Api.Models.Constants;
using System.ComponentModel.DataAnnotations;

namespace AguasNico_Api.Models;

public class Cart : AuditableEntity
{
    [Key]
    public long ID { get; set; }

    [Required]
    public long ClientID { get; set; }

    [Required]
    public long RouteID { get; set; }

    public int Priority { get; set; }

    [Required]
    public State State { get; set; } = State.Pending;

    public bool IsStatic { get; set; }

    public virtual Client Client { get; set; }
    public virtual List<CartProduct> Products { get; set; } = [];
    public virtual List<CartAbonoProduct> AbonoProducts { get; set; } = [];
    public virtual List<ReturnedProduct> ReturnedProducts { get; set; } = [];
    public virtual List<CartPaymentMethod> PaymentMethods { get; set; } = [];
    public virtual Route Route { get; set; }
}
