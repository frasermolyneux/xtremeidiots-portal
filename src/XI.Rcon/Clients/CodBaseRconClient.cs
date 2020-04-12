using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Polly;

namespace XI.Rcon.Clients
{
    public class CodBaseRconClient : BaseRconClient
    {
        private readonly ILogger _logger;

        public CodBaseRconClient(ILogger logger) : base(logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                    SendTimeout = 5000,
                    ReceiveTimeout = 5000
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

                var response = new StringBuilder();

                do
                {
                    var recieveBuffer = new byte[65536];
                    var bytesReceived = client.Receive(recieveBuffer);
                    var data = Encoding.ASCII.GetString(recieveBuffer, 0, bytesReceived);

                    if (data.IndexOf("print", StringComparison.Ordinal) == 4) data = data.Substring(10);

                    response.Append(data);
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