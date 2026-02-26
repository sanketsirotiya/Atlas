using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AtlasPortfolioEngine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // POST api/auth/token
   [HttpPost("token")]
    public IActionResult GetToken([FromBody] LoginRequest request)
    {
        var validUsername = _configuration["Jwt:DemoUsername"];
        var validPassword = _configuration["Jwt:DemoPassword"];

        if (request.Username != validUsername || request.Password != validPassword)
            return Unauthorized(new { Message = "Invalid credentials." });

        var token = GenerateJwtToken();
        return Ok(new
        {
            Token = token,
            ExpiresIn = 3600,
            TokenType = "Bearer",
            User = new { Username = request.Username, Role = "Advisor" }
        });
    }
    private string GenerateJwtToken()
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "advisor"),
            new Claim(ClaimTypes.Role, "Advisor"),
            new Claim("system", "AtlasPortfolioEngine")
        };

        var token = new JwtSecurityToken(
            issuer:   _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims:   claims,
            expires:  DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record LoginRequest(string Username, string Password);