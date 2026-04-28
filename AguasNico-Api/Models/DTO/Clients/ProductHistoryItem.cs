using AguasNico_Api.Models.Constants;
namespace AguasNico_Api.Models.DTO.Clients;

public class ProductHistoryItem
{
    public ProductType ProductType { get; set; }
    public string ProductTypeName { get; set; }
    public ProductActionType ActionType { get; set; }
    public string ActionTypeName { get; set; }
    public int Quantity { get; set; }
    public DateTime Date { get; set; }
}



