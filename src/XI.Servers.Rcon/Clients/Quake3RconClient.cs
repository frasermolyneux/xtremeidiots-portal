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
                .WaitAndRetry(GetRetryTimeSpans(), (result, timeSpan, retryCount, context) => { _logger.LogWarning("[{serverName}] Failed to execute rcon command - retry count: {count}", ServerName, retryCount); })
                .Execute(() => ExecuteCommandInternal(rconCommand));

            return commandResult;
        }

        private string ExecuteCommandInternal(string rconCommand)
        {
            _logger.LogInformation("[{serverName}] Executing {command} command against server", ServerName, rconCommand);

            try
            {
                var client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                {
                    SendTimeout = 15000,
                    ReceiveTimeout = 15000
                };
                client.Connect(IPAddress.Parse(Hostname), QueryPort);

                var command = $"rcon {RconPassword} {rconCommand}";
                var bufferTemp = Encoding.ASCII.GetBytes(command);
                var bufferSend = new byte[bufferTemp.Length + 4];

                bufferSend[0] = 0xFF;
                bufferSend[1] = 0xFF;
                bufferSend[2] = 0xFF;
                bufferSend[3] = 0xFF;

                var j = 4;
                foreach (var commandBytes in bufferTemp)
                {
                    bufferSend[j] = commandBytes;
                    j++;
                }

                client.Send(bufferSend, SocketFlags.None);

                Thread.Sleep(1000);

                var response = new StringBuilder();

                do
                {
                    _logger.LogInformation($"A - Client available is {client.Available}");
                    var receiveBuffer = new byte[65536];
                    var bytesReceived = client.Receive(receiveBuffer);
                    var data = Encoding.ASCII.GetString(receiveBuffer, 0, bytesReceived);

                    if (data.IndexOf("print", StringComparison.Ordinal) == 4) data = data.Substring(10);

                    response.Append(data);

                    _logger.LogInformation($"B - Client available is {client.Available}");
                    Thread.Sleep(1000);
                } while (client.Available > 0);

                return response.ToString().Replace("\0", "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{serverName}] Failed to execute rcon command", ServerName);
                throw;
            }
        }
    }
}