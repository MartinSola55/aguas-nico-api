using AguasNico_Api.Models.Constants;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AguasNico_Api.Models;

public class Product
{
    [Key]
    public long ID { get; set; }

    [Required(ErrorMessage = "Debes ingresar un nombre")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Debes ingresar un nombre de menos de 200 caracteres")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Debes ingresar un precio")]
    [Column(TypeName = "money")]
    [Range(0, 1000000, ErrorMessage = "Debes ingresar un precio válido")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Debes ingresar un tipo de producto")]
    public ProductType Type { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = LocalClock.Now;
    public DateTime UpdatedAt { get; set; } = LocalClock.Now;

    [DefaultValue(true)]
    public bool IsActive { get; set; } = true;
}
