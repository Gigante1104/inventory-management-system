using System.Security.Claims;
using System.Text;
using InventarioApi.Data;
using InventarioApi.DTOs;
using InventarioApi.Exceptions;
using InventarioApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace InventarioApi.Services;

public interface IAuthService
{
    Task<AuthRespuestaDto> RegistrarAsync(RegistroDto dto);
    Task<AuthRespuestaDto> LoginAsync(LoginDto dto);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AuthRespuestaDto> RegistrarAsync(RegistroDto dto)
    {
        var nombre = dto.NombreUsuario.Trim();
        if (await _db.Usuarios.AnyAsync(u => u.NombreUsuario == nombre))
            throw new ConflictException("Ese nombre de usuario ya está en uso");

        var usuario = new Usuario
        {
            NombreUsuario = nombre,
            // BCrypt genera un hash con "sal" aleatoria: nunca se guarda la contraseña real
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();
        return GenerarToken(usuario);
    }

    public async Task<AuthRespuestaDto> LoginAsync(LoginDto dto)
    {
        var nombre = dto.NombreUsuario.Trim();
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombre);

        // Mismo mensaje si falla el usuario o la clave, para no dar pistas a un atacante
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
            throw new UnauthorizedAppException("Usuario o contraseña incorrectos");

        return GenerarToken(usuario);
    }

    private AuthRespuestaDto GenerarToken(Usuario usuario)
    {
        var clave = _config["Jwt:Key"]!;
        var minutos = _config.GetValue("Jwt:ExpireMinutes", 60);
        var expira = DateTime.UtcNow.AddMinutes(minutos);

        var descriptor = new SecurityTokenDescriptor
        {
            // Claims = datos que viajan dentro del token
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.Role, usuario.Rol)
            }),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            Expires = expira,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)),
                SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return new AuthRespuestaDto { Token = token, NombreUsuario = usuario.NombreUsuario, Expira = expira };
    }
}
