using AtlasPortfolioEngine.Application.DTOs;
using AtlasPortfolioEngine.Domain.Entities;
using AtlasPortfolioEngine.Domain.Enums;

namespace AtlasPortfolioEngine.Application.Interfaces;

public interface ISuitabilityService
{
    SuitabilityResult Check(RiskProfile riskProfile, string assetClass, TransactionType transactionType, decimal amount, decimal totalPortfolioValue);
}