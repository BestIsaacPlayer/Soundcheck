using Microsoft.EntityFrameworkCore;
using Soundcheck.Web.Data;

namespace Soundcheck.Web.Services;

public class ConsultantRatingService(ApplicationDbContext databaseContext) : IConsultantRatingService
{
    public async Task<IReadOnlyDictionary<int, decimal>> GetAverageRatingsAsync(IEnumerable<int> consultantProfileIds)
    {
        var requestedProfileIds = consultantProfileIds.ToList();

        var ratings = await databaseContext.ConsultantProfiles
            .AsNoTracking()
            .Where(profile => requestedProfileIds.Contains(profile.Id))
            .Select(profile => new
            {
                profile.Id,
                AverageRating = ApplicationDbContext.ConsultantAverageRating()
            })
            .ToListAsync();

        return ratings
            .Where(rating => rating.AverageRating.HasValue)
            .ToDictionary(rating => rating.Id, rating => rating.AverageRating!.Value);
    }
}
