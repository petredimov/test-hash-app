using Hash.Data.Context;
using Hash.Data.Context.Entities;
using Hash.Data.ViewModels;

namespace Hash.Data.Services;

public class HashAnalyticsService (DatabaseContext context) : IHashAnalyticsService
{
    public GetHashesViewModel GetHashAnalytics()
    {
        var hashes = new List<HashAnalyticsViewModel>();
        foreach (var item in context.HashAnalytics)
        {
            hashes.Add(new HashAnalyticsViewModel()
            {
                Date = new DateTime(item.DateTicks).ToString("yyyy-MM-dd"),
                Count = item.Count,
            });
        }

        return new GetHashesViewModel()
        {
            Hashes = hashes
        };
    }

    public async Task CheckHashAnalytics(long ticks)
    {
        var hashAnalytics = context.HashAnalytics.FirstOrDefault(x => x.DateTicks == ticks);

        if (hashAnalytics == null)
        {
            context.HashAnalytics.Add(new HashAnalyticsModel()
            {
                DateTicks = ticks, Count = 1
            });
        }
        else
        {
            hashAnalytics.Count += 1;
        }
        
        await context.SaveChangesAsync();
    }
}