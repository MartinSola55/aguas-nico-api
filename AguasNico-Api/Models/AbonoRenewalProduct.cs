using AguasNico_Api.Models.Constants;
using System.ComponentModel.DataAnnotations;

namespace AguasNico_Api.Models;

public class AbonoRenewalProduct : AuditableEntity
{
    [Key]
    public long ID { get; set; }

    [Required]
    public long AbonoRenewalID { get; set; }

    [Required]
    public ProductType Type { get; set; }

    public int Available { get; set; }

    public virtual AbonoRenewal AbonoRenewal { get; set; }
}
