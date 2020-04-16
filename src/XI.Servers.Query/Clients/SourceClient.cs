using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using XI.Servers.Query.Models;

namespace XI.Servers.Query.Clients
{
    public class SourceClient : IQueryClient
    {
        private readonly ILogger _logger;

        public SourceClient(ILogger logger)
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
            var (infoQuery, infoQueryBytes) = Query(A2S_INFO());
            var serverParams = GetParams(infoQueryBytes);

            var (playersPreQuery, playersPreQueryBytes) = Query(A2S_PLAYERS_PRE());
            var challengeResponse = playersPreQueryBytes.Skip(5).ToArray();
            var (playersQuery, playersQueryBytes) = Query(A2S_PLAYERS(challengeResponse));

            var players = ParsePlayers(playersQueryBytes);

            return Task.FromResult((IQueryResponse) new SourceQueryResponse(serverParams, players));
        }

        private byte[] A2S_INFO()
        {
            //ÿÿÿÿTSource Engine Query
            return new byte[] {0xFF, 0xFF, 0xFF, 0xFF, 0x54, 0x53, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x20, 0x45, 0x6E, 0x67, 0x69, 0x6E, 0x65, 0x20, 0x51, 0x75, 0x65, 0x72, 0x79, 0x00};
        }

        private byte[] A2S_PLAYERS_PRE()
        {
            return new byte[] {0xFF, 0xFF, 0xFF, 0xFF, 0x55, 0xFF, 0xFF, 0xFF, 0xFF};
        }

        private byte[] A2S_PLAYERS(byte[] challengeResponse)
        {
            var start = new byte[] {0xFF, 0xFF, 0xFF, 0xFF, 0x55};
            return start.Concat(challengeResponse).ToArray();
        }

        private static List<IQueryPlayer> ParsePlayers(byte[] responseBytes)
        {
            var players = new List<IQueryPlayer>();
            var numPlayers = responseBytes[5];
            var i = 6;

            for (var ii = 0; ii < numPlayers; ii++)
            {
                var newPlayer = new SourceQueryPlayer();

                // ReSharper disable once UnusedVariable
                var index = responseBytes[i++];

                var playerNameArr = new List<byte>();

                while (responseBytes[i] != 0x00) playerNameArr.Add(responseBytes[i++]);

                i++;

                newPlayer.Name = Encoding.UTF8.GetString(playerNameArr.ToArray());
                newPlayer.Score = BitConverter.ToInt32(responseBytes, i);

                i += 4;

                newPlayer.Time = new TimeSpan(0, 0, (int) BitConverter.ToSingle(responseBytes, i));

                i += 4;

                players.Add(newPlayer);
            }

            return players;
        }

        private static Dictionary<string, string> GetParams(byte[] responseBytes)
        {
            var serverParams = new Dictionary<string, string>();

            serverParams["protocolver"] = responseBytes[5].ToString();
            var offset = 6;
            serverParams["hostname"] = ReadNextParam(responseBytes, offset, out offset);
            serverParams["mapname"] = ReadNextParam(responseBytes, offset, out offset);
            serverParams["mod"] = ReadNextParam(responseBytes, offset, out offset);
            serverParams["modname"] = ReadNextParam(responseBytes, offset, out offset);

            var appid = new byte[2];
            Array.Copy(responseBytes, offset++, appid, 0, 2);
            serverParams["appid"] = BitConverter.ToInt16(appid, 0).ToString();
            offset++;

            serverParams["numplayers"] = responseBytes[offset++].ToString();
            serverParams["maxplayers"] = responseBytes[offset++].ToString();
            serverParams["botcount"] = responseBytes[offset++].ToString();
            serverParams["servertype"] = responseBytes[offset++].ToString();
            serverParams["serveros"] = responseBytes[offset++].ToString();
            serverParams["passworded"] = responseBytes[offset++].ToString();
            serverParams["secureserver"] = responseBytes[offset++].ToString();
            serverParams["version"] = ReadNextParam(responseBytes, offset, out offset);

            return serverParams;
        }

        private static string ReadNextParam(byte[] responseBytes, int offset, out int newOffset)
        {
            var temp = "";
            for (; offset < responseBytes.Length; offset++)
            {
                if (responseBytes[offset] == 0)
                {
                    offset++;
                    break;
                }

                temp += (char) responseBytes[offset];
            }

            newOffset = offset;
            return temp;
        }


        private Tuple<string, byte[]> Query(byte[] commandBytes)
        {
            _logger.LogInformation("Executing command against server");

            UdpClient udpClient = null;

            try
            {
                var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                udpClient = new UdpClient(QueryPort) {Client = {SendTimeout = 5000, ReceiveTimeout = 5000}};
                udpClient.Connect(Hostname, QueryPort);
                udpClient.Send(commandBytes, commandBytes.Length);

                var datagrams = new List<byte[]>();
                do
                {
                    var datagramBytes = udpClient.Receive(ref remoteIpEndPoint);
                    datagrams.Add(datagramBytes);

                    if (udpClient.Available == 0)
                        Thread.Sleep(1000);
                } while (udpClient.Available > 0);

                var responseText = new StringBuilder();
                byte[] responseBytes = null;

                foreach (var datagram in datagrams)
                {
                    var datagramBytes = datagram;
                    var datagramText = Encoding.Default.GetString(datagram);

                    responseBytes = responseBytes == null ? datagramBytes : responseBytes.Concat(datagramBytes).ToArray();
                    responseText.Append(datagramText);
                }

                return new Tuple<string, byte[]>(responseText.ToString(), responseBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute command");
                throw;
            }
            finally
            {
                udpClient?.Dispose();
            }
        }
    }
}