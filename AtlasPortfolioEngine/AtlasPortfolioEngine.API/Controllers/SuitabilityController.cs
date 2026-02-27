using AtlasPortfolioEngine.Application.Interfaces;
using AtlasPortfolioEngine.Domain.Enums;
using AtlasPortfolioEngine.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;

namespace AtlasPortfolioEngine.API.Controllers;

[ApiController]
//[Route("api/[controller]")]
[Route("api/v{version:apiVersion}/suitability")]
[ApiVersion("1.0")]
public class SuitabilityController : ControllerBase
{
    private readonly AtlasDbContext _context;
    private readonly ISuitabilityService _suitabilityService;

    public SuitabilityController(AtlasDbContext context, ISuitabilityService suitabilityService)
    {
        _context = context;
        _suitabilityService = suitabilityService;
    }

    // POST api/suitability/check
    [HttpPost("check")]
    public async Task<IActionResult> Check([FromBody] SuitabilityCheckRequest request)
    {
        var client = await _context.Clients
            .Include(c => c.RiskProfile)
            .Include(c => c.Account)
            .ThenInclude(a => a!.Holdings)
            .FirstOrDefaultAsync(c => c.Id == request.ClientId);

        if (client == null) return NotFound("Client not found.");
        if (client.RiskProfile == null) return BadRequest("Client has no risk profile. Complete risk assessment first.");
        if (client.Account == null) return BadRequest("Client has no account.");

        if (!Enum.TryParse<TransactionType>(request.TransactionType, true, out var transactionType))
            return BadRequest($"Invalid transaction type. Valid values: {string.Join(", ", Enum.GetNames<TransactionType>())}");

        var totalPortfolioValue = client.Account.Holdings.Sum(h => h.MarketValue);

        var result = _suitabilityService.Check(
            client.RiskProfile,
            request.AssetClass,
            transactionType,
            request.Amount,
            totalPortfolioValue
        );

        // Audit every suitability check — required in regulated environments
        var audit = Domain.Entities.AuditLog.Record(
            client.Id,
            result.IsApproved ? "SUITABILITY_APPROVED" : "SUITABILITY_REJECTED",
            $"Asset: {request.AssetClass}, Type: {request.TransactionType}, Amount: {request.Amount:C}, " +
            $"ReasonCode: {result.ReasonCode}"
        );
        await _context.AuditLogs.AddAsync(audit);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            result.IsApproved,
            result.ReasonCode,
            result.Message,
            ClientRiskCategory = client.RiskProfile.Category.ToString(),
            ClientRiskScore = client.RiskProfile.Score
        });
    }
}

public record SuitabilityCheckRequest(
    Guid ClientId,
    string AssetClass,
    string TransactionType,
    decimal Amount
);