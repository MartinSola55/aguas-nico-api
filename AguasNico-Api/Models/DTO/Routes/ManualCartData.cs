namespace AguasNico_Api.Models.DTO.Routes;

public class ManualCartDataRequest
{
    public long RouteId { get; set; }
}

public class ManualCartDataResponse
{
    public RouteItem Route { get; set; }
    public List<PaymentMethodItem> PaymentMethods { get; set; } = [];
}

public class PaymentMethodItem
{
    public short Id { get; set; }
    public string Name { get; set; }
}



