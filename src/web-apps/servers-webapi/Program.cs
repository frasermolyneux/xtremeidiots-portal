using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Identity.Web;

using XtremeIdiots.Portal.RepositoryApiClient;
using XtremeIdiots.Portal.ServersWebApi;
using XtremeIdiots.Portal.ServersWebApi.Factories;
using XtremeIdiots.Portal.ServersWebApi.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
builder.Services.AddLogging();
builder.Services.AddMemoryCache();
builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddSingleton<IQueryClientFactory, QueryClientFactory>();
builder.Services.AddSingleton<IRconClientFactory, RconClientFactory>();

builder.Services.AddRepositoryApiClient(options =>
{
    options.ApimBaseUrl = builder.Configuration["repository-api-base-url"] ?? builder.Configuration["apim-base-url"];
    options.ApimSubscriptionKey = builder.Configuration["apim-subscription-key"];
    options.ApiPathPrefix = builder.Configuration["repository-api-path-prefix"] ?? "repository";
});

// Add services to the container.
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
