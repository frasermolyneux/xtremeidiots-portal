using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using XI.Servers.Rcon.Models;

namespace XI.Servers.Rcon.Clients
{
    public class BaseRconClient : IRconClient
    {
        private readonly ILogger _logger;

        public BaseRconClient(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Guid ServerId { get; set; }
        public string Hostname { get; private set; }
        public int QueryPort { get; private set; }
        public string RconPassword { get; private set; }
        public List<TimeSpan> RetryOverride { get; set; }
        public virtual Regex PlayerRegex { get; set; }

        public void Configure(Guid serverId, string hostname, int queryPort, string rconPassword, List<TimeSpan> retryOverride)
        {
            ServerId = serverId;
            Hostname = hostname;
            QueryPort = queryPort;
            RconPassword = rconPassword;
            RetryOverride = retryOverride;
        }

        public virtual List<IRconPlayer> GetPlayers()
        {
            _logger.LogWarning("[{serverId}] GetPlayers not currently implemented", ServerId);
            return new List<IRconPlayer>();
        }

        public virtual string PlayerStatus()
        {
            _logger.LogWarning("[{serverId}] PlayerStatus not currently implemented", ServerId);
            return string.Empty;
        }

        public virtual string KickPlayer(string targetPlayerNum)
        {
            _logger.LogWarning("[{serverId}] KickPlayer not currently implemented", ServerId);
            return string.Empty;
        }

        public virtual string BanPlayer(string targetPlayerNum)
        {
            _logger.LogWarning("[{serverId}] BanPlayer not currently implemented", ServerId);
            return string.Empty;
        }

        public virtual string RestartServer()
        {
            _logger.LogWarning("[{serverId}] RestartServer not currently implemented", ServerId);
            return string.Empty;
        }

        public virtual string RestartMap()
        {
            _logger.LogWarning("[{serverId}] RestartMap not currently implemented", ServerId);
            return string.Empty;
        }

        public virtual string NextMap()
        {
            _logger.LogWarning("[{serverId}] NextMap not currently implemented", ServerId);
            return string.Empty;
        }

        public virtual string MapRotation()
        {
            _logger.LogWarning("[{serverId}] MapRotation not currently implemented", ServerId);
            return string.Empty;
        }

        public virtual string Say(string message)
        {
            _logger.LogWarning("[{serverId}] Say not currently implemented", ServerId);
            return string.Empty;
        }

        public IEnumerable<TimeSpan> GetRetryTimeSpans()
        {
            if (RetryOverride != null) return RetryOverride;

            var random = new Random();

            return new[]
            {
                TimeSpan.FromSeconds(random.Next(1)),
                TimeSpan.FromSeconds(random.Next(3)),
                TimeSpan.FromSeconds(random.Next(5))
            };
        }
    }
}