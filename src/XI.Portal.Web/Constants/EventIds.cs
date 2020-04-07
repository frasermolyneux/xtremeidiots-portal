using Microsoft.Extensions.Logging;

namespace XI.Portal.Web.Constants
{
    public static class EventIds
    {
        public static EventId Management { get; set; } = new EventId(100, nameof(Management));
    }
}