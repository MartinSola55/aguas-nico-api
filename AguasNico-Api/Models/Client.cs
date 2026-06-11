using AguasNico_Api.Models.Constants;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AguasNico_Api.Models;

public class Client
{
    [Key]
    public long ID { get; set; }

    [Required(ErrorMessage = "Debes ingresar un nombre")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Debes ingresar un nombre de menos de 200 caracteres")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Debes ingresar una dirección")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Debes ingresar una dirección de menos de 200 caracteres")]
    public string Address { get; set; }

    [Required(ErrorMessage = "Debes ingresar un teléfono")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Debes ingresar un teléfono de menos de 200 caracteres")]
    [Phone(ErrorMessage = "Debes ingresar un teléfono válido")]
    public string Phone { get; set; }

    [StringLength(200, ErrorMessage = "Debes ingresar un email de menos de 200 caracteres")]
    [EmailAddress(ErrorMessage = "Debes ingresar un email válido")]
    public string Email { get; set; }

    [StringLength(300, MinimumLength = 1, ErrorMessage = "Debes ingresar observaciones de menos de 300 caracteres")]
    public string Observations { get; set; }

    [StringLength(300, MinimumLength = 1, ErrorMessage = "Las notas deben tener menos de 300 caracteres")]
    public string Notes { get; set; }

    [Column(TypeName = "numeric(18,2)")]
    [DefaultValue(0)]
    public decimal Debt { get; set; }

    public string DealerID { get; set; }

    [DefaultValue(false)]
    public bool HasInvoice { get; set; }

    [DefaultValue(false)]
    public bool OnlyAbonos { get; set; }

    public InvoiceType? InvoiceType { get; set; }
    public TaxCondition? TaxCondition { get; set; }

    [StringLength(11, MinimumLength = 1, ErrorMessage = "Debes ingresar un CUIT de menos de 12 caracteres")]
    public string CUIT { get; set; }

    public Day? DeliveryDay { get; set; }

    public DateTime CreatedAt { get; set; } = LocalClock.Now;
    public DateTime UpdatedAt { get; set; } = LocalClock.Now;

    [DefaultValue(true)]
    public bool IsActive { get; set; } = true;

    public virtual User Dealer { get; set; }
    public virtual List<ClientProduct> Products { get; set; } = [];
    public virtual List<Cart> Carts { get; set; } = [];
    public virtual List<ClientAbono> Abonos { get; set; } = [];
    public virtual List<AbonoRenewal> AbonosRenewed { get; set; } = [];
    public virtual List<Transfer> Transfers { get; set; } = [];
}
