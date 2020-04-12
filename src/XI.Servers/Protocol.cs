using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace XI.Servers
{
    public abstract class Protocol
    {
        protected string _requestString = "";
        protected string _responseString = "";
        protected StringCollection _teams;
        protected GameProtocol gameProtocol;
        internal string Host;
        protected int Packages;
        protected StringDictionary Params;
        protected PlayerCollection playerCollection;
        internal int Port;
        private bool queryInProgress;
        private IPEndPoint remoteIpEndPoint;
        private byte[] sendBuffer;
        private Socket serverConnection;
        protected bool ServerOnline = true;

        protected Protocol()
        {
            playerCollection = new PlayerCollection();
            Params = new StringDictionary();
            _teams = new StringCollection();
        }

        protected Protocol(string host, int port) : this()
        {
            Host = host;
            Port = port;
        }

        protected byte[] Response { get; private set; }

        protected string ResponseString => _responseString;

        protected int Offset { get; set; }

        public int Timeout { get; set; } = 1500;

        public StringDictionary Parameters => Params;

        public StringCollection Teams => _teams;

        public PlayerCollection PlayerCollection => playerCollection;

        public int NumPlayers => playerCollection.Count == 0 && Params["numplayers"] != null
            ? short.Parse(Params["numplayers"])
            : playerCollection.Count;

        public DateTime ScanTime { get; private set; }

        public bool DebugMode { get; set; }

        public virtual string Name => !ServerOnline ? null : Params["hostname"];

        public virtual string Mod => !ServerOnline ? null : Params["modname"];

        public virtual string Map => !ServerOnline ? null : Params["mapname"];

        public virtual bool Passworded => Params.ContainsKey("passworded") && Params["passworded"] != "0";

        public virtual bool IsOnline => ServerOnline;

        public virtual int MaxPlayers
        {
            get
            {
                try
                {
                    return short.Parse(Params["maxplayers"]);
                }
                catch
                {
                    return -1;
                }
            }
        }

        protected void Connect(string host, int port)
        {
            serverConnection = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            serverConnection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, Timeout);

            IPAddress ip;
            try
            {
                ip = IPAddress.Parse(host);
            }
            catch (FormatException)
            {
                ip = Dns.GetHostEntry(host).AddressList[0];
            }

            remoteIpEndPoint = new IPEndPoint(ip, port);
            ServerOnline = true;
        }

        protected void Connect()
        {
            Connect(Host, Port);
        }

        protected void Query(string request)
        {
            if (queryInProgress) throw new InvalidOperationException("Another query for this server is in progress");
            queryInProgress = true;
            Response = new byte[100 * 1024];
            EndPoint remoteEndPoint = remoteIpEndPoint;
            Packages = 0;
            int read;
            var bufferOffset = 0;

            sendBuffer = Encoding.Default.GetBytes(request);
            serverConnection.SendTo(sendBuffer, remoteIpEndPoint);

            do
            {
                read = 0;
                try
                {
                    if (Packages > 0)
                        switch (gameProtocol)
                        {
                            case GameProtocol.HalfLife:
                            case GameProtocol.Source:
                                var tempBuffer = new byte[100 * 1024];
                                read = serverConnection.ReceiveFrom(tempBuffer, ref remoteEndPoint);

                                var packets = tempBuffer[8] & 15;
                                var packetNr = (tempBuffer[8] >> 4) + 1;

                                if (packetNr < packets)
                                {
                                    Array.Copy(Response, 9, tempBuffer, read, bufferOffset);
                                    Response = tempBuffer;
                                }
                                else
                                {
                                    Array.Copy(tempBuffer, 9, Response, bufferOffset, read);
                                }

                                tempBuffer = null;
                                break;

                            case GameProtocol.GameSpy:
                            case GameProtocol.GameSpy2:
                                read = serverConnection.ReceiveFrom(Response, bufferOffset,
                                    Response.Length - bufferOffset, SocketFlags.None, ref remoteEndPoint);
                                break;
                            case GameProtocol.Samp:
                                break;
                            case GameProtocol.Doom3:
                                break;
                            case GameProtocol.None:
                                break;
                            default:
                            case GameProtocol.Ase:
                            case GameProtocol.Quake3:
                                break;
                        }
                    else
                        read = serverConnection.ReceiveFrom(Response, ref remoteEndPoint);

                    bufferOffset += read;
                    Packages++;
                }
                catch (SocketException e)
                {
                    Trace.TraceError("Socket exception " + e.SocketErrorCode + " " + e.Message);
                    break;
                }
            } while (read > 0);

            ScanTime = DateTime.Now;

            if (bufferOffset > 0 && bufferOffset < Response.Length)
            {
                var temp = new byte[bufferOffset];
                for (var i = 0; i < temp.Length; i++) temp[i] = Response[i];
                Response = temp;
            }
            else
            {
                Trace.TraceError("Answer is either zero-length or exceeds buffer length");
                ServerOnline = false;
            }

            _responseString = Encoding.Default.GetString(Response);
            queryInProgress = false;

            if (!DebugMode) return;
            var stream = File.OpenWrite("LastQuery.dat");
            stream.Write(Response, 0, Response.Length);
            stream.Close();
        }

        protected void AddParams(string[] parts)
        {
            if (!IsOnline) return;
            for (var i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length == 0) continue;
                var key = parts[i++];
                var val = parts[i];

                if (key == "final") break;
                if (key == "querid") continue;

                Params[key] = val;
            }
        }

        protected string ReadNextParam(int offset)
        {
            if (offset > Response.Length) throw new IndexOutOfRangeException();
            Offset = offset;
            return ReadNextParam();
        }

        protected string ReadNextParam()
        {
            var temp = "";
            for (; Offset < Response.Length; Offset++)
            {
                if (Response[Offset] == 0)
                {
                    Offset++;
                    break;
                }

                temp += (char) Response[Offset];
            }

            return temp;
        }

        public virtual void GetServerInfo()
        {
            if (!IsOnline) Connect();
        }
    }
}