using System.ComponentModel.DataAnnotations;

namespace AguasNico_Api.Models;

public class ClientProduct : AuditableEntity
{
    [Required]
    public long ClientID { get; set; }

    [Required]
    public long ProductID { get; set; }

    [Required(ErrorMessage = "Debes ingresar un stock")]
    [Range(0, 200, ErrorMessage = "El stock debe estar entre 0 y 200")]
    public int Stock { get; set; }

    public virtual Product Product { get; set; }
    public virtual Client Client { get; set; }
}
