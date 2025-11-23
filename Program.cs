using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MisFinanzas.Components;
using MisFinanzas.Components.Account;
using MisFinanzas.Domain.Entities;
using MisFinanzas.Infrastructure.Data;
using MisFinanzas.Infrastructure.Interfaces;
using MisFinanzas.Infrastructure.Services;
using System.Globalization;


var builder = WebApplication.CreateBuilder(args);

// CONFIGURACIÓN DE MONEDA DOMINICANA (PESO DOMINICANO - DOP)
var dominicanCulture = new CultureInfo("es-DO");
dominicanCulture.NumberFormat.CurrencySymbol = "RD$";
dominicanCulture.NumberFormat.CurrencyDecimalDigits = 2;
dominicanCulture.NumberFormat.CurrencyDecimalSeparator = ".";
dominicanCulture.NumberFormat.CurrencyGroupSeparator = ",";

CultureInfo.DefaultThreadCurrentCulture = dominicanCulture;
CultureInfo.DefaultThreadCurrentUICulture = dominicanCulture;

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
// Agregar soporte para controladores API
builder.Services.AddControllers();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

// CONFIGURAR SQLite CON NUESTRO DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//  CONFIGURAR IDENTITY CON ApplicationUser
builder.Services.AddIdentityCore<MisFinanzas.Domain.Entities.ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

//  REGISTRAR NUESTROS SERVICIOS (Dependency Injection)
builder.Services.AddScoped<ICategoryService, CategoryService> ();
builder.Services.AddScoped<IExpenseIncomeService, ExpenseIncomeService>();
builder.Services.AddScoped<IFinancialGoalService, FinancialGoalService>();
// Servicios de negocio adicionales
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Servicios de reportes
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IPdfReportGenerator, PdfReportGenerator>();
builder.Services.AddScoped<IExcelReportGenerator, ExcelReportGenerator>();

// Cache temporal (Singleton porque mantiene estado en memoria)
builder.Services.AddSingleton<ITemporaryFileCache, TemporaryFileCache>();

// Servicio en background para notificaciones
builder.Services.AddHostedService<NotificationBackgroundService>();

//Servicio para correo
builder.Services.AddScoped<IEmailSender<MisFinanzas.Domain.Entities.ApplicationUser>, MisFinanzas.Infrastructure.Services.EmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

// Mapear controladores API
app.MapControllers();

app.Run();
