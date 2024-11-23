using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System;

var builder = WebApplication.CreateBuilder(args);

// Configura��o do MongoDB
builder.Services.AddSingleton<IMongoClient, MongoClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetSection("MongoDB:ConnectionString").Value;
    return new MongoClient(connectionString);
});

// Configura��o do Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString");
    options.Configuration = redisConnectionString;
    options.InstanceName = "EnergyMonitorCache:"; // Prefixo para as chaves do Redis
});

// Configura��o do Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GS Microservice API",
        Version = "v1",
        Description = "API para monitoramento de consumo de energia el�trica com MongoDB e Redis",
        Contact = new OpenApiContact
        {
            Name = "Daniel Franceschi",
            Email = "daniel@example.com",
            Url = new Uri("https://example.com")
        }
    });
});

// Configura��o de servi�os gerais
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configura��o do Swagger para ser acessado em desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GS Microservice API v1");
        c.RoutePrefix = string.Empty; // Swagger ser� acessado na raiz (/)
    });
}

// Middleware
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
