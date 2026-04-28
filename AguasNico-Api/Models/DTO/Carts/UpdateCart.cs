namespace AguasNico_Api.Models.DTO.Carts;

public class UpdateCartRequest
{
    public long Id { get; set; }
    public long ClientId { get; set; }
    public long RouteId { get; set; }
    public List<CartProductRequestItem> Products { get; set; } = [];
    public List<CartProductRequestItem> AbonoProducts { get; set; } = [];
    public List<ReturnedProductRequestItem> ReturnedProducts { get; set; } = [];
    public List<CartPaymentRequestItem> PaymentMethods { get; set; } = [];
}



