namespace Soundcheck.Web.Services;

public interface IConsultantRatingService
{
    Task<IReadOnlyDictionary<int, decimal>> GetAverageRatingsAsync(IEnumerable<int> consultantProfileIds);
}
