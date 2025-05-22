using GestionAirPort.data;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);

// ✅ Configuration de la connexion à SQL Server
builder.Services.AddDbContext<AirportContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// ✅ Ajout du support MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ✅ Gestion des erreurs et HTTPS
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// ✅ Activation WebSocket avec options
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120)
});

// ✅ Activation CORS
app.UseCors("AllowAll");

// ✅ Middleware WebSocket : accepte les connexions
app.Use(async (context, next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await HandleWebSocket(context, webSocket); // Fonction de gestion
    }
    else
    {
        await next();
    }
});

app.UseRouting();
app.UseAuthorization();

// ✅ Routes spécifiques pour les contrôleurs
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

// ✅ Route par défaut
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// ✅ Méthode de gestion WebSocket simple (echo)
static async Task HandleWebSocket(HttpContext context, WebSocket webSocket)
{
    var buffer = new byte[1024 * 4];
    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

    while (!result.CloseStatus.HasValue)
    {
        await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count),
            result.MessageType, result.EndOfMessage, CancellationToken.None);

        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    }

    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
}
