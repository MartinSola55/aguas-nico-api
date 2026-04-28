using AguasNico_Api.Models.Constants;
using System.ComponentModel.DataAnnotations;

namespace AguasNico_Api.Models;

public class DispatchedProduct : AuditableEntity
{
    [Required]
    public long RouteID { get; set; }

    [Required]
    public ProductType Type { get; set; }

    [Required(ErrorMessage = "Debes ingresar una cantidad")]
    [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 0")]
    public int Quantity { get; set; }

    public virtual Route Route { get; set; }
}
