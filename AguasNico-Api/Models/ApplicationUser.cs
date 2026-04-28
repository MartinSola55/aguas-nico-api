using Microsoft.AspNetCore.Identity;

namespace AguasNico_Api.Models;

public partial class ApplicationUser : IdentityUser
{
    public string Name { get; set; }
    public int? TruckNumber { get; set; }

    public virtual List<Client> Clients { get; set; } = [];
    public virtual List<Route> Routes { get; set; } = [];
    public virtual List<Expense> Expenses { get; set; } = [];
    public virtual List<Transfer> Transfers { get; set; } = [];
}
