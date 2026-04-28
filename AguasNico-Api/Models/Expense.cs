using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AguasNico_Api.Models;

public class Expense : AuditableEntity
{
    [Key]
    public long ID { get; set; }

    [Required]
    public string UserID { get; set; }

    [Required(ErrorMessage = "Debes seleccionar un repartidor")]
    [Column(TypeName = "money")]
    [Range(0, 10000000, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Debes ingresar una descripción")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Debes ingresar una descripción de menos de 200 caracteres")]
    public string Description { get; set; }

    public virtual ApplicationUser User { get; set; }
}
