using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using XI.Servers.Rcon.Models;
using XI.Servers.Rcon.Models.Source;

namespace XI.Servers.Rcon.Clients
{
    public class SourceRconClient : BaseRconClient
    {
        private readonly ILogger _logger;
        private int _sequenceId = 1;

        private TcpClient _tcpClient;

        public SourceRconClient(ILogger logger) : base(logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string Result { get; set; }
        private bool Connected { get; set; }

        public override Regex PlayerRegex { get; set; } = new Regex(
            "^\\#\\s([0-9]+)\\s([0-9]+)\\s\\\"(.+)\\\"\\s([STEAM0-9:_]+)\\s+([0-9:]+)\\s([0-9]+)\\s([0-9]+)\\s([a-z]+)\\s([0-9]+)\\s((?:(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\\.){3}(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])):?(-?[0-9]{1,5})");

        private SourceRconPacket AuthPacket(string authPassword)
        {
            return new SourceRconPacket(_sequenceId++, 3, authPassword);
        }

        private SourceRconPacket ExecuteCommandPacket(string command)
        {
            return new SourceRconPacket(_sequenceId++, 2, command);
        }

        private SourceRconPacket EmptyResponsePacket()
        {
            return new SourceRconPacket(_sequenceId++, 0, string.Empty);
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
                var name = match.Groups[3].ToString();
                var guid = match.Groups[4].ToString();
                var ping = match.Groups[6].ToString();
                var rate = match.Groups[9].ToString();
                var ipAddress = match.Groups[10].ToString();

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

        public override string PlayerStatus()
        {
            CreateConnection();

            var statusPackets = GetCommandPackets("status");

            var response = new StringBuilder();
            foreach (var packet in statusPackets) response.Append(packet.Body.Trim());

            return response.ToString();
        }

        public void CreateConnection()
        {
            try
            {
                if (_tcpClient != null && _tcpClient.Connected)
                    return;

                _tcpClient = new TcpClient(Hostname, QueryPort) {ReceiveTimeout = 5000};

                var authPackets = GetAuthPackets(RconPassword);
                var authResultPacket = authPackets.SingleOrDefault(packet => packet.Type == 2);

                if (authResultPacket == null)
                    throw new Exception("Could not create authenticated session with server");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private IEnumerable<SourceRconPacket> GetAuthPackets(string rconPassword)
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

        private IEnumerable<SourceRconPacket> GetCommandPackets(string command)
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

        private Tuple<List<SourceRconPacket>, byte[]> BytesIntoPackets(byte[] bytes)
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