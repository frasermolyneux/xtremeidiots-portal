using Microsoft.Extensions.Logging;

namespace XI.Rcon.Clients
{
    public class Cod4RconClient : CodBaseRconClient
    {
        public Cod4RconClient(ILogger logger) : base(logger)
        {
        }
    }
}