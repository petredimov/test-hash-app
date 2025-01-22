using Hash.Data.Context.Entities;
using Hash.Data.Events;
using Hash.Data.Services;
using MassTransit;

namespace Hash.Data.Consumer;

public class HashConsumer (IHashService hashService, IHashAnalyticsService hashAnalyticsService) : IConsumer<HashEvent>
{
    public async Task Consume(ConsumeContext<HashEvent> context)
    {
        // Generate date random from UTC today + 1-10days
        var random = new Random();
        var events = context.Message.Data.Select(message => new HashModel() { Sha1 = message, Date = DateTime.UtcNow.AddDays(random.Next(1, 10)), }).ToList();
        
        var groupedEvents = events.GroupBy(x => x.Date.Date);

        // Set hash date analytics before enter the data to database
        foreach (var groupEvent in groupedEvents)
        {
            await hashAnalyticsService.CheckHashAnalytics(groupEvent.Key.Date.Ticks);
        }
        
        await hashService.AddHashListAsync(events);
    }
}