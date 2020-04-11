using Microsoft.Extensions.Logging;

namespace XI.CommonTypes
{
    public static class EventIds
    {
        public static EventId Management { get; set; } = new EventId(100, nameof(Management));
        public static EventId User { get; set; } = new EventId(200, nameof(User));
    }
}