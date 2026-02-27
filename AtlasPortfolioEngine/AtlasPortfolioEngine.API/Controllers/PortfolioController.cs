using AtlasPortfolioEngine.Application.Interfaces;
using AtlasPortfolioEngine.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;

namespace AtlasPortfolioEngine.API.Controllers;

[ApiController]
//[Route("api/[controller]")]
[Route("api/v{version:apiVersion}/portfolio")]
[ApiVersion("1.0")]
public class PortfolioController : ControllerBase
{
    private readonly AtlasDbContext _context;
    private readonly IModelPortfolioService _modelPortfolioService;
    private readonly IDriftDetectionService _driftDetectionService;

    public PortfolioController(
        AtlasDbContext context,
        IModelPortfolioService modelPortfolioService,
        IDriftDetectionService driftDetectionService)
    {
        _context = context;
        _modelPortfolioService = modelPortfolioService;
        _driftDetectionService = driftDetectionService;
    }

    // GET api/portfolio/{clientId}
    [HttpGet("{clientId:guid}")]
    public async Task<IActionResult> GetPortfolio(Guid clientId)
    {
        var account = await _context.Accounts
            .Include(a => a.Holdings)
            .FirstOrDefaultAsync(a => a.ClientId == clientId);

        if (account == null) return NotFound();

        var totalMarketValue = account.Holdings.Sum(h => h.MarketValue);
        var totalCost = account.Holdings.Sum(h => h.AverageCost * h.Quantity);
        var totalGainLoss = totalMarketValue - totalCost;

        return Ok(new
        {
            AccountId = account.Id,
            CashBalance = account.CashBalance,
            TotalMarketValue = totalMarketValue,
            TotalCost = totalCost,
            TotalUnrealizedGainLoss = totalGainLoss,
            ReturnPercentage = totalCost > 0 ? Math.Round(totalGainLoss / totalCost * 100, 2) : 0,
            Holdings = account.Holdings.Select(h => new
            {
                h.Symbol,
                h.AssetClass,
                h.Quantity,
                h.AverageCost,
                h.CurrentPrice,
                MarketValue = h.MarketValue,
                UnrealizedGainLoss = h.UnrealizedGainLoss,
                Weight = totalMarketValue > 0
                    ? Math.Round(h.MarketValue / totalMarketValue * 100, 2)
                    : 0
            })
        });
    }

    // GET api/portfolio/{clientId}/drift
    [HttpGet("{clientId:guid}/drift")]
    public async Task<IActionResult> GetDrift(Guid clientId)
    {
        var client = await _context.Clients
            .Include(c => c.RiskProfile)
            .Include(c => c.Account)
            .ThenInclude(a => a!.Holdings)
            .FirstOrDefaultAsync(c => c.Id == clientId);

        if (client == null) return NotFound();
        if (client.RiskProfile == null) return BadRequest("Client has no risk profile. Complete risk assessment first.");
        if (client.Account == null) return BadRequest("Client has no account.");

        var targetAllocations = _modelPortfolioService.GetTargetAllocations(client.RiskProfile.Category);
        var driftResults = _driftDetectionService.Detect(client.Account, targetAllocations);

        var requiresRebalancing = driftResults.Any(d => d.RequiresRebalancing);

        return Ok(new
        {
            ClientId = clientId,
            RiskCategory = client.RiskProfile.Category.ToString(),
            RequiresRebalancing = requiresRebalancing,
            Drift = driftResults.Select(d => new
            {
                d.AssetClass,
                TargetWeight = Math.Round(d.TargetWeight * 100, 2),
                ActualWeight = Math.Round(d.ActualWeight * 100, 2),
                DriftPercentage = Math.Round(d.DriftPercentage * 100, 2),
                d.RequiresRebalancing
            })
        });
    }
}