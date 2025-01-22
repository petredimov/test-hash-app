using Hash.Data.ViewModels;

namespace Hash.Data.Services;

public interface IHashAnalyticsService
{
    GetHashesViewModel GetHashAnalytics();
    Task CheckHashAnalytics(long ticks);
}