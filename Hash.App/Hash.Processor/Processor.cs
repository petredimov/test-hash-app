using System.Text;
using Hash.Data.Context.Entities;
using Hash.Data.Events;
using Hash.Data.Services;
using Hash.Data.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Hash.Processor;

public class Processor : BackgroundService
{
    private IConnection _connection;
    private IChannel _channel;
    
    private readonly ILogger<Processor> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHashService _hashService;
    private readonly IHashAnalyticsService _hashAnalyticsService;
    private readonly RabbitMqSettings _rabbitMqSettings;
    
    public Processor(ILogger<Processor> logger, IServiceProvider serviceProvider, IHashService hashService, IHashAnalyticsService hashAnalyticsService, IOptions<RabbitMqSettings> rabbitMqSetting)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _hashService = hashService;
        _hashAnalyticsService = hashAnalyticsService;
        _rabbitMqSettings = rabbitMqSetting.Value;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqSettings.Host,
            UserName = _rabbitMqSettings.Username,
            Password = _rabbitMqSettings.Password
        };
        
        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
        
        await StartConsuming(_rabbitMqSettings.QueueName, stoppingToken);
        await Task.CompletedTask;
    }
    
    private async Task StartConsuming(string queueName, CancellationToken cancellationToken)
    { 
        await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            var processedSuccessfully = false;
            
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    processedSuccessfully = await ProcessMessageAsync(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occurred while processing message from queue {queueName}: {ex}");
            }

            if (processedSuccessfully)
            {
                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
            }
            else
            {
                await _channel.BasicRejectAsync(deliveryTag: ea.DeliveryTag, requeue: true, cancellationToken: cancellationToken);
            }
        };

        await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);
        
    }
    
    private async Task<bool> ProcessMessageAsync(string message)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var hashEvent = JsonConvert.DeserializeObject<HashEvent>(message);

                    var random = new Random();
                    var events = hashEvent.Data.Select(message => new HashModel() { Sha1 = message, Date = DateTime.UtcNow.AddDays(random.Next(1, 10)), }).ToList();
        
                    var groupedEvents = events.GroupBy(x => x.Date.Date);

                    // Set hash date analytics before enter the data to database
                    foreach (var groupEvent in groupedEvents)
                    {
                        await _hashAnalyticsService.CheckHashAnalytics(groupEvent.Key.Date.Ticks);
                    }
        
                    await _hashService.AddHashListAsync(events);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing message: {ex.Message}");
                return false;
            }
        }

        public override void Dispose()
        {
            _channel.CloseAsync();
            _connection.CloseAsync();
            base.Dispose();
        }
}