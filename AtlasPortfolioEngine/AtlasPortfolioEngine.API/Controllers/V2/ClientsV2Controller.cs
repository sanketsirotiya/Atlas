using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtlasPortfolioEngine.Infrastructure.Persistence;

namespace AtlasPortfolioEngine.API.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/clients")]
[ApiVersion("2.0")]
[Authorize]
public class ClientsV2Controller : ControllerBase
{
    private readonly AtlasDbContext _context;

    public ClientsV2Controller(AtlasDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetClients()
    {
        var clients = await _context.Clients
            .Include(c => c.RiskProfile)
            .ToListAsync();

        var response = new
        {
            ApiVersion = "2.0",
            TotalCount = clients.Count,
            Data = clients.Select(c => new
            {
                c.Id,
                c.FullName,
                c.Email,
                RiskProfile = c.RiskProfile == null ? null : new
                {
                    c.RiskProfile.Score,
                    Category = c.RiskProfile.Category.ToString(),
                    c.RiskProfile.AssessedAt
                },
                Links = new
                {
                    Self = $"/api/v2/clients/{c.Id}",
                    Portfolio = $"/api/v2/clients/{c.Id}/portfolio",
                    Drift = $"/api/v2/clients/{c.Id}/drift"
                }
            })
        };

        return Ok(response);
    }
}