using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryWebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
builder.Services.AddLogging();
builder.Services.AddMemoryCache();
builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddDbContext<PortalDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration["sql-connection-string"], sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
        sqlOptions.CommandTimeout(180);
    });
});

// Add services to the container.
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

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