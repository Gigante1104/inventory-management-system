using System.Text;
using InventarioApi.Data;
using InventarioApi.Middleware;
using InventarioApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// --- Configuración obligatoria (viene de User Secrets en desarrollo) ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Falta ConnectionStrings:DefaultConnection. Configúrala con 'dotnet user-secrets set'.");

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
    throw new InvalidOperationException(
        "Falta Jwt:Key o tiene menos de 32 caracteres. Configúrala con 'dotnet user-secrets set'.");

// --- Servicios ---
builder.Services.AddDbContext<AppDbContext>(opciones => opciones.UseNpgsql(connectionString));

builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllers();

// Autenticación JWT: valida firma, emisor, audiencia y expiración del token
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

// CORS: permite que el frontend (otro puerto) llame a la API
var origenesPermitidos = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173" };
builder.Services.AddCors(opciones =>
    opciones.AddDefaultPolicy(politica =>
        politica.WithOrigins(origenesPermitidos).AllowAnyHeader().AllowAnyMethod()));

// Swagger con botón "Authorize" para pegar el token
builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo { Title = "Inventario API", Version = "v1" });
    opciones.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Pega solo el token (sin la palabra Bearer)"
    });
    opciones.AddSecurityRequirement(documento => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", documento)] = new List<string>()
    });
});

var app = builder.Build();

// --- Pipeline (el orden importa) ---
app.UseMiddleware<ErrorHandlingMiddleware>(); // primero, para atrapar errores de todo lo demás

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication(); // ¿quién eres?
app.UseAuthorization();  // ¿tienes permiso?
app.MapControllers();

app.Run();

// Hace pública la clase Program para que el proyecto de pruebas pueda arrancar la API
public partial class Program { }