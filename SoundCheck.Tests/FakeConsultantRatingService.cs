using Soundcheck.Web.Services;

namespace Soundcheck.Tests;

public class FakeConsultantRatingService(IReadOnlyDictionary<int, decimal> averageRatings) : IConsultantRatingService
{
    public FakeConsultantRatingService()
        : this(new Dictionary<int, decimal>())
    {
    }

    public Task<IReadOnlyDictionary<int, decimal>> GetAverageRatingsAsync(IEnumerable<int> consultantProfileIds)
    {
        return Task.FromResult(averageRatings);
    }
}
