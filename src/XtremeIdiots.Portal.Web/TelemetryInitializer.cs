using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace XtremeIdiots.Portal.Web;

/// <summary>
/// Initializes telemetry data with custom properties for the XtremeIdiots Portal web application
/// </summary>
public class TelemetryInitializer : ITelemetryInitializer
{
    /// <summary>
    /// Initializes telemetry data with application-specific context information
    /// </summary>
    /// <param name="telemetry">The telemetry item to initialize</param>
    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.Cloud.RoleName = "Portal WebApp";
    }
}