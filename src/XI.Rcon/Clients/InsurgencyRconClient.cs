using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Logging;
using XI.Rcon.Models;

namespace XI.Rcon.Clients
{
    public class InsurgencyRconClient : BaseRconClient
    {
        private readonly ILogger _logger;
        private int _packetCount;

        private Socket _socket;

        public InsurgencyRconClient(ILogger logger) : base(logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string Result { get; set; }

        private bool Connected { get; set; }

        public override string PlayerStatus()
        {
            return ExecuteCommand("status");
        }

        private string ExecuteCommand(string rconCommand)
        {
            _logger.LogInformation("[{serverName}] Executing {command} command against server", ServerName, rconCommand);

            try
            {
                if (!Connected)
                {
                    _logger.LogDebug("[{serverName}] RconClient is not connected, connecting to {hostname}:{queryPort}", ServerName, Hostname, QueryPort);
                    Connected = Connect(new IPEndPoint(IPAddress.Parse(Hostname), QueryPort), RconPassword);
                    _logger.LogDebug("[{serverName}] The RconClient state is now {state}", ServerName, Connected);
                }

                var packetToSend = new RconPacket
                {
                    RequestId = 2,
                    ServerDataSent = RconPacket.SERVERDATA_sent.SERVERDATA_EXECCOMMAND,
                    String1 = rconCommand
                };

                SendRconPacket(packetToSend);

                var timeout = DateTime.UtcNow;
                while (string.IsNullOrWhiteSpace(Result) && timeout < DateTime.UtcNow.AddSeconds(10)) Thread.Sleep(100);

                return Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute {command} against server", rconCommand);
                return string.Empty;
            }
        }

        public bool Connect(IPEndPoint server, string password)
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(server);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{serverName}] Failed to connect to server", ServerName);
                return false;
            }

            var serverAuth = new RconPacket
            {
                RequestId = 1,
                String1 = password,
                ServerDataSent = RconPacket.SERVERDATA_sent.SERVERDATA_AUTH
            };

            SendRconPacket(serverAuth);
            StartGetNewPacket();

            return true;
        }

        private void SendRconPacket(RconPacket p)
        {
            var packet = p.OutputAsBytes();
            _socket.BeginSend(packet, 0, packet.Length, SocketFlags.None, SendCallback, this);
        }

        private void SendCallback(IAsyncResult ar)
        {
            _socket.EndSend(ar);
        }

        private void StartGetNewPacket()
        {
            var state = new RecState
            {
                IsPacketLength = true,
                Data = new byte[4],
                PacketCount = _packetCount
            };

            _packetCount++;

            _socket.BeginReceive(state.Data, 0, 4, SocketFlags.None, ReceiveCallback, state);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var bytesgotten = _socket.EndReceive(ar);
                var state = (RecState) ar.AsyncState;
                state.BytesSoFar += bytesgotten;

                ProcessIncomingData(state);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{serverName}] Failed receiving data from the server", ServerName);
                Result = ex.Message;
                //throw;
            }
        }

        private void ProcessIncomingData(RecState state)
        {
            if (state.IsPacketLength)
            {
                // First 4 bytes of a new packet are the total packet length
                state.PacketLength = BitConverter.ToInt32(state.Data, 0);

                state.IsPacketLength = false;
                state.BytesSoFar = 0;
                state.Data = new byte[state.PacketLength];
                _socket.BeginReceive(state.Data, 0, state.PacketLength, SocketFlags.None, ReceiveCallback, state);
            }
            else
            {
                if (state.BytesSoFar < state.PacketLength)
                {
                    _socket.BeginReceive(state.Data, state.BytesSoFar, state.PacketLength - state.BytesSoFar, SocketFlags.None, ReceiveCallback, state);
                }
                else
                {
                    var returnPacket = new RconPacket();
                    returnPacket.ParseFromBytes(state.Data, this);

                    ProcessResponse(returnPacket);

                    StartGetNewPacket();
                }
            }
        }

        private void ProcessResponse(RconPacket packet)
        {
            _logger.LogDebug("[{serverName}] Packet ServerDataReceived {ServerDataReceived} has been received from server", ServerName, packet.ServerDataReceived);
            _logger.LogDebug("[{serverName}] Packet ServerDataSent {ServerDataSent} has been received from server", ServerName, packet.ServerDataSent);
            _logger.LogDebug("[{serverName}] Packet String1 {String1} has been received from server", ServerName, packet.String1);
            _logger.LogDebug("[{serverName}] Packet String2 {String2} has been received from server", ServerName, packet.String2);

            switch (packet.ServerDataReceived)
            {
                case RconPacket.SERVERDATA_rec.SERVERDATA_AUTH_RESPONSE:
                    if (packet.RequestId != -1)
                    {
                        // Connected.
                        Connected = true;
                        _logger.LogInformation("[{serverName}] Successfully connected to server", ServerName);
                    }
                    else
                    {
                        _logger.LogError("[{serverName}] Failed to connect to server", ServerName);
                    }

                    break;
                case RconPacket.SERVERDATA_rec.SERVERDATA_RESPONSE_VALUE:
                    //Result = Encoding.UTF8.GetString(packet.OutputAsBytes());
                    Result = packet.String1;
                    break;
                default:
                    _logger.LogError("[{serverName}] Unknown response from server", ServerName);
                    break;
            }
        }
    }
}