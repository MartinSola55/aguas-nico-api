using AguasNico_Api.Models.Constants;
using System.ComponentModel.DataAnnotations;

namespace AguasNico_Api.Models;

public class ReturnedProduct : AuditableEntity
{
    [Required]
    public ProductType Type { get; set; }

    [Required]
    public long CartID { get; set; }

    [Required(ErrorMessage = "Debes ingresar una cantidad")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Quantity { get; set; }

    public virtual Cart Cart { get; set; }
}
