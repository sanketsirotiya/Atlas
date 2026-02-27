using AtlasPortfolioEngine.Application.Interfaces;
using AtlasPortfolioEngine.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;

namespace AtlasPortfolioEngine.API.Controllers;

[ApiController]
//[Route("api/[controller]")]
[Route("api/v{version:apiVersion}/rebalance")]
[ApiVersion("1.0")]
public class RebalanceController : ControllerBase
{
    private readonly AtlasDbContext _context;
    private readonly IModelPortfolioService _modelPortfolioService;
    private readonly IDriftDetectionService _driftDetectionService;
    private readonly IRebalancingService _rebalancingService;

    public RebalanceController(
        AtlasDbContext context,
        IModelPortfolioService modelPortfolioService,
        IDriftDetectionService driftDetectionService,
        IRebalancingService rebalancingService)
    {
        _context = context;
        _modelPortfolioService = modelPortfolioService;
        _driftDetectionService = driftDetectionService;
        _rebalancingService = rebalancingService;
    }

    // GET api/rebalance/{clientId}/preview
    [HttpGet("{clientId:guid}/preview")]
    public async Task<IActionResult> Preview(Guid clientId)
    {
        var client = await _context.Clients
            .Include(c => c.RiskProfile)
            .Include(c => c.Account)
            .ThenInclude(a => a!.Holdings)
            .FirstOrDefaultAsync(c => c.Id == clientId);

        if (client == null) return NotFound();
        if (client.RiskProfile == null) return BadRequest("Client has no risk profile.");
        if (client.Account == null) return BadRequest("Client has no account.");

        var targetAllocations = _modelPortfolioService.GetTargetAllocations(client.RiskProfile.Category);
        var driftResults = _driftDetectionService.Detect(client.Account, targetAllocations);

        if (!driftResults.Any(d => d.RequiresRebalancing))
            return Ok(new { Message = "Portfolio is within acceptable drift thresholds. No rebalancing required." });

        var orders = _rebalancingService.GenerateOrders(client.Account, driftResults);

        return Ok(new
        {
            ClientId = clientId,
            RiskCategory = client.RiskProfile.Category.ToString(),
            Orders = orders.Select(o => new
            {
                o.AssetClass,
                o.Symbol,
                OrderType = o.OrderType.ToString(),
                Amount = Math.Round(o.Amount, 2)
            })
        });
    }

    // POST api/rebalance/{clientId}/execute
    [HttpPost("{clientId:guid}/execute")]
    public async Task<IActionResult> Execute(Guid clientId)
    {
        var client = await _context.Clients
            .Include(c => c.RiskProfile)
            .Include(c => c.Account)
            .ThenInclude(a => a!.Holdings)
            .FirstOrDefaultAsync(c => c.Id == clientId);

        if (client == null) return NotFound();
        if (client.RiskProfile == null) return BadRequest("Client has no risk profile.");
        if (client.Account == null) return BadRequest("Client has no account.");

        var targetAllocations = _modelPortfolioService.GetTargetAllocations(client.RiskProfile.Category);
        var driftResults = _driftDetectionService.Detect(client.Account, targetAllocations);

        if (!driftResults.Any(d => d.RequiresRebalancing))
            return Ok(new { Message = "Portfolio is within acceptable drift thresholds. No rebalancing required." });

        var orders = _rebalancingService.GenerateOrders(client.Account, driftResults).ToList();

        // Record each rebalance order as a transaction
        foreach (var order in orders)
        {
            var transaction = Domain.Entities.Transaction.Record(
                client.Account.Id,
                order.Symbol,
                order.OrderType,
                quantity: 1, // simplified — in real system would calculate shares from amount
                price: order.Amount
            );
            await _context.Transactions.AddAsync(transaction);
        }

        // Audit the rebalance event
        var audit = Domain.Entities.AuditLog.Record(
            clientId,
            "REBALANCE_EXECUTED",
            $"Rebalance executed. {orders.Count} orders generated for {client.RiskProfile.Category} portfolio."
        );
        await _context.AuditLogs.AddAsync(audit);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = $"Rebalance executed. {orders.Count} orders processed.",
            Orders = orders.Select(o => new
            {
                o.AssetClass,
                o.Symbol,
                OrderType = o.OrderType.ToString(),
                Amount = Math.Round(o.Amount, 2)
            })
        });
    }
}