using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace XtremeIdiots.Portal.Web;

public class TelemetryInitializer : ITelemetryInitializer
{

 public void Initialize(ITelemetry telemetry)
 {

 telemetry.Context.Cloud.RoleName = "Portal WebApp";
 }
}