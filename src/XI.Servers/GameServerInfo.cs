using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using XI.CommonTypes;
using XI.Servers.Extensions;
using XI.Servers.Protocols;

namespace XI.Servers
{
    public class GameServerInfo
    {
        private bool _debugMode;
        private Protocol _serverInfo;

        public GameServerInfo(string host, int port, GameType type)
        {
            Host = host;
            Port = port;
            GameType = type;

            CheckServerType();
        }

        public GameServerInfo(string host, int port, GameType type, int timeout)
        {
            Host = host;
            Port = port;
            GameType = type;

            CheckServerType();
        }

        public int Timeout
        {
            get => _serverInfo.Timeout;
            set => _serverInfo.Timeout = value;
        }

        public StringDictionary Parameters => _serverInfo.Parameters;

        public bool IsOnline => _serverInfo.IsOnline;

        public DateTime ScanTime => _serverInfo.ScanTime;

        public PlayerCollection Players => _serverInfo.PlayerCollection;

        public StringCollection Teams => _serverInfo.Teams;

        public int MaxPlayers => _serverInfo.MaxPlayers;

        public int NumPlayers => _serverInfo.NumPlayers;

        public string Name => _serverInfo.Name;

        public string Mod => _serverInfo.Mod;

        public string Map => _serverInfo.Map;

        public bool Passworded => _serverInfo.Passworded;

        public GameType GameType { get; }

        public string Host { get; }

        public int Port { get; }

        public bool DebugMode
        {
            get => _debugMode;
            set
            {
                if (_serverInfo != null) _serverInfo.DebugMode = value;
                _debugMode = value;
            }
        }

        private void CheckServerType()
        {
            var gameProtocol = GameType.Protocol();

            switch (gameProtocol)
            {
                case GameProtocol.Samp:
                    _serverInfo = new Samp(Host, Port);
                    break;
                case GameProtocol.Ase:
                    _serverInfo = new Ase(Host, Port);
                    break;
                case GameProtocol.Doom3:
                    _serverInfo = new Doom3(Host, Port);
                    break;
                case GameProtocol.GameSpy:
                    _serverInfo = new GameSpy(Host, Port);
                    break;
                case GameProtocol.GameSpy2:
                    _serverInfo = new GameSpy2(Host, Port);
                    break;
                case GameProtocol.HalfLife:
                    _serverInfo = new HalfLife(Host, Port);
                    break;
                case GameProtocol.Quake3:
                    _serverInfo = new Quake3(Host, Port);
                    break;
                case GameProtocol.Source:
                    _serverInfo = new Source(Host, Port);
                    break;
                default:
                    throw new NotImplementedException();
            }

            _serverInfo.DebugMode = _debugMode;
        }

        public void QueryServer()
        {
            _serverInfo.GetServerInfo();
        }

        public static string CleanName(string name)
        {
            var regex = new Regex(@"(\^\d)|(\$\d)");
            return regex.Replace(name, "");
        }
    }
}