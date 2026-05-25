using AguasNico_Api.Models.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AguasNico_Api.Models;

public class Route : AuditableEntity
{
    [Key]
    public long ID { get; set; }

    public string UserID { get; set; }

    [Required(ErrorMessage = "Debes ingresar un día de la semana")]
    public Day DayOfWeek { get; set; }

    public bool IsStatic { get; set; }
    public bool IsClosed { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal DispenserPrice { get; set; }

    public virtual User User { get; set; }
    public virtual List<Cart> Carts { get; set; } = [];
    public virtual List<DispatchedProduct> DispatchedProducts { get; set; } = [];
}
