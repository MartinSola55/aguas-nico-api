using AguasNico_Api.DAL;
using AguasNico_Api.DAL.DB;
using AguasNico_Api.Helpers.Interceptors;
using AguasNico_Api.Models.Constants;
using AguasNico_Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("APIContextConnection") ?? throw new InvalidOperationException("Connection string 'APIContextConnection' not found.");

ServiceContainer.AddServices(builder.Services);
builder.Services.AddScoped<MigrationRunner>();
InterceptorsContainer.AddInterceptors(builder.Services);

builder.Services.AddDbContext<APIContext>((serviceProvider, options) =>
{
    options.UseNpgsql(connectionString);
    InterceptorsContainer.ConfigureInterceptors(serviceProvider, options);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

var key = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(key))
    throw new InvalidOperationException("JWT key not found.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.Admin, policy => policy.RequireRole(Roles.Admin))
    .AddPolicy(Policies.Dealer, policy => policy.RequireRole(Roles.Admin, Roles.Dealer));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(cors => cors.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

await RunMigrations();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

async Task RunMigrations()
{
    using var scope = app.Services.CreateScope();
    var runner = scope.ServiceProvider.GetRequiredService<MigrationRunner>();
    await runner.Run();
}
