using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XI.Servers.Interfaces;
using XI.Servers.Interfaces.Models;
using XI.Servers.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Servers.Clients
{
    public class SourceRconClient : IRconClient
    {
        private readonly ILogger _logger;

        private readonly Regex _playerRegex =
            new Regex(
                "^\\#\\s([0-9]+)\\s([0-9]+)\\s\\\"(.+)\\\"\\s([STEAM0-9:_]+)\\s+([0-9:]+)\\s([0-9]+)\\s([0-9]+)\\s([a-z]+)\\s([0-9]+)\\s((?:(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\\.){3}(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])):?(-?[0-9]{1,5})");

        // ReSharper disable once NotAccessedField.Local
        private GameType _gameType;
        private string _hostname;
        private int _queryPort;
        private string _rconPassword;

        private int _sequenceId = 1;

        private Guid _serverId;
        private TcpClient _tcpClient;

        public SourceRconClient(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Configure(GameType gameType, Guid serverId, string hostname, int queryPort, string rconPassword)
        {
            _logger.LogDebug("[{ServerId}] Configuring Source rcon client for {GameType} with endpoint {Hostname}:{QueryPort}", serverId, gameType, hostname, queryPort);

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
                var match = _playerRegex.Match(line);

                if (!match.Success)
                    continue;

                var num = match.Groups[1].ToString();
                var name = match.Groups[3].ToString();
                var guid = match.Groups[4].ToString();
                var ping = match.Groups[6].ToString();
                var rate = match.Groups[9].ToString();
                var ipAddress = match.Groups[10].ToString();

                _logger.LogDebug("[{ServerId}] Player {Name} with {Guid} and {IpAddress} parsed from result", _serverId, name, guid, ipAddress);

                players.Add(new SourceRconPlayer
                {
                    Num = num,
                    Ping = ping,
                    Guid = guid,
                    Name = name,
                    IpAddress = ipAddress,
                    Rate = rate
                });
            }

            return players;
        }

        public Task Say(string message)
        {
            return Task.CompletedTask;
        }

        public Task<string> Restart()
        {
            return Task.FromResult("Not Implemented");
        }

        public Task<string> RestartMap()
        {
            return Task.FromResult("Not Implemented");
        }

        public Task<string> FastRestartMap()
        {
            return Task.FromResult("Not Implemented");
        }

        public Task<string> NextMap()
        {
            return Task.FromResult("Not Implemented");
        }

        private string PlayerStatus()
        {
            CreateConnection();

            var statusPackets = GetCommandPackets("status");

            _logger.LogDebug("[{ServerId}] Total status packets retrieved from server: {Count}", _serverId, statusPackets.Count);

            var response = new StringBuilder();
            foreach (var packet in statusPackets) response.Append(packet.Body.Trim());

            return response.ToString();
        }

        private SourceRconPacket AuthPacket(string rconPassword)
        {
            return new SourceRconPacket(_sequenceId++, 3, rconPassword);
        }

        private SourceRconPacket ExecuteCommandPacket(string command)
        {
            return new SourceRconPacket(_sequenceId++, 2, command);
        }

        private SourceRconPacket EmptyResponsePacket()
        {
            return new SourceRconPacket(_sequenceId++, 0, string.Empty);
        }

        private void CreateConnection()
        {
            try
            {
                if (_tcpClient != null && _tcpClient.Connected)
                    return;

                _logger.LogDebug("[{ServerId}] Creating a new TcpClient and attempting to authenticate", _serverId);

                _tcpClient = new TcpClient(_hostname, _queryPort) { ReceiveTimeout = 5000 };

                var authPackets = GetAuthPackets(_rconPassword);
                var authResultPacket = authPackets.SingleOrDefault(packet => packet.Type == 2);

                _logger.LogDebug("[{ServerId}] Total auth packets retrieved from server: {Count}", _serverId, authPackets.Count());

                if (authResultPacket == null)
                {
                    _logger.LogError("[{ServerId}] Could not establish authenticated session with server", _serverId);
                    throw new Exception("Could not establish authenticated session with server");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ServerId}] Could not establish TCP connection to server", _serverId);
                throw;
            }
        }

        private List<SourceRconPacket> GetAuthPackets(string rconPassword)
        {
            var endpoint = _tcpClient.Client.RemoteEndPoint;

            var authPacket = AuthPacket(rconPassword);
            _tcpClient.Client.Send(authPacket.PacketBytes);

            var responsePackets = new List<SourceRconPacket>();
            byte[] leftoverBytes = null;
            do
            {
                var tempBuffer = new byte[8192];
                var bytesRead = _tcpClient.Client.ReceiveFrom(tempBuffer, SocketFlags.None, ref endpoint);

                var bytesToProcess = tempBuffer.Take(bytesRead).ToArray();

                if (leftoverBytes != null) bytesToProcess = leftoverBytes.Concat(bytesToProcess).ToArray();

                var (packets, leftover) = BytesIntoPackets(bytesToProcess);
                responsePackets.AddRange(packets);

                leftoverBytes = leftover;
            } while (responsePackets.Count != 2);

            return responsePackets;
        }

        private List<SourceRconPacket> GetCommandPackets(string command)
        {
            var endpoint = _tcpClient.Client.RemoteEndPoint;

            var executeCommandPacket = ExecuteCommandPacket(command);
            _tcpClient.Client.Send(executeCommandPacket.PacketBytes);
            var emptyResponsePacket = EmptyResponsePacket();
            _tcpClient.Client.Send(emptyResponsePacket.PacketBytes);

            var responsePackets = new List<SourceRconPacket>();
            byte[] leftoverBytes = null;
            do
            {
                var tempBuffer = new byte[8192];
                var bytesRead = _tcpClient.Client.ReceiveFrom(tempBuffer, SocketFlags.None, ref endpoint);

                var bytesToProcess = tempBuffer.Take(bytesRead).ToArray();

                if (leftoverBytes != null) bytesToProcess = leftoverBytes.Concat(bytesToProcess).ToArray();

                var (packets, leftover) = BytesIntoPackets(bytesToProcess);
                responsePackets.AddRange(packets);

                leftoverBytes = leftover;
            } while (responsePackets.All(packet => packet.Id != emptyResponsePacket.Id));

            return responsePackets.Where(packet => packet.Id == executeCommandPacket.Id).ToList();
        }

        private static Tuple<List<SourceRconPacket>, byte[]> BytesIntoPackets(byte[] bytes)
        {
            var packets = new List<SourceRconPacket>();
            var offset = 0;

            try
            {
                do
                {
                    if (offset + 4 > bytes.Length)
                        break;

                    var size = BitConverter.ToInt32(bytes, offset);

                    if (size == 0)
                        break;

                    if (offset + size > bytes.Length)
                        break;

                    var id = BitConverter.ToInt32(bytes, offset + 4);
                    var type = BitConverter.ToInt32(bytes, offset + 8);
                    var body = Encoding.ASCII.GetString(bytes.Skip(offset + 12).Take(size - 6).ToArray()).Trim();

                    offset += 4 + size;

                    var packet = new SourceRconPacket(id, type, body);
                    packets.Add(packet);
                } while (true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            var leftover = offset == bytes.Length ? null : bytes.Skip(offset).Take(bytes.Length - offset).ToArray();
            return new Tuple<List<SourceRconPacket>, byte[]>(packets, leftover);
        }
    }
}