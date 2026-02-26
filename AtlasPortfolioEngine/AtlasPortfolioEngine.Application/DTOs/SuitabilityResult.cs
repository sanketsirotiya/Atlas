namespace AtlasPortfolioEngine.Application.DTOs;

public class SuitabilityResult
{
    public bool IsApproved { get; set; }
    public string ReasonCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public static SuitabilityResult Approved() => new()
    {
        IsApproved = true,
        ReasonCode = "APPROVED",
        Message = "Trade is suitable for client risk profile."
    };

    public static SuitabilityResult Rejected(string reasonCode, string message) => new()
    {
        IsApproved = false,
        ReasonCode = reasonCode,
        Message = message
    };
}