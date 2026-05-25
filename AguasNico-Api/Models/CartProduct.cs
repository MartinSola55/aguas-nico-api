using AguasNico_Api.Models.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AguasNico_Api.Models;

public class CartProduct : AuditableEntity
{
    [Required]
    public long CartID { get; set; }

    [Required]
    public ProductType Type { get; set; }

    [Required(ErrorMessage = "Debes ingresar una cantidad")]
    [Range(1, 100, ErrorMessage = "La cantidad debe estar entre 1 y 100")]
    public int Quantity { get; set; }

    [Column(TypeName = "numeric(18,2)")]
    public decimal SettedPrice { get; set; }

    public virtual Cart Cart { get; set; }
}
