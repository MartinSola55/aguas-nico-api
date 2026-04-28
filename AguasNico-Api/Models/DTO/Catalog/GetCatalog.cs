namespace AguasNico_Api.Models.DTO.Catalog;

public class GetCatalogResponse
{
    public List<EnumItem<int>> States { get; set; } = [];
    public List<EnumItem<int>> ProductTypes { get; set; } = [];
    public List<EnumItem<int>> Days { get; set; } = [];
    public List<EnumItem<int>> InvoiceTypes { get; set; } = [];
    public List<EnumItem<int>> TaxConditions { get; set; } = [];
    public List<PaymentMethodCatalogItem> PaymentMethods { get; set; } = [];
}

public class EnumItem<T>
{
    public T Id { get; set; }
    public string Description { get; set; }
}

public class PaymentMethodCatalogItem
{
    public short Id { get; set; }
    public string Description { get; set; }
}



