using AguasNico_Api.Models.Constants;
using System.ComponentModel.DataAnnotations;

namespace AguasNico_Api.Models;

public class AbonoProduct : AuditableEntity
{
    [Required]
    public long AbonoID { get; set; }

    [Required]
    public ProductType Type { get; set; }

    [Required(ErrorMessage = "Debes ingresdar una cantidad")]
    [Range(1, 100, ErrorMessage = "La cantidad debe estar entre 1 y 100")]
    public int Quantity { get; set; }

    public virtual Abono Abono { get; set; }
}
