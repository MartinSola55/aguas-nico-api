namespace AguasNico_Api.Models.DTO.Abonos;

public class UpdateAbonoRequest
{
    public long Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

public class UpdateAbonoResponse
{
    public long Id { get; set; }
}



