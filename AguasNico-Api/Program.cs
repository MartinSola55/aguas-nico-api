using AguasNico_Api.DAL.DB;
using AguasNico_Api.DAL.Seeding;
using AguasNico_Api.Models;
using AguasNico_Api.Models.Constants;
using AguasNico_Api.Security;
using AguasNico_Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("APIContextConnection") ?? throw new InvalidOperationException("Connection string 'APIContextConnection' not found.");

ServiceContainer.AddServices(builder.Services);
builder.Services.AddScoped<Seeder>();

builder.Services.AddDbContext<APIContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<APIContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IAuthorizationHandler, RolesHandler>();

var key = builder.Configuration["JWT:Key"] ?? throw new InvalidOperationException("JWT key not found.");
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
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.Admin, policy => policy.Requirements.Add(new AuthorizeRolesAttribute(Roles.Admin)))
    .AddPolicy(Policies.Dealer, policy => policy.Requirements.Add(new AuthorizeRolesAttribute(Roles.Dealer)));

var app = builder.Build();

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("es-ES");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("es-ES");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(cors => cors.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

await Seed();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

async Task Seed()
{
    using var scope = app.Services.CreateScope();
    var dbSeeder = scope.ServiceProvider.GetRequiredService<Seeder>();
    await dbSeeder.Seed();
}