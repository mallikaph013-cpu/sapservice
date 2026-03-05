using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using myapp.Data;
using myapp.Models;
using Microsoft.AspNetCore.Identity;
using myapp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var databaseProvider = builder.Configuration["DatabaseProvider"]?.Trim() ?? "Sqlite";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        var sqlServerConnection = builder.Configuration.GetConnectionString("DefaultConnectionSqlServer")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnectionSqlServer' not found.");
        options.UseSqlServer(sqlServerConnection);
        return;
    }

    if (databaseProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        var sqliteConnection = builder.Configuration.GetConnectionString("DefaultConnectionSqlite")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnectionSqlite' not found.");
        options.UseSqlite(sqliteConnection);
        return;
    }

    throw new InvalidOperationException("Invalid DatabaseProvider. Use 'Sqlite' or 'SqlServer'.");
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add Identity services & configure IT role claim
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ITUserClaimsPrincipalFactory>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
