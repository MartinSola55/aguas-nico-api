using System.ComponentModel.DataAnnotations;

namespace AguasNico_Api.Models;

public class ClientAbono : AuditableEntity
{
    [Required]
    public long ClientID { get; set; }

    [Required]
    public long AbonoID { get; set; }

    public virtual Client Client { get; set; }
    public virtual Abono Abono { get; set; }
}
