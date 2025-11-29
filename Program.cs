using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MisFinanzas.Components;
using MisFinanzas.Components.Account;
using MisFinanzas.Domain.Entities;
using MisFinanzas.Infrastructure.Data;
using MisFinanzas.Infrastructure.Interfaces;
using MisFinanzas.Infrastructure.Services;
using System.Globalization;
using Microsoft.AspNetCore.StaticFiles;

// Configurar PostgreSQL para usar timestamps sin zona horaria (compatibilidad con SQLite)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// CONFIGURACIÓN DE MONEDA DOMINICANA (PESO DOMINICANO - DOP)
var dominicanCulture = new CultureInfo("es-DO");
dominicanCulture.NumberFormat.CurrencySymbol = "RD$";
dominicanCulture.NumberFormat.CurrencyDecimalDigits = 2;
dominicanCulture.NumberFormat.CurrencyDecimalSeparator = ".";
dominicanCulture.NumberFormat.CurrencyGroupSeparator = ",";

CultureInfo.DefaultThreadCurrentCulture = dominicanCulture;
CultureInfo.DefaultThreadCurrentUICulture = dominicanCulture;

// Configurar que la aplicación siempre use HTTPS en producción
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.HttpsPort = 443;
    });
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
// Agregar soporte para controladores API
builder.Services.AddControllers();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// Agregar Autenticación con Identity y Google
var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
});

authBuilder.AddIdentityCookies();

// Configurar cookies para usar HTTPS
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Configurar autenticación con Google
authBuilder.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]
        ?? throw new InvalidOperationException("Google ClientId not configured");
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]
        ?? throw new InvalidOperationException("Google ClientSecret not configured");
    options.CallbackPath = "/signin-google";
    // Forzar uso de HTTPS en producción (Render)
    if (!builder.Environment.IsDevelopment())
    {
        options.Events.OnRedirectToAuthorizationEndpoint = context =>
        {
            context.HttpContext.Request.Scheme = "https";
            context.Response.Redirect(context.RedirectUri.Replace("http://", "https://"));
            return Task.CompletedTask;
        };

        options.Events.OnRemoteFailure = context =>
        {
            context.HttpContext.Request.Scheme = "https";
            return Task.CompletedTask;
        };
    }
    options.SaveTokens = true;

    // Solicitar permisos de perfil y email
    options.Scope.Add("profile");
    options.Scope.Add("email");

    Console.WriteLine("Google Authentication configured");
});

// Configurar autenticación con Microsoft
authBuilder.AddMicrosoftAccount(options =>
{
    options.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"]
        ?? throw new InvalidOperationException("Microsoft ClientId not configured");
    options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"]
        ?? throw new InvalidOperationException("Microsoft ClientSecret not configured");
    options.CallbackPath = "/signin-microsoft";
    // Forzar uso de HTTPS en producción (Render)
    if (!builder.Environment.IsDevelopment())
    {
        options.Events.OnRedirectToAuthorizationEndpoint = context =>
        {
            context.HttpContext.Request.Scheme = "https";
            context.Response.Redirect(context.RedirectUri.Replace("http://", "https://"));
            return Task.CompletedTask;
        };

        options.Events.OnRemoteFailure = context =>
        {
            context.HttpContext.Request.Scheme = "https";
            return Task.CompletedTask;
        };
    }
    options.SaveTokens = true;

    // Solicitar permisos de perfil y email
    options.Scope.Add("User.Read");

    Console.WriteLine("✅ Microsoft Authentication configured");
});


// CONFIGURAR PostgreSQL
// En producción (Render), usar variable de entorno DATABASE_URL
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Render usa formato DATABASE_URL de Heroku, convertir a formato Npgsql si es necesario
if (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://"))
{
    var uri = new Uri(connectionString);
    var dbPort = uri.Port > 0 ? uri.Port : 5432; // Usar puerto por defecto si no está especificado
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Port={dbPort};Database={uri.LocalPath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
    Console.WriteLine($"✅ Connection string convertido correctamente (Host: {uri.Host}, Port: {dbPort})");
}
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Registrar también DbContext para servicios que lo necesiten directamente
builder.Services.AddScoped(p =>
    p.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// CONFIGURAR DATA PROTECTION PARA RENDER
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>()
    .SetApplicationName("MisFinanzas");

//  CONFIGURAR IDENTITY CON ApplicationUser
builder.Services.AddIdentityCore<MisFinanzas.Domain.Entities.ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
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
builder.Services.AddScoped<IAIAssistantService, AIAssistantService>();


// Servicios de reportes
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IPdfReportGenerator, PdfReportGenerator>();
builder.Services.AddScoped<IExcelReportGenerator, ExcelReportGenerator>();

// Cache temporal (Singleton porque mantiene estado en memoria)
builder.Services.AddSingleton<ITemporaryFileCache, TemporaryFileCache>();

// HttpClient para servicios externos (Google Gemini API)
builder.Services.AddHttpClient<IAIAssistantService, AIAssistantService>();


// Agregar soporte para controladores API

builder.Services.AddControllers();

// Configurar SignalR para archivos grandes
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10 MB
});

// Registrar servicio de fondo para notificaciones automáticas
// ACTIVO EN MODO TESTING (cada 1 minuto) para demostración/presentación
// Ver NotificationBackgroundService.cs para cambiar a modo producción (24 horas)
 builder.Services.AddHostedService<NotificationBackgroundService>();

//Servicio para correo
builder.Services.AddScoped<IEmailSender<MisFinanzas.Domain.Entities.ApplicationUser>, MisFinanzas.Infrastructure.Services.EmailSender>();

// Configurar puerto para Render (usa variable de entorno PORT) - SOLO EN PRODUCCIÓN
if (!builder.Environment.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(int.Parse(port));
    });
}

// Configurar ForwardedHeaders en el builder (ANTES de construir la app)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});


var app = builder.Build();

// Forzar HTTPS en producción ANTES de cualquier otro middleware
if (!app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        context.Request.Scheme = "https";
        context.Request.Host = new HostString(context.Request.Host.Host);
        await next();
    });
}

// APLICAR MIGRACIONES AUTOMÁTICAMENTE EN PRODUCCIÓN
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Aplicar migraciones pendientes
        if (context.Database.GetPendingMigrations().Any())
        {
            Console.WriteLine("🔄 Aplicando migraciones pendientes...");
            context.Database.Migrate();
            Console.WriteLine("✅ Migraciones aplicadas exitosamente");
        }
        else
        {
            Console.WriteLine("✅ Base de datos ya está actualizada");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error al aplicar migraciones: {ex.Message}");
        // En producción, podrías querer que falle si no puede migrar
        // throw;
    }
}

// Aplicar ForwardedHeaders
app.UseForwardedHeaders();

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
