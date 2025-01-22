using Hash.Data.Context;
using Hash.Data.Registrations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.RegisterDatabaseContext(builder.Configuration);
builder.Services.RegisterServices();
builder.Services.AddControllers();

builder.Services.RegisterConsumers(builder.Configuration);
// builder.Services.RegisterRabbitMqUsage(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//execute db migrations
var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
var context = serviceScope.ServiceProvider.GetService<DatabaseContext>();
context?.Database?.Migrate();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();