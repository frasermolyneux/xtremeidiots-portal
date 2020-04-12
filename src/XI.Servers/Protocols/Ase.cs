using System;
using XI.Servers.Models;

namespace XI.Servers.Protocols
{
    public class Ase : Protocol
    {
        private const string QueryBase = "s";

        public Ase(string host, int port)
            : base(host, port)
        {
            gameProtocol = GameProtocol.Ase;
            Connect(host, port);
        }

        public override string Mod => null;

        public override void GetServerInfo()
        {
            base.GetServerInfo();
            if (!IsOnline) return;
            Query(QueryBase);
            ParseDetails();
        }

        private void ParseDetails()
        {
            Offset = 5;
            Params["gamename"] = ReadNextString();
            Params["port"] = ReadNextString();
            Params["hostname"] = ReadNextString();
            Params["gametype"] = ReadNextString();
            Params["mapname"] = ReadNextString();
            Params["version"] = ReadNextString();
            Params["passworded"] = ReadNextString();
            Params["players"] = ReadNextString();
            Params["maxplayers"] = ReadNextString();

            while (Offset < Response.Length)
            {
                if (Response[Offset - 1] == 1) break;
                Params[ReadNextString()] = ReadNextString();
            }

            var checkOffset = Offset;
            while (Offset < Response.Length)
            {
                var pNr = playerCollection.Add(new LivePlayer());
                Offset += 2;

                if ((Response[checkOffset] & 1) != 0) playerCollection[pNr].Name = ReadNextString();
                if ((Response[checkOffset] & 2) != 0) playerCollection[pNr].Team = ReadNextString();
                if ((Response[checkOffset] & 4) != 0) playerCollection[pNr].Parameters.Add("skin", ReadNextString());
                if ((Response[checkOffset] & 8) != 0) playerCollection[pNr].Score = short.Parse(ReadNextString());
                if ((Response[checkOffset] & 16) != 0) playerCollection[pNr].Ping = short.Parse(ReadNextString());
                if ((Response[checkOffset] & 32) != 0)
                {
                    int time = short.Parse(ResponseString[Offset++].ToString());
                    playerCollection[pNr].Time = (char) Response[Offset] == 'm'
                        ? new TimeSpan(0, time, 0)
                        : new TimeSpan(time, 0, 0);
                }

                try
                {
                    Offset++;
                    if (Response[checkOffset] != Response[Offset]) Offset++;
                    if ((Response[checkOffset] & 64) != 0) ReadNextString();
                    if ((Response[checkOffset] & 128) != 0) ReadNextString();
                }
                catch (ArgumentOutOfRangeException)
                {
                    break;
                }
            }
        }

        private string ReadNextString()
        {
            var oldOffset = Offset;
            Offset += Response[Offset - 1];

            return ResponseString.Substring(oldOffset, Response[oldOffset - 1] - 1);
        }
    }
}