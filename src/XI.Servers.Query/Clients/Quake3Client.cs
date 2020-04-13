using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using XI.Servers.Query.Models;

namespace XI.Servers.Query.Clients
{
    public class Quake3Client : IQueryClient
    {
        private const string PlayerRegex = "(?<score>.+) (?<ping>.+) \\\"(?<name>.+)\\\"";

        private readonly ILogger _logger;

        public Quake3Client(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string Hostname { get; set; }
        private int QueryPort { get; set; }

        public void Configure(string hostname, int queryPort)
        {
            if (string.IsNullOrWhiteSpace(hostname))
                throw new ArgumentNullException(nameof(hostname));

            if (queryPort == 0)
                throw new ArgumentNullException(nameof(queryPort));

            Hostname = hostname;
            QueryPort = queryPort;
        }

        public Task<IQueryResponse> GetServerStatus()
        {
            var queryResult = Query("getstatus");

            var lines = queryResult.Substring(3).Split('\n');
            if (lines.Length < 2) return null;

            var serverParams = GetParams(lines[1].Split('\\'));

            if (lines.Length <= 2)
                return Task.FromResult((IQueryResponse) new Quake3QueryResponse(serverParams, new List<IQueryPlayer>()));

            var players = new List<IQueryPlayer>();
            for (var i = 2; i < lines.Length; i++)
            {
                if (lines[i].Length == 0) continue;
                players.Add(ParsePlayer(lines[i]));
            }

            return Task.FromResult((IQueryResponse) new Quake3QueryResponse(serverParams, players));
        }

        private static IQueryPlayer ParsePlayer(string playerInfo)
        {
            var regPattern = new Regex(PlayerRegex);
            var regMatch = regPattern.Match(playerInfo);

            var player = new Quake3QueryPlayer
            {
                Name = regMatch.Groups["name"].Value,
                Score = int.Parse(regMatch.Groups["score"].Value),
                Ping = int.Parse(regMatch.Groups["ping"].Value)
            };

            return player;
        }

        private static Dictionary<string, string> GetParams(IReadOnlyList<string> parts)
        {
            var serverParams = new Dictionary<string, string>();

            for (var i = 0; i < parts.Count; i++)
            {
                if (parts[i].Length == 0) continue;
                var key = parts[i++];
                var val = parts[i];

                if (key == "final") break;
                if (key == "querid") continue;

                serverParams[key] = val;
            }

            return serverParams;
        }

        private string Query(string command)
        {
            _logger.LogInformation("Executing {command} command against server", command);

            try
            {
                var client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                {
                    SendTimeout = 5000,
                    ReceiveTimeout = 5000
                };
                client.Connect(IPAddress.Parse(Hostname), QueryPort);

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

                    response.Append(data);
                } while (client.Available > 0);

                return response.ToString().Replace("\0", "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute command");
                throw;
            }
        }
    }
}