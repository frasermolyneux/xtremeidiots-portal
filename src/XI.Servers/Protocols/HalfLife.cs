using System;
using XI.Servers.Models;

namespace XI.Servers.Protocols
{
    public class HalfLife : Protocol
    {
        private const string QueryDetails = @"ÿÿÿÿdetails";
        private const string QueryRules = @"ÿÿÿÿrules";
        private const string QueryPlayers = @"ÿÿÿÿplayers";

        private const string QueryPing = @"ÿÿÿÿping";
        private const string QueryInfo = @"ÿÿÿÿinfostring";

        public HalfLife(string host, int port)
            : base(host, port)
        {
            gameProtocol = GameProtocol.HalfLife;
            Connect(host, port);
        }

        public override string Mod => Params["mod"];

        public override void GetServerInfo()
        {
            base.GetServerInfo();
            if (!IsOnline) return;
            Query(QueryDetails);
            ParseDetails();
        }

        private void ParseDetails()
        {
            Offset = 6;
            Params["serveraddress"] = ReadNextParam();
            Params["hostname"] = ReadNextParam();
            Params["mapname"] = ReadNextParam();
            Params["mod"] = ReadNextParam();
            Params["modname"] = ReadNextParam();
            Params["numplayers"] = Response[Offset++].ToString();
            Params["maxplayers"] = Response[Offset++].ToString();
            Params["protocolver"] = Response[Offset++].ToString();

            Params["servertype"] = ((char) Response[Offset++]).ToString();
            Params["serveros"] = ((char) Response[Offset++]).ToString();
            Params["passworded"] = Response[Offset++].ToString();
            Params["modded"] = Response[Offset].ToString();
        }

        private void ParseRules()
        {
            Offset = 7;

            for (var i = 0; i < BitConverter.ToInt16(Response, 5) * 2; i++)
            {
                var key = ReadNextParam();
                var val = ReadNextParam();
                if (key.Length == 0) continue;
                Params[key] = val;
            }
        }

        private void ParsePlayers()
        {
            Params["numplayers"] = Response[5].ToString();
            Offset = 6;

            for (var i = 0; i < Response[5]; i++)
            {
                var pNr = playerCollection.Add(new LivePlayer());
                playerCollection[pNr].Parameters.Add("playernr", Response[Offset++].ToString());
                playerCollection[pNr].Name = ReadNextParam();
                playerCollection[pNr].Score = BitConverter.ToInt32(Response, Offset);
                Offset += 4;
                playerCollection[pNr].Time = new TimeSpan(0, 0, (int) BitConverter.ToSingle(Response, Offset));
                Offset += 4;
            }
        }
    }
}