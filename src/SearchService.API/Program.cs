using MongoDB.Driver;
using SearchService.Application.Handlers;
using SearchService.Domain.Interfaces;
using SearchService.Infrastructure.Configurations;
using SearchService.Infrastructure.Persistence;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration["MONGO_CONNECTION_STRING"];
    return new MongoClient(connectionString);
});

builder.Services.AddScoped(sp =>
{
    var databaseName = builder.Configuration["MONGO_DATABASE_NAME"];
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(databaseName);
});

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(SearchMenuItemsHandler).Assembly));

builder.Services.AddScoped<ISearchRepository, MongoSearchRepository>();

// Configurar métricas do Prometheus
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

// Configurar métricas do Prometheus
app.UseMetricServer();
app.UseHttpMetrics();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();