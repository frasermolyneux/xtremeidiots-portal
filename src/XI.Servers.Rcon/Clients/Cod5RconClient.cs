using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace XI.Servers.Rcon.Clients
{
    public class Cod5RconClient : Quake3RconClient
    {
        public Cod5RconClient(ILogger logger) : base(logger)
        {
        }

        public override Regex PlayerRegex { get; set; } = new Regex(
            "^\\s*([0-9]+)\\s+([0-9-]+)\\s+([0-9]+)\\s+([0-9]+)\\s+(.*?)\\s+([0-9]+?)\\s*((?:(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\\.){3}(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])):?(-?[0-9]{1,5})\\s*(-?[0-9]{1,5})\\s+([0-9]+)$");
    }
}