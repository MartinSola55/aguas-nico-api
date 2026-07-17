using System.ComponentModel.DataAnnotations;

namespace AguasNico_Api.Models.Constants;

public static class BusinessConstants
{
    public const int InvoiceUnitType = 7;
    public const int InvoiceSalesPoint = 5;
}

public enum Actividad
{
    SODA_GRAVADO,
    AGUA_GRAVADA,
    AGUA_EXENTA,
    ALQUILER_DISPENSER,
}

public static class ProductTax
{
    private static readonly (Actividad Actividad, decimal Alicuota) Fallback = (Actividad.AGUA_GRAVADA, 21m);

    private static readonly Dictionary<ProductType, (Actividad Actividad, decimal Alicuota)> Fixed = new()
    {
        { ProductType.Soda, (Actividad.SODA_GRAVADO, 21m) },
        { ProductType.Maquina, (Actividad.ALQUILER_DISPENSER, 21m) },
    };

    private static readonly HashSet<ProductType> Water = [ProductType.B20L, ProductType.B12L, ProductType.B5L];

    // Ley de IVA art. 7 inc. f: el agua ordinaria natural solo pierde la exención
    // cuando el comprador es responsable inscripto.
    private static readonly Dictionary<TaxCondition, (Actividad Actividad, decimal Alicuota)> WaterByTaxCondition = new()
    {
        { TaxCondition.RI, (Actividad.AGUA_GRAVADA, 21m) },
        { TaxCondition.MO, (Actividad.AGUA_EXENTA, 0m) },
        { TaxCondition.EX, (Actividad.AGUA_EXENTA, 0m) },
        { TaxCondition.CF, (Actividad.AGUA_EXENTA, 0m) },
    };

    /// <summary>
    /// Actividad y alícuota del agua para un comprador. También se usa para los abonos, que se
    /// facturan enteros como agua.
    /// </summary>
    public static (Actividad Actividad, decimal Alicuota) ResolveWater(TaxCondition? taxCondition)
    {
        if (taxCondition.HasValue && WaterByTaxCondition.TryGetValue(taxCondition.Value, out var waterTax))
            return waterTax;

        return Fallback;
    }

    public static (Actividad Actividad, decimal Alicuota) Resolve(ProductType type, TaxCondition? taxCondition)
    {
        if (Fixed.TryGetValue(type, out var fixedTax))
            return fixedTax;

        if (Water.Contains(type))
            return ResolveWater(taxCondition);

        return Fallback;
    }

    public static decimal NetoFromSubtotal(decimal subtotal, decimal alicuota)
    {
        return alicuota == 0m ? subtotal : subtotal / (1m + alicuota / 100m);
    }
}

public static class Roles
{
    public const string Admin = "ADMIN";
    public const string Dealer = "DEALER";

    public static List<string> GetRoles()
    {
        return [Admin, Dealer];
    }
}

public static class Policies
{
    public const string Admin = "Admin";
    public const string Dealer = "Dealer";
}

public enum State
{
    [Display(Name = "Pendiente")] Pending = 0,
    [Display(Name = "Confirmado")] Confirmed = 1,
    [Display(Name = "No estaba")] Ausent = 2,
    [Display(Name = "No necesitaba")] NotNeeded = 3,
    [Display(Name = "De vacaciones")] Holidays = 4
}

public enum ProductType
{
    [Display(Name = "Bidón 20L")] B20L = 1,
    [Display(Name = "Bidón 12L")] B12L = 2,
    [Display(Name = "Soda")] Soda = 3,
    [Display(Name = "Máquina frío calor")] Maquina = 4,
    [Display(Name = "Bidón 5L")] B5L = 5,
}

public enum Day
{
    Lunes = 1,
    Martes = 2,
    Miércoles = 3,
    Jueves = 4,
    Viernes = 5
}

public enum InvoiceType
{
    [Display(Name = "Factura A")] A = 1,
    [Display(Name = "Factura B")] B = 2,
}

public enum TaxCondition
{
    [Display(Name = "Responsable Inscripto")] RI = 1,
    [Display(Name = "Monotributo")] MO = 2,
    [Display(Name = "Exento")] EX = 3,
    [Display(Name = "Consumidor Final")] CF = 4,
}

public enum CartsTransfersType
{
    [Display(Name = "Transferencia")] Transfer,
    [Display(Name = "Carrito")] Cart,
    [Display(Name = "Abono")] Abono
}

public enum ProductActionType
{
    [Display(Name = "Baja")] Baja,
    [Display(Name = "Devuelve")] Devuelve,
    [Display(Name = "Baja (abono)")] Abono,
}
