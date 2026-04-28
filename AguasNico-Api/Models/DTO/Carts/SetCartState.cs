using AguasNico_Api.Models.Constants;
namespace AguasNico_Api.Models.DTO.Carts;

public class SetCartStateRequest
{
    public long CartId { get; set; }
    public State State { get; set; }
}



