using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AguasNico_Api.Models;

public class AbonoRenewal : AuditableEntity
{
    [Key]
    public long ID { get; set; }

    [Required]
    public long AbonoID { get; set; }

    [Required]
    public long ClientID { get; set; }

    [Column(TypeName = "money")]
    public decimal SettedPrice { get; set; }

    public virtual Abono Abono { get; set; }
    public virtual Client Client { get; set; }
    public virtual List<AbonoRenewalProduct> ProductsAvailables { get; set; } = [];
}
