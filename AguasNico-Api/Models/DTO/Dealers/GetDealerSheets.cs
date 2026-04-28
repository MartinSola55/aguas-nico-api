using AguasNico_Api.Models.Constants;
namespace AguasNico_Api.Models.DTO.Dealers;

public class GetDealerSheetsRequest
{
    public string DealerId { get; set; }
}

public class GetDealerSheetsResponse
{
    public List<DealerSheetItem> Sheets { get; set; } = [];
}

public class DealerSheetItem
{
    public Day Day { get; set; }
    public long ClientId { get; set; }
    public string ClientName { get; set; }
    public string ClientPhone { get; set; }
    public string ClientAddress { get; set; }
    public string ClientObservations { get; set; }
    public decimal ClientDebt { get; set; }
    public List<ProductSheetItem> Products { get; set; } = [];
    public List<AbonoProductSheetItem> AbonoProducts { get; set; } = [];
    public List<AbonoSheetItem> Abonos { get; set; } = [];

    public class ProductSheetItem
    {
        public ProductType Type { get; set; }
        public string TypeName { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }

    public class AbonoProductSheetItem
    {
        public long AbonoId { get; set; }
        public ProductType Type { get; set; }
        public string TypeName { get; set; }
        public int Available { get; set; }
        public int Stock { get; set; }
    }

    public class AbonoSheetItem
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}



