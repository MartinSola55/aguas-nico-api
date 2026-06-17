using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AguasNico_Api.Models;

public class Tercero : AuditableEntity
{
    [Key]
    public long ID { get; set; }

    public DateTime Date { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; }

    public int SodaQuantity { get; set; }

    [Column(TypeName = "numeric(18,2)")]
    public decimal SodaAmount { get; set; }

    public int B12LQuantity { get; set; }

    [Column(TypeName = "numeric(18,2)")]
    public decimal B12LAmount { get; set; }

    public int B20LQuantity { get; set; }

    [Column(TypeName = "numeric(18,2)")]
    public decimal B20LAmount { get; set; }
}
