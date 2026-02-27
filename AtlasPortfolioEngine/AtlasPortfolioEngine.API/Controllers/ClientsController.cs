using AtlasPortfolioEngine.Application.Interfaces;
using AtlasPortfolioEngine.Domain.Enums;
using AtlasPortfolioEngine.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning; 


namespace AtlasPortfolioEngine.API.Controllers;

[ApiController]
//[Route("api/[controller]")]
[Route("api/clients")]
[Route("api/v{version:apiVersion}/clients")]
[ApiVersion("1.0")]
public class ClientsController : ControllerBase
{
    private readonly AtlasDbContext _context;
    private readonly IRiskProfileService _riskProfileService;

    public ClientsController(AtlasDbContext context, IRiskProfileService riskProfileService)
    {
        _context = context;
        _riskProfileService = riskProfileService;
    }

    // GET api/clients
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clients = await _context.Clients
            .Include(c => c.RiskProfile)
            .Select(c => new
            {
                c.Id,
                c.FullName,
                c.Email,
                c.CreatedAt,
                RiskProfile = c.RiskProfile == null ? null : new
                {
                    c.RiskProfile.Score,
                    Category = c.RiskProfile.Category.ToString(),
                    c.RiskProfile.AssessedAt
                }
            })
            .ToListAsync();

        return Ok(clients);
    }

    // GET api/clients/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var client = await _context.Clients
            .Include(c => c.RiskProfile)
            .Include(c => c.Account)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null) return NotFound();

        return Ok(new
        {
            client.Id,
            client.FullName,
            client.Email,
            client.DateOfBirth,
            client.CreatedAt,
            RiskProfile = client.RiskProfile == null ? null : new
            {
                client.RiskProfile.Score,
                Category = client.RiskProfile.Category.ToString(),
                client.RiskProfile.AssessedAt
            },
            Account = client.Account == null ? null : new
            {
                client.Account.Id,
                client.Account.CashBalance
            }
        });
    }

    // POST api/clients/{id}/risk-assessment
    [HttpPost("{id:guid}/risk-assessment")]
    public async Task<IActionResult> AssessRisk(Guid id, [FromBody] List<int> answers)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null) return NotFound();

        var riskProfile = _riskProfileService.Assess(id, answers);

        // Remove old profile if exists
        var existing = await _context.RiskProfiles.FirstOrDefaultAsync(r => r.ClientId == id);
        if (existing != null) _context.RiskProfiles.Remove(existing);

        await _context.RiskProfiles.AddAsync(riskProfile);

        // Audit
        var audit = Domain.Entities.AuditLog.Record(id, "RISK_ASSESSMENT",
            $"Risk score: {riskProfile.Score}, Category: {riskProfile.Category}");
        await _context.AuditLogs.AddAsync(audit);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            riskProfile.Score,
            Category = riskProfile.Category.ToString(),
            riskProfile.AssessedAt
        });
    }
}