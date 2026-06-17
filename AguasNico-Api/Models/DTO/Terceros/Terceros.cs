namespace AguasNico_Api.Models.DTO.Terceros;

public class GetTercerosRequest
{
    public DateTime Date { get; set; }
}

public class GetTercerosResponse
{
    public List<TerceroItem> Items { get; set; } = [];
}

public class TerceroItem
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public string Name { get; set; }
    public int SodaQuantity { get; set; }
    public decimal SodaAmount { get; set; }
    public int B12LQuantity { get; set; }
    public decimal B12LAmount { get; set; }
    public int B20LQuantity { get; set; }
    public decimal B20LAmount { get; set; }
}

public class CreateTerceroRequest
{
    public DateTime Date { get; set; }
    public string Name { get; set; }
    public int SodaQuantity { get; set; }
    public decimal SodaAmount { get; set; }
    public int B12LQuantity { get; set; }
    public decimal B12LAmount { get; set; }
    public int B20LQuantity { get; set; }
    public decimal B20LAmount { get; set; }
}

public class CreateTerceroResponse
{
    public long Id { get; set; }
}

public class UpdateTerceroRequest
{
    public long Id { get; set; }
    public string Name { get; set; }
    public int SodaQuantity { get; set; }
    public decimal SodaAmount { get; set; }
    public int B12LQuantity { get; set; }
    public decimal B12LAmount { get; set; }
    public int B20LQuantity { get; set; }
    public decimal B20LAmount { get; set; }
}

public class UpdateTerceroResponse
{
    public long Id { get; set; }
}

public class DeleteTerceroRequest
{
    public long Id { get; set; }
}
