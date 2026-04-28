using AguasNico_Api.Models.Constants;
namespace AguasNico_Api.Models.DTO.Routes;

public class RouteItem
{
    public long Id { get; set; }
    public string UserId { get; set; }
    public string DealerName { get; set; }
    public int TruckNumber { get; set; }
    public Day DayOfWeek { get; set; }
    public bool IsStatic { get; set; }
    public bool IsClosed { get; set; }
    public decimal DispenserPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalCarts { get; set; }
    public int CompletedCarts { get; set; }
    public int PendingCarts { get; set; }
}



