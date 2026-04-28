using AguasNico_Api.Models.Constants;
using AguasNico_Api.Models.DTO.Common;


namespace AguasNico_Api.Models.DTO.Carts;

public class GetCartForEditRequest
{
    public long Id { get; set; }
}

public class GetCartForEditResponse
{
    public long Id { get; set; }
    public long ClientId { get; set; }
    public string ClientName { get; set; }
    public long RouteId { get; set; }
    public State State { get; set; }
    public List<ProductQuantityItem> Products { get; set; } = [];
    public List<ProductQuantityItem> AbonoProducts { get; set; } = [];
    public List<ProductQuantityItem> ReturnedProducts { get; set; } = [];
    public List<PaymentMethodOptionItem> PaymentMethods { get; set; } = [];

    public class PaymentMethodOptionItem
    {
        public short Id { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }
        public decimal Amount { get; set; }
    }
}


