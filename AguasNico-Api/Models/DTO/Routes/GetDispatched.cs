using AguasNico_Api.Models.DTO.Common;


namespace AguasNico_Api.Models.DTO.Routes;

public class GetDispatchedRequest
{
    public long RouteId { get; set; }
}

public class GetDispatchedResponse
{
    public List<ProductQuantityItem> Items { get; set; } = [];
}



