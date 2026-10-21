using InventarioApi.DTOs;
using InventarioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventarioApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    // POST api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult<AuthRespuestaDto>> Registrar(RegistroDto dto) =>
        Ok(await _auth.RegistrarAsync(dto));

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<AuthRespuestaDto>> Login(LoginDto dto) =>
        Ok(await _auth.LoginAsync(dto));
}
