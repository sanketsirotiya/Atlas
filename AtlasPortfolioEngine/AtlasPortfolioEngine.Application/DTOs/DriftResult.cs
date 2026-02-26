namespace AtlasPortfolioEngine.Application.DTOs;

public class DriftResult
{
    public string AssetClass { get; set; } = string.Empty;
    public decimal TargetWeight { get; set; }
    public decimal ActualWeight { get; set; }
    public decimal DriftPercentage => ActualWeight - TargetWeight;
    public bool RequiresRebalancing { get; set; }
}