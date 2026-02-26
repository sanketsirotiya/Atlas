using AtlasPortfolioEngine.Application.Interfaces;
using AtlasPortfolioEngine.Domain.Entities;

namespace AtlasPortfolioEngine.Application.Services;

public class RiskProfileService : IRiskProfileService
{
    // Each answer is 1-5 (5 = highest risk tolerance)
    // Normalized to 0-100
    private const int MaxRawScore = 25;

    public RiskProfile Assess(Guid clientId, IEnumerable<int> questionnaireAnswers)
    {
        var answers = questionnaireAnswers.ToList();

        if (answers.Count == 0) throw new ArgumentException("Questionnaire answers are required.");
        if (answers.Any(a => a < 1 || a > 5)) throw new ArgumentException("Each answer must be between 1 and 5.");

        var rawScore = answers.Sum();
        var normalizedScore = (int)Math.Round((double)rawScore / (answers.Count * 5) * 100);

        return RiskProfile.Create(clientId, normalizedScore);
    }
}