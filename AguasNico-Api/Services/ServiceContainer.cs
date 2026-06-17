namespace AguasNico_Api.Services;

public static class ServiceContainer
{
    public static void AddServices(IServiceCollection services)
    {
        services.AddScoped<TokenService>();
        services.AddScoped<AuthService>();
        services.AddScoped<ProductService>();
        services.AddScoped<ClientService>();
        services.AddScoped<AbonoService>();
        services.AddScoped<CartService>();
        services.AddScoped<RouteService>();
        services.AddScoped<ExpenseService>();
        services.AddScoped<TransferService>();
        services.AddScoped<DealerService>();
        services.AddScoped<StatsService>();
        services.AddScoped<InvoiceService>();
        services.AddScoped<HomeService>();
        services.AddScoped<CatalogService>();
        services.AddScoped<TerceroService>();
        services.AddScoped<CajaService>();
    }
}

