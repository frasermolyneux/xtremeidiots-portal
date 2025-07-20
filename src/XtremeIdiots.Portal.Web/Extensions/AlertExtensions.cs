using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

using XtremeIdiots.Portal.Web.Models;

namespace XtremeIdiots.Portal.Web.Extensions;

/// <summary>
/// Extension methods for adding alert messages to controllers using TempData
/// </summary>
public static class AlertExtensions
{
    private const string AlertKey = "Alerts";

    /// <summary>
    /// Adds a success alert message to be displayed on the next page view
    /// </summary>
    /// <param name="controller">The controller instance</param>
    /// <param name="message">The success message to display</param>
    public static void AddAlertSuccess(this Controller controller, string message)
    {
        var alerts = GetAlerts(controller);
        alerts.Add(new Alert(message, "alert-success"));
        controller.TempData[AlertKey] = JsonConvert.SerializeObject(alerts);
    }

    /// <summary>
    /// Adds an informational alert message to be displayed on the next page view
    /// </summary>
    /// <param name="controller">The controller instance</param>
    /// <param name="message">The informational message to display</param>
    public static void AddAlertInfo(this Controller controller, string message)
    {
        var alerts = GetAlerts(controller);
        alerts.Add(new Alert(message, "alert-info"));
        controller.TempData[AlertKey] = JsonConvert.SerializeObject(alerts);
    }

    /// <summary>
    /// Adds a warning alert message to be displayed on the next page view
    /// </summary>
    /// <param name="controller">The controller instance</param>
    /// <param name="message">The warning message to display</param>
    public static void AddAlertWarning(this Controller controller, string message)
    {
        var alerts = GetAlerts(controller);
        alerts.Add(new Alert(message, "alert-warning"));
        controller.TempData[AlertKey] = JsonConvert.SerializeObject(alerts);
    }

    /// <summary>
    /// Adds a danger/error alert message to be displayed on the next page view
    /// </summary>
    /// <param name="controller">The controller instance</param>
    /// <param name="message">The error message to display</param>
    public static void AddAlertDanger(this Controller controller, string message)
    {
        var alerts = GetAlerts(controller);
        alerts.Add(new Alert(message, "alert-danger"));
        controller.TempData[AlertKey] = JsonConvert.SerializeObject(alerts);
    }

    private static ICollection<Alert> GetAlerts(Controller controller)
    {
        var alertsTemp = controller.TempData[AlertKey] ?? JsonConvert.SerializeObject(new HashSet<Alert>());
        var alerts = JsonConvert.DeserializeObject<ICollection<Alert>>(alertsTemp.ToString() ?? string.Empty) ?? [];
        return alerts;
    }
}