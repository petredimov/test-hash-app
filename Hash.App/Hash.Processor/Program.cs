using Hash.Data.Registrations;
using Hash.Data.Settings;
using Hash.Processor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.RegisterDatabaseContext(builder.Configuration);
builder.Services.RegisterServices();
builder.Services.RegisterServices();

// Use built-in consumer HashProcess
builder.Services.RegisterConsumers(builder.Configuration);

// Use hosted processor
// builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQSettings"));
// builder.Services.AddHostedService<Processor>();

IHost host = builder.Build();
host.Run();
