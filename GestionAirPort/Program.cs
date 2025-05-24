using GestionAirPort.data;
using GestionAirPort.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;
using GestionAirPort.Constants;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Configuration de la connexion à SQL Server
builder.Services.AddDbContext<AirportContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//  Configuration Identity avec EntityFramework
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AirportContext>() 
.AddDefaultTokenProviders();

// ✅ Configuration des cookies d'authentification
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// ✅ Activation de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ✅ Ajout du support MVC avec vues
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ✅ Configuration de l'environnement
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// ✅ Configuration WebSocket
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120)
});

app.UseCors("AllowAll");

// ✅ Middleware WebSocket
app.Use(async (context, next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await HandleWebSocket(context, webSocket);
    }
    else
    {
        await next();
    }
});

app.UseRouting();

// Configuration Authentication/Authorization
app.UseAuthentication();
app.UseAuthorization();

// Initialisation des rôles et admin
using (var scope = app.Services.CreateScope())
{
    try
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Création des rôles
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Création admin par défaut
        var adminEmail = "airport@gmail.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "System",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.Parse("2025-05-22 18:33:27")
            };

            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, Roles.Administrator);
            }
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Une erreur s'est produite lors de l'initialisation des rôles et de l'administrateur.");
    }
}

//  Configuration des routes
app.MapControllerRoute(
    name: "planes",
    pattern: "Planes/{action=Index}/{id?}",
    defaults: new { controller = "Planes" });

app.MapControllerRoute(
    name: "flights",
    pattern: "Flights/{action=Index}/{id?}",
    defaults: new { controller = "Flights" });

app.MapControllerRoute(
    name: "passengers",
    pattern: "Passengers/{action=Index}/{id?}",
    defaults: new { controller = "Passengers" });

app.MapControllerRoute(
    name: "staffs",
    pattern: "Staffs/{action=Index}/{id?}",
    defaults: new { controller = "Staffs" });

app.MapControllerRoute(
    name: "travellers",
    pattern: "Travellers/{action=Index}/{id?}",
    defaults: new { controller = "Travellers" });

app.MapControllerRoute(
    name: "tickets",
    pattern: "Tickets/{action=Index}/{id?}",
    defaults: new { controller = "Tickets" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// ✅ Gestion WebSocket
static async Task HandleWebSocket(HttpContext context, WebSocket webSocket)
{
    try
    {
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!result.CloseStatus.HasValue)
        {
            await webSocket.SendAsync(
                new ArraySegment<byte>(buffer, 0, result.Count),
                result.MessageType,
                result.EndOfMessage,
                CancellationToken.None);

            result = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(
            result.CloseStatus.Value,
            result.CloseStatusDescription,
            CancellationToken.None);
    }
    catch (Exception ex)
    {
        // Log l'erreur
        if (webSocket.State == WebSocketState.Open)
        {
            await webSocket.CloseAsync(
                WebSocketCloseStatus.InternalServerError,
                "Une erreur s'est produite",
                CancellationToken.None);
        }
    }
}