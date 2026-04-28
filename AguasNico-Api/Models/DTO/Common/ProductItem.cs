using AguasNico_Api.Models.Constants;

namespace AguasNico_Api.Models.DTO.Common;

public class ProductItem
{
    public long Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public ProductType Type { get; set; }
    public string TypeName { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}


