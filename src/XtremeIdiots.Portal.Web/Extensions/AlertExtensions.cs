using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

using XtremeIdiots.Portal.Web.Models;

namespace XtremeIdiots.Portal.Web.Extensions;

public static class AlertExtensions
{
    private const string AlertKey = "Alerts";

    public static void AddAlertSuccess(this Controller controller, string message)
    {
        var alerts = GetAlerts(controller);

        alerts.Add(new Alert(message, "alert-success"));

        controller.TempData[AlertKey] = JsonConvert.SerializeObject(alerts);
    }

    public static void AddAlertInfo(this Controller controller, string message)
    {
        var alerts = GetAlerts(controller);

        alerts.Add(new Alert(message, "alert-info"));

        controller.TempData[AlertKey] = JsonConvert.SerializeObject(alerts);
    }

    public static void AddAlertWarning(this Controller controller, string message)
    {
        var alerts = GetAlerts(controller);

        alerts.Add(new Alert(message, "alert-warning"));

        controller.TempData[AlertKey] = JsonConvert.SerializeObject(alerts);
    }

    public static void AddAlertDanger(this Controller controller, string message)
    {
        var alerts = GetAlerts(controller);

        alerts.Add(new Alert(message, "alert-danger"));

        controller.TempData[AlertKey] = JsonConvert.SerializeObject(alerts);
    }

    private static ICollection<Alert> GetAlerts(Controller controller)
    {
        var alertsTemp = controller.TempData[AlertKey] ?? JsonConvert.SerializeObject(new HashSet<Alert>());

        var alerts = JsonConvert.DeserializeObject<ICollection<Alert>>(alertsTemp.ToString() ?? string.Empty) ?? new HashSet<Alert>();

        return alerts;
    }
}