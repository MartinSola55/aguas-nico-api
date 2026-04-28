using System.ComponentModel.DataAnnotations;

namespace AguasNico_Api.Models.Constants;

public static class BusinessConstants
{
    public const int InvoiceUnitType = 7;
    public const int InvoiceSalesPoint = 5;
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
