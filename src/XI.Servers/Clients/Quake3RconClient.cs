using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using XI.Servers.Interfaces;
using XI.Servers.Interfaces.Models;
using XI.Servers.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Servers.Clients
{
    public class Quake3RconClient : IRconClient
    {
        private readonly ILogger _logger;

        private GameType _gameType;
        private string _hostname;
        private int _queryPort;
        private string _rconPassword;

        private Guid _serverId;

        public Quake3RconClient(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Configure(GameType gameType, Guid serverId, string hostname, int queryPort, string rconPassword)
        {
            _logger.LogDebug("[{ServerId}] Configuring Quake3 rcon client for {GameType} with endpoint {Hostname}:{QueryPort}", serverId, gameType, hostname, queryPort);

            _gameType = gameType;
            _serverId = serverId;
            _hostname = hostname;
            _queryPort = queryPort;
            _rconPassword = rconPassword;
        }

        public List<IRconPlayer> GetPlayers()
        {
            _logger.LogDebug("[{ServerId}] Attempting to get a list of players from the server", _serverId);

            var players = new List<IRconPlayer>();

            var playerStatus = PlayerStatus();
            var lines = playerStatus.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
            for (var i = 3; i < lines.Count; i++)
            {
                var line = lines[i];
                var match = GameTypeRegex(_gameType).Match(line);

                if (!match.Success)
                    continue;

                var num = match.Groups[1].ToString();
                var score = match.Groups[2].ToString();
                var ping = match.Groups[3].ToString();
                var guid = match.Groups[4].ToString();
                var name = match.Groups[5].ToString().Trim();
                var ipAddress = match.Groups[7].ToString();
                var qPort = match.Groups[9].ToString();
                var rate = match.Groups[10].ToString();

                _logger.LogDebug("[{ServerId}] Player {Name} with {Guid} and {IpAddress} parsed from result", _serverId, name, guid, ipAddress);

                players.Add(new Quake3RconPlayer
                {
                    Num = num,
                    Score = score,
                    Ping = ping,
                    Guid = guid,
                    Name = name,
                    IpAddress = ipAddress,
                    QPort = qPort,
                    Rate = rate
                });
            }

            return players;
        }

        public Task Say(string message)
        {
            _logger.LogDebug("[{ServerId}] Attempting to send '{message}' to the server", _serverId, message);

            Policy.Handle<Exception>()
                .WaitAndRetry(GetRetryTimeSpans(), (result, timeSpan, retryCount, context) => { _logger.LogWarning("[{serverName}] Failed to execute rcon command - retry count: {count}", _serverId, retryCount); })
                .Execute(() => GetCommandPackets($"say \"{message}\""));

            return Task.CompletedTask;
        }

        public Task<string> Restart()
        {
            _logger.LogDebug("[{ServerId}] Attempting to send restart the server", _serverId);

            var packets = Policy.Handle<Exception>()
                .WaitAndRetry(GetRetryTimeSpans(), (result, timeSpan, retryCount, context) => { _logger.LogWarning("[{serverName}] Failed to execute rcon command - retry count: {count}", _serverId, retryCount); })
                .Execute(() => GetCommandPackets("quit", true));

            return Task.FromResult("Restart command sent to the server");
        }

        public Task<string> RestartMap()
        {
            _logger.LogDebug("[{ServerId}] Attempting to restart the current map", _serverId);

            var packets = Policy.Handle<Exception>()
                .WaitAndRetry(GetRetryTimeSpans(), (result, timeSpan, retryCount, context) => { _logger.LogWarning("[{serverName}] Failed to execute rcon command - retry count: {count}", _serverId, retryCount); })
                .Execute(() => GetCommandPackets("map_restart"));

            return Task.FromResult(GetStringFromPackets(packets));
        }

        public Task<string> FastRestartMap()
        {
            _logger.LogDebug("[{ServerId}] Attempting to fast restart the current map", _serverId);

            var packets = Policy.Handle<Exception>()
                .WaitAndRetry(GetRetryTimeSpans(), (result, timeSpan, retryCount, context) => { _logger.LogWarning("[{serverName}] Failed to execute rcon command - retry count: {count}", _serverId, retryCount); })
                .Execute(() => GetCommandPackets("fast_restart"));

            return Task.FromResult(GetStringFromPackets(packets));
        }

        public Task<string> NextMap()
        {
            _logger.LogDebug("[{ServerId}] Attempting to rotate to the next map", _serverId);

            var packets = Policy.Handle<Exception>()
                .WaitAndRetry(GetRetryTimeSpans(), (result, timeSpan, retryCount, context) => { _logger.LogWarning("[{serverName}] Failed to execute rcon command - retry count: {count}", _serverId, retryCount); })
                .Execute(() => GetCommandPackets("map_rotate"));

            return Task.FromResult(GetStringFromPackets(packets));
        }

        private string PlayerStatus()
        {
            var packets = Policy.Handle<Exception>()
                .WaitAndRetry(GetRetryTimeSpans(), (result, timeSpan, retryCount, context) => { _logger.LogWarning("[{serverName}] Failed to execute rcon command - retry count: {count}", _serverId, retryCount); })
                .Execute(() => GetCommandPackets("status"));

            _logger.LogDebug("[{ServerId}] Total status packets retrieved from server: {Count}", _serverId, packets.Count);

            return GetStringFromPackets(packets);
        }

        private string GetStringFromPackets(List<byte[]> packets)
        {
            var responseText = new StringBuilder();

            foreach (var packet in packets)
            {
                var text = Encoding.Default.GetString(packet);
                if (text.IndexOf("print", StringComparison.Ordinal) == 4) text = text.Substring(10);

                responseText.Append(text);
            }

            return responseText.ToString();
        }

        private static Regex GameTypeRegex(GameType gameType)
        {
            switch (gameType)
            {
                case GameType.CallOfDuty2:
                    return new Regex(
                        "^\\s*([0-9]+)\\s+([0-9-]+)\\s+([0-9]+)\\s+([0-9]+)\\s+(.*?)\\s+([0-9]+?)\\s*((?:(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\\.){3}(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])):?(-?[0-9]{1,5})\\s*(-?[0-9]{1,5})\\s+([0-9]+)$");
                case GameType.CallOfDuty4:
                    return new Regex(
                        "^\\s*([0-9]+)\\s+([0-9-]+)\\s+([0-9]+)\\s+([0-9a-f]{32})\\s+(.*?)\\s+([0-9]+?)\\s*((?:(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\\.){3}(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])):?(-?[0-9]{1,5})\\s*(-?[0-9]{1,5})\\s+([0-9]+)$");
                case GameType.CallOfDuty5:
                    return new Regex(
                        "^\\s*([0-9]+)\\s+([0-9-]+)\\s+([0-9]+)\\s+([0-9]+)\\s+(.*?)\\s+([0-9]+?)\\s*((?:(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\\.){3}(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])):?(-?[0-9]{1,5})\\s*(-?[0-9]{1,5})\\s+([0-9]+)$");
                default:
                    throw new Exception("Unsupported game type");
            }
        }

        private static byte[] ExecuteCommandPacket(string rconPassword, string command)
        {
            //ÿÿÿÿrcon {rconPassword} {command}
            var prefix = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
            var commandText = $"rcon {rconPassword} {command}";
            var commandBytes = Encoding.Default.GetBytes(commandText);

            return prefix.Concat(commandBytes).ToArray();
        }

        private List<byte[]> GetCommandPackets(string command, bool skipReceive = false)
        {
            UdpClient udpClient = null;

            try
            {
                var commandBytes = ExecuteCommandPacket(_rconPassword, command);
                var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                udpClient = new UdpClient(_queryPort) { Client = { SendTimeout = 5000, ReceiveTimeout = 5000 } };
                udpClient.Connect(_hostname, _queryPort);
                udpClient.Send(commandBytes, commandBytes.Length);

                var datagrams = new List<byte[]>();
                if (!skipReceive)
                {
                    do
                    {
                        var datagramBytes = udpClient.Receive(ref remoteIpEndPoint);
                        datagrams.Add(datagramBytes);

                        if (udpClient.Available == 0)
                            Thread.Sleep(500);
                    } while (udpClient.Available > 0);
                }

                return datagrams;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{serverName}] Failed to execute rcon command", _serverId);
                throw;
            }
            finally
            {
                udpClient?.Dispose();
            }
        }

        private static IEnumerable<TimeSpan> GetRetryTimeSpans()
        {
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