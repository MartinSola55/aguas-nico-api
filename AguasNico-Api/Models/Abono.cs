using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AguasNico_Api.Models;

public class Abono : AuditableEntity
{
    [Key]
    public long ID { get; set; }

    [Required(ErrorMessage = "Debes ingresar un nombre")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Debes ingresar un precio")]
    [Column(TypeName = "money")]
    public decimal Price { get; set; }

    public virtual List<AbonoProduct> Products { get; set; } = [];
    public virtual List<AbonoRenewal> Renewals { get; set; } = [];
}
