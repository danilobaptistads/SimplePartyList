using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SimplePartyList.Core.DTOs;
using SimplePartyList.Core.Entities;

namespace SimplePartyList.Web.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<Admin> _userManager;
    private readonly SignInManager<Admin> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<Admin> userManager,
        SignInManager<Admin> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    [Authorize]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var admin = new Admin
        {
            UserName = dto.Email,
            Email = dto.Email,
            Name = dto.Name
        };

        var result = await _userManager.CreateAsync(admin, dto.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = "Administrador registrado com sucesso." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var admin = await _userManager.FindByEmailAsync(dto.Email);

        if (admin is null)
            return Unauthorized(new { message = "Credenciais inv�lidas." });

        var signInResult = await _signInManager.CheckPasswordSignInAsync(admin, dto.Password, false);

        if (!signInResult.Succeeded)
            return Unauthorized(new { message = "Credenciais inv�lidas." });

        var token = await GenerateJwtAsync(admin);

        return Ok(token);
    }

    private async Task<AuthResponseDto> GenerateJwtAsync(Admin admin)
    {
        var jwtKey = _configuration["Jwt:Key"]!;
        var issuer = _configuration["Jwt:Issuer"]!;
        var audience = _configuration["Jwt:Audience"]!;
        var expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, admin.Id),
            new(ClaimTypes.Email, admin.Email!),
            new(ClaimTypes.Name, admin.Name),
        };

        var roles = await _userManager.GetRolesAsync(admin);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expire = DateTime.UtcNow.AddMinutes(expireMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expire,
            signingCredentials: creds);

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expire = expire
        };
    }
}
