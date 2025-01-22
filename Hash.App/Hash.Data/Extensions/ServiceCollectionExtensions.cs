using Hash.Data.Consumer;
using Hash.Data.Context;
using Hash.Data.Services;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MySql.Data.EntityFrameworkCore.Extensions;

namespace Hash.Data.Registrations;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("Database");

        services.AddDbContext<DatabaseContext>(options =>
            options.UseMySql(connection, ServerVersion.AutoDetect(connection)));

        return services;
    }
    
    
    public static IServiceCollection RegisterConsumers(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.SetKebabCaseEndpointNameFormatter();

            var numOfConsumers = int.Parse(configuration["RabbitMQSettings:ConcurrencyLimit"]);

            for (var i = 0; i < numOfConsumers; i++)
            {
                busConfigurator.AddConsumer<HashConsumer>()
                    .Endpoint(e => e.Name = configuration["RabbitMQSettings:QueueName"]);
            }
            

            busConfigurator.UsingRabbitMq((context, configurator) =>
            {
                configurator.UseMessageRetry(retryConfig =>
                {
                    retryConfig.Interval(5, TimeSpan.FromSeconds(10)); // Retry up to 5 times, every 10 seconds
                });
                
                // Hash Processor
                configurator.ReceiveEndpoint(configuration["RabbitMQSettings:QueueName"],
                    e =>
                    {
                        e.ConfigureConsumer<HashConsumer>(context);
                        e.UseConcurrencyLimit(numOfConsumers);
                    });
                
                configurator.Host(configuration["RabbitMQSettings:Host"], "/", h =>
                {
                    h.Heartbeat(10);
                    h.Username(configuration["RabbitMQSettings:Username"]);
                    h.Password(configuration["RabbitMQSettings:Password"]);
                });
            });
        });

        return services;
    }
    
    public static IServiceCollection RegisterRabbitMqUsage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.SetKebabCaseEndpointNameFormatter();

            busConfigurator.UsingRabbitMq((context, configurator) =>
            {
                configurator.UseConcurrencyLimit(int.Parse(configuration["RabbitMQSettings:ConcurrencyLimit"]));
                configurator.Host(configuration["RabbitMQSettings:Host"], "/", h =>
                {
                    h.Username(configuration["RabbitMQSettings:Username"]);
                    h.Password(configuration["RabbitMQSettings:Password"]);
                });

                configurator.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddTransient<IHashService, HashService>();
        services.AddTransient<IHashAnalyticsService, HashAnalyticsService>();
        
        return services;
    }
}