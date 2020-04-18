using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Polly;
using XI.Servers.Rcon.Models;

namespace XI.Servers.Rcon.Clients
{
    public class Quake3RconClient : BaseRconClient
    {
        private readonly ILogger _logger;

        public Quake3RconClient(ILogger logger) : base(logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private byte[] ExecuteCommandPacket(string rconPassword, string command)
        {
            //ÿÿÿÿrcon {rconPassword} {command}
            var prefix = new byte[] {0xFF, 0xFF, 0xFF, 0xFF};
            var commandText = $"rcon {rconPassword} {command}";
            var commandBytes = Encoding.Default.GetBytes(commandText);

            return prefix.Concat(commandBytes).ToArray();
        }

        public override List<IRconPlayer> GetPlayers()
        {
            var players = new List<IRconPlayer>();

            var playerStatus = PlayerStatus();
            var lines = playerStatus.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
            for (var i = 3; i < lines.Count; i++)
            {
                var line = lines[i];
                var match = PlayerRegex.Match(line);

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

        public override string PlayerStatus()
        {
            return ExecuteCommand("status");
        }

        public override string KickPlayer(string targetPlayerNum)
        {
            return ExecuteCommand($"clientkick {targetPlayerNum}");
        }

        public override string BanPlayer(string targetPlayerNum)
        {
            return ExecuteCommand($"banclient {targetPlayerNum}");
        }

        public override string RestartServer()
        {
            return ExecuteCommand("quit");
        }

        public override string RestartMap()
        {
            return ExecuteCommand("map_restart");
        }

        public override string NextMap()
        {
            return ExecuteCommand("map_rotate");
        }

        public override string MapRotation()
        {
            return ExecuteCommand("sv_maprotation");
        }

        public override string Say(string message)
        {
            return ExecuteCommand($"say \"{message}\"");
        }

        private string ExecuteCommand(string rconCommand)
        {
            var commandResult = Policy.Handle<Exception>()
                .WaitAndRetry(GetRetryTimeSpans(), (result, timeSpan, retryCount, context) => { _logger.LogWarning("[{serverName}] Failed to execute rcon command - retry count: {count}", ServerId, retryCount); })
                .Execute(() => ExecuteCommandInternal(rconCommand));

            return commandResult;
        }

        private string ExecuteCommandInternal(string rconCommand)
        {
            _logger.LogInformation("[{serverName}] Executing {command} command against server", ServerId, rconCommand);

            UdpClient udpClient = null;

            try
            {
                var commandBytes = ExecuteCommandPacket(RconPassword, rconCommand);
                var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                udpClient = new UdpClient(QueryPort) {Client = {SendTimeout = 5000, ReceiveTimeout = 5000}};
                udpClient.Connect(Hostname, QueryPort);
                udpClient.Send(commandBytes, commandBytes.Length);

                var datagrams = new List<string>();
                do
                {
                    var datagramBytes = udpClient.Receive(ref remoteIpEndPoint);
                    var datagramText = Encoding.Default.GetString(datagramBytes);

                    datagrams.Add(datagramText);

                    if (udpClient.Available == 0)
                        Thread.Sleep(500);
                } while (udpClient.Available > 0);

                var responseText = new StringBuilder();

                foreach (var datagram in datagrams)
                {
                    var text = datagram;
                    if (text.IndexOf("print", StringComparison.Ordinal) == 4) text = text.Substring(10);

                    responseText.Append(text);
                }

                return responseText.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{serverName}] Failed to execute rcon command", ServerId);
                throw;
            }
            finally
            {
                udpClient?.Dispose();
            }
        }
    }
}