using MongoDB.Driver;
using SearchService.Application.Handlers;
using SearchService.Domain.Interfaces;
using SearchService.Infrastructure.Configurations;
using SearchService.Infrastructure.Persistence;
using Prometheus;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

var jwtKey = builder.Configuration["JWT_KEY"];
var jwtIssuer = builder.Configuration["JWT_ISSUER"];
var jwtAudience = builder.Configuration["JWT_AUDIENCE"];
var keyBytes = Encoding.UTF8.GetBytes(jwtKey!);

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Informe o token JWT no campo abaixo: Bearer {token}",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseMetricServer();
app.UseHttpMetrics();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();