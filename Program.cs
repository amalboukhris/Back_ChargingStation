using ChargingStation.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.SignalR;

using System.Net;
using System.Net.WebSockets;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using System.Reflection;
using ChargingStation.Hubs;

using ChargingStation.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configuration de la base de données PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Ajout des services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });


builder.Services.AddEndpointsApiExplorer();

// Configuration Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Charging Station API",
        Version = "v1",
        Description = "API for OCPP-compliant charging stations"
    });
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    // Include XML comments if available
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // JWT Bearer Authentication configuration
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Authentification JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])
            )
        };

        // Configuration pour SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/ocppHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddScoped<IChargePointService, ChargePointService>();
// Add this with your other service registrations
builder.Services.AddScoped<IOcppProtocolService, OcppProtocolService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
// Add this with your other service registrations
builder.Services.AddScoped<IFirmwareService, FirmwareService>();
//builder.Services.AddScoped<IReservationService, ReservationService>();
// Configuration CORS optimisée
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllClients", policy =>
    {
        policy.WithOrigins(
            "http://localhost",
            "http://localhost:3000",      // React
            "http://localhost:5173",      // React (Vite)
            "http://localhost:7080",      // Backend local
            "http://127.0.0.1:7080",      // Flutter Web
            "http://10.0.2.2:7080",       // Flutter Android emulator
            "http://192.168.0.10:7080",   // (Remplace par IP locale réelle pour mobile sur Wi-Fi)
            "https://votrefrontend.com"   // Domaine déployé
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});


// Configuration Kestrel
// Update the Kestrel configuration
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Port HTTP (pour l'émulateur Android)
    serverOptions.Listen(IPAddress.Any, 7080);

    // Port HTTPS (si nécessaire)
    serverOptions.Listen(IPAddress.Any, 7081, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Services.AddSignalR(options => {
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
});

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Charging Station API v1");
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
    });
}

app.UseRouting();

// IMPORTANT: L'ordre des middlewares est crucial
app.UseCors("AllowAllClients");

app.UseAuthentication();
app.UseAuthorization();

// Configuration WebSocket
app.UseWebSockets(new Microsoft.AspNetCore.Builder.WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30)
});

//// Middleware personnalisé pour les connexions OCPP
//app.Use(async (context, next) =>
//{
//    if (context.Request.Path.StartsWithSegments("/ocpp") &&
//        context.WebSockets.IsWebSocketRequest)
//    {
//        var chargePointId = context.Request.Path.Value?.Split('/').Last();
//        if (!string.IsNullOrEmpty(chargePointId))
//        {
//            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
//            var hubContext = context.RequestServices.GetRequiredService<IHubContext<ChargingHub>>();
//            await HandleOcppConnection(webSocket, chargePointId, hubContext);
//        }
//    }
//    else
//    {
//        await next();
//    }
//});
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new
        {
            error = ex.Message,
            stackTrace = app.Environment.IsDevelopment() ? ex.StackTrace : null
        });
    }
});


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<NotificationHub>("/notifications");
    endpoints.MapHub<ChargingHub>("/chargingHub");
    


});


async Task HandleOcppConnection(WebSocket webSocket, string chargePointId, IHubContext<ChargingHub> hubContext)
{
    var buffer = new byte[1024 * 4];
    try
    {
        // Ajouter au groupe SignalR
        await hubContext.Groups.AddToGroupAsync(chargePointId, chargePointId);

        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        while (!result.CloseStatus.HasValue)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            await hubContext.Clients.Group(chargePointId).SendAsync("ProcessOcppMessage", chargePointId, message);
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erreur WebSocket: {ex.Message}");
    }
    finally
    {
        await hubContext.Groups.RemoveFromGroupAsync(chargePointId, chargePointId);
    }
}

app.Run();