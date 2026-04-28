namespace AguasNico_Api.Models.DTO.Common;

public class SoldProductsItem
{
    public string Name { get; set; }
    public int Dispatched { get; set; }
    public int Sold { get; set; }
    public decimal Total { get; set; }
    public int Returned { get; set; }
    public int ClientStock { get; set; }
}



