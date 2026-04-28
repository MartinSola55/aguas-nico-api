using AguasNico_Api.DAL.Seeding;
using AguasNico_Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AguasNico_Api.DAL.DB;

public class APIContext(DbContextOptions<APIContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        new DbInitializer(builder).Seed();

        builder.Entity<CartPaymentMethod>().HasKey(entity => new { entity.CartID, entity.PaymentMethodID });
        builder.Entity<CartProduct>().HasKey(entity => new { entity.Type, entity.CartID });
        builder.Entity<ClientProduct>().HasKey(entity => new { entity.ProductID, entity.ClientID });
        builder.Entity<ReturnedProduct>().HasKey(entity => new { entity.Type, entity.CartID });
        builder.Entity<DispatchedProduct>().HasKey(entity => new { entity.RouteID, entity.Type });
        builder.Entity<AbonoProduct>().HasKey(entity => new { entity.AbonoID, entity.Type });
        builder.Entity<ClientAbono>().HasKey(entity => new { entity.ClientID, entity.AbonoID });
        builder.Entity<CartAbonoProduct>().HasKey(entity => new { entity.CartID, entity.Type });

        builder.Entity<Cart>().HasQueryFilter(entity => entity.DeletedAt == null);
        builder.Entity<CartPaymentMethod>().HasQueryFilter(entity => entity.DeletedAt == null);
        builder.Entity<Expense>().HasQueryFilter(entity => entity.DeletedAt == null);
        builder.Entity<CartProduct>().HasQueryFilter(entity => entity.DeletedAt == null);
        builder.Entity<ClientProduct>().HasQueryFilter(entity => entity.DeletedAt == null);
        builder.Entity<ReturnedProduct>().HasQueryFilter(entity => entity.DeletedAt == null);
        builder.Entity<Models.Route>().HasQueryFilter(entity => entity.DeletedAt == null);
        builder.Entity<Transfer>().HasQueryFilter(entity => entity.DeletedAt == null);
        builder.Entity<DispatchedProduct>().HasQueryFilter(entity => entity.DeletedAt == null);
        builder.Entity<Abono>().HasQueryFilter(entity => entity.DeletedAt == null);
        builder.Entity<AbonoProduct>().HasQueryFilter(entity => entity.DeletedAt == null);
        builder.Entity<ClientAbono>().HasQueryFilter(entity => entity.DeletedAt == null);
        builder.Entity<AbonoRenewal>().HasQueryFilter(entity => entity.DeletedAt == null);
        builder.Entity<AbonoRenewalProduct>().HasQueryFilter(entity => entity.DeletedAt == null);
        builder.Entity<CartAbonoProduct>().HasQueryFilter(entity => entity.DeletedAt == null);
    }

    public DbSet<ApplicationUser> User { get; set; }
    public DbSet<Abono> Abonos { get; set; }
    public DbSet<AbonoProduct> AbonoProducts { get; set; }
    public DbSet<AbonoRenewal> AbonoRenewals { get; set; }
    public DbSet<AbonoRenewalProduct> AbonoRenewalProducts { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartPaymentMethod> CartPaymentMethods { get; set; }
    public DbSet<CartAbonoProduct> CartAbonoProducts { get; set; }
    public DbSet<CartProduct> CartProducts { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<ClientAbono> ClientAbonos { get; set; }
    public DbSet<ClientProduct> ClientProducts { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ReturnedProduct> ReturnedProducts { get; set; }
    public DbSet<DispatchedProduct> DispatchedProducts { get; set; }
    public DbSet<Models.Route> Routes { get; set; }
    public DbSet<Transfer> Transfers { get; set; }
}
