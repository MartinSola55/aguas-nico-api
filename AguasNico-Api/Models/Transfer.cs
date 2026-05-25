using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AguasNico_Api.Models;

public class Transfer : AuditableEntity
{
    [Key]
    public long ID { get; set; }

    [Required]
    public long ClientID { get; set; }

    [Required]
    public string UserID { get; set; }

    [Required(ErrorMessage = "Debes ingresar un monto")]
    [Range(1, 10000000, ErrorMessage = "El monto debe ser mayor a 0")]
    [Column(TypeName = "numeric(18,2)")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Debes ingresar una fecha")]
    public DateTime Date { get; set; } = LocalClock.Now;

    public virtual Client Client { get; set; }
    public virtual User User { get; set; }
}
