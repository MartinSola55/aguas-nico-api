using AguasNico_Api.DAL.DB;
using AguasNico_Api.Models;
using AguasNico_Api.Models.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AguasNico_Api.DAL.Seeding;

public class Seeder(APIContext db, IConfiguration config, IHostEnvironment env, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    private readonly APIContext _db = db;
    private readonly IConfiguration _config = config;
    private readonly IHostEnvironment _env = env;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;

    public async Task Seed()
    {
        //if (!_env.IsProduction() && (await _db.Database.GetPendingMigrationsAsync()).Any())
        //    await _db.Database.MigrateAsync();

        if (await _db.Roles.AnyAsync(x => x.Name == Roles.Admin))
            return;

        foreach (var role in Roles.GetRoles())
            await _roleManager.CreateAsync(new IdentityRole(role));

        var admin = new ApplicationUser
        {
            UserName = _config["AdminEmail"] ?? "admin@admin",
            Email = _config["AdminEmail"] ?? "admin@admin",
            Name = "Admin",
            EmailConfirmed = true,
        };

        var dealer = new ApplicationUser
        {
            UserName = _config["DealerEmail"] ?? "dealer@dealer",
            Email = _config["DealerEmail"] ?? "dealer@dealer",
            Name = "Dealer",
            EmailConfirmed = true,
        };

        await _userManager.CreateAsync(admin, _config["AdminPassword"] ?? "Password1!");
        await _userManager.CreateAsync(dealer, _config["DealerPassword"] ?? "Password1!");

        await _userManager.AddToRoleAsync(admin, Roles.Admin);
        await _userManager.AddToRoleAsync(dealer, Roles.Dealer);
    }
}
