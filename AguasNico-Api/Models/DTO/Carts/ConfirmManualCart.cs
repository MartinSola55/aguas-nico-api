namespace AguasNico_Api.Models.DTO.Carts;

public class ConfirmManualCartRequest
{
    public long ClientId { get; set; }
    public long RouteId { get; set; }
    public List<CartProductRequestItem> Products { get; set; } = [];
    public List<CartProductRequestItem> AbonoProducts { get; set; } = [];
    public List<CartPaymentRequestItem> PaymentMethods { get; set; } = [];
}

public class ConfirmManualCartResponse
{
    public long CartId { get; set; }
    public long ClientId { get; set; }
}



