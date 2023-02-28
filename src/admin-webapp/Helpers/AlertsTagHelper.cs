using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json;
using XtremeIdiots.Portal.AdminWebApp.Models;

namespace XtremeIdiots.Portal.AdminWebApp.Helpers
{
    public class AlertsTagHelper : TagHelper
    {
        private const string AlertKey = "Alerts";

        [ViewContext] public ViewContext ViewContext { get; set; }

        protected ITempDataDictionary TempData => ViewContext.TempData;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";

            if (TempData[AlertKey] == null)
                TempData[AlertKey] = JsonConvert.SerializeObject(new HashSet<Alert>());

            var alerts = JsonConvert.DeserializeObject<ICollection<Alert>>(TempData[AlertKey].ToString());

            var html = string.Empty;

            foreach (var alert in alerts)
                html += $"<div class='alert {alert.Type}' id='inner-alert' role='alert' style='padding-top:10px'>" +
                        "<button type='button' class='close' data-dismiss='alert' aria-label='Close'>" +
                        "<span aria-hidden='true'>&times;</span>" +
                        "</button>" +
                        $"{alert.Message}" +
                        "</div>";

            output.Content.SetHtmlContent(html);
        }
    }
}