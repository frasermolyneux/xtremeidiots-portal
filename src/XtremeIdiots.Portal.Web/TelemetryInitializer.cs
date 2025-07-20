using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace XtremeIdiots.Portal.Web;

/// <summary>
/// Custom telemetry initializer for Application Insights that enriches telemetry data
/// with application-specific context information for better monitoring and diagnostics.
/// </summary>
public class TelemetryInitializer : ITelemetryInitializer
{
 /// <summary>
 /// Initializes telemetry data by setting the cloud role name for Application Insights.
 /// This helps identify telemetry data in multi-service environments and enables
 /// proper application map visualization in Azure Application Insights.
 /// </summary>
 /// <param name="telemetry">The telemetry item to initialize with contextual information</param>
 public void Initialize(ITelemetry telemetry)
 {
 // Set the cloud role name to identify this application in Application Insights
 // This enables proper service identification in application maps and dependency tracking
 telemetry.Context.Cloud.RoleName = "Portal WebApp";
 }
}
