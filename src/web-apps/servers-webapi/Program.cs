using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

using XtremeIdiots.Portal.RepositoryApiClient;
using XtremeIdiots.Portal.ServersWebApi;
using XtremeIdiots.Portal.ServersWebApi.Factories;
using XtremeIdiots.Portal.ServersWebApi.Interfaces;
using XtremeIdiots.Portal.ServersWebApi.OpenApiOperationFilters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
builder.Services.AddLogging();
builder.Services.AddMemoryCache();

builder.Services.Configure<TelemetryConfiguration>(telemetryConfiguration =>
{
    var builder = telemetryConfiguration.DefaultTelemetrySink.TelemetryProcessorChainBuilder;

    // Using fixed rate sampling
    double fixedSamplingPercentage = 50;
    builder.UseSampling(fixedSamplingPercentage);
});

builder.Services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
{
    EnableAdaptiveSampling = false,
});

builder.Services.AddSingleton<IQueryClientFactory, QueryClientFactory>();
builder.Services.AddSingleton<IRconClientFactory, RconClientFactory>();

builder.Services.AddRepositoryApiClient(options =>
{
    options.BaseUrl = builder.Configuration["apim_base_url"] ?? builder.Configuration["repository_base_url"];
    options.ApiKey = builder.Configuration["portal_repository_apim_subscription_key"];
    options.ApiPathPrefix = builder.Configuration["repository_api_path_prefix"] ?? "repository";
});

// Add services to the container.
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Servers API", Version = "v1" });

    options.SchemaFilter<EnumSchemaFilter>();

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

app.MapGet("/", [AllowAnonymous] () => "OK");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
