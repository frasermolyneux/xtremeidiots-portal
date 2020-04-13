using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private bool _queryInProgress = false;

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
            Stopwatch watch = new Stopwatch();
            watch.Start();
            var (infoQuery, infoQueryBytes) = Query(A2S_INFO());
            var a = watch.Elapsed;
            var serverParams = GetParams(infoQueryBytes);
            var b = watch.Elapsed;

            var (playersPreQuery, playersPreQueryBytes) = Query(A2S_PLAYERS_PRE());
            var c = watch.Elapsed;
            var challengeResponse = playersPreQueryBytes.Skip(5).ToArray();
            var d = watch.Elapsed;
            var (playersQuery, playersQueryBytes) = Query(A2S_PLAYERS(challengeResponse));
            var e = watch.Elapsed;

            var players = ParsePlayers(playersQueryBytes);

            return Task.FromResult((IQueryResponse) new SourceQueryResponse(serverParams, players));
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

        private byte[] A2S_INFO()
        {
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

        private byte[] A2S_RULES_PRE()
        {
            return new byte[] {0xFF, 0xFF, 0xFF, 0xFF, 0x56, 0xFF, 0xFF, 0xFF, 0xFF};
        }

        private byte[] A2S_RULES(byte[] challengeResponse)
        {
            var start = new byte[] {0xFF, 0xFF, 0xFF, 0xFF, 0x56};
            return start.Concat(challengeResponse).ToArray();
        }

        private Tuple<string, byte[]> Query(byte[] command)
        {
            if (_queryInProgress)
                throw new Exception("There is already a query in progress");

            _queryInProgress = true;

            var commandHex = BitConverter.ToString(command);
            var commandString = Encoding.UTF8.GetString(command);

            _logger.LogInformation("Executing {CommandString} ({CommandHex}) command against server", commandString, commandHex);

            try
            {
                var client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                {
                    //SendTimeout = 5000,
                    //ReceiveTimeout = 5000
                };

                client.Connect(IPAddress.Parse(Hostname), QueryPort);
                client.Send(command, SocketFlags.None);

                int read;
                var bufferOffset = 0;
                var packages = 0;

                var response = new byte[100 * 1024];
                var remoteEndPoint = client.RemoteEndPoint;

                do
                {
                    read = 0;
                    try
                    {
                        if (packages > 0)
                        {
                            var tempBuffer = new byte[100 * 1024];
                            read = client.ReceiveFrom(tempBuffer, ref remoteEndPoint);

                            var packets = tempBuffer[8] & 15;
                            var packetNr = (tempBuffer[8] >> 4) + 1;

                            if (packetNr < packets)
                            {
                                Array.Copy(response, 9, tempBuffer, read, bufferOffset);
                                response = tempBuffer;
                            }
                            else
                            {
                                Array.Copy(tempBuffer, 9, response, bufferOffset, read);
                            }
                        }
                        else
                        {
                            read = client.ReceiveFrom(response, ref remoteEndPoint);
                        }

                        bufferOffset += read;
                        packages++;
                    }
                    catch (SocketException ex)
                    {
                        _logger.LogError(ex, "Socket error {EX} executing {Command} against server", ex.Message, commandHex);
                        break;
                    }
                } while (read > 0);

                if (bufferOffset > 0 && bufferOffset < response.Length)
                {
                    var temp = new byte[bufferOffset];
                    for (var i = 0; i < temp.Length; i++) temp[i] = response[i];
                    response = temp;
                }
                else
                {
                    _logger.LogError("Server response is either zero-length or exceeds buffer length");
                }

                return new Tuple<string, byte[]>(Encoding.Default.GetString(response), response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute command - {EX}", ex.Message);
                throw;
            }
            finally
            {
                _queryInProgress = false;
            }
        }
    }
}