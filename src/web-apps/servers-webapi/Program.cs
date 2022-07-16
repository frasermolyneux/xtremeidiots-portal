using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authorization;
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
    options.BaseUrl = builder.Configuration["repository_api_base_url"] ?? builder.Configuration["apim_base_url"];
    options.ApiKey = builder.Configuration["apim_subscription_key"];
    options.ApiPathPrefix = builder.Configuration["repository_api_path_prefix"] ?? "repository";
});

// Add services to the container.
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
