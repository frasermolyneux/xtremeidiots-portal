using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XI.Servers.Models;

namespace XI.Servers.Protocols
{
    public class Source : Protocol
    {
        private const string QueryGetChallenge = @"ÿÿÿÿUÿÿÿÿ";
        private const string QueryDetails = @"ÿÿÿÿTSource Engine Query";
        private const string QueryRules = @"ÿÿÿÿV";
        private const string QueryPlayers = @"ÿÿÿÿU";

        private string challenge;

        public Source(string host, int port) : base(host, port)
        {
            gameProtocol = GameProtocol.Source;
            Connect(host, port);
        }

        public override void GetServerInfo()
        {
            base.GetServerInfo();
            if (!IsOnline) return;

            Query(QueryDetails + Encoding.Default.GetString(new byte[] {0x00}));
            ParseDetails();

            Query(QueryGetChallenge);
            var challengeResponse = Response.Skip(5).ToArray();
            Query(QueryPlayers + Encoding.Default.GetString(challengeResponse));
            ParsePlayers();
        }

        private void ParseChallenge()
        {
            challenge = Encoding.Default.GetString(Response, 5, 4);
        }

        private void ParseDetails()
        {
            Params["protocolver"] = Response[5].ToString();
            Params["hostname"] = ReadNextParam(6);
            Params["mapname"] = ReadNextParam();
            Params["mod"] = ReadNextParam();
            Params["modname"] = ReadNextParam();
            var appid = new byte[2];
            Array.Copy(Response, Offset++, appid, 0, 2);
            Params["appid"] = BitConverter.ToInt16(appid, 0).ToString();
            Offset++;
            Params["numplayers"] = Response[Offset++].ToString();
            Params["maxplayers"] = Response[Offset++].ToString();
            Params["botcount"] = Response[Offset++].ToString();
            Params["servertype"] = Response[Offset++].ToString();
            Params["serveros"] = Response[Offset++].ToString();
            Params["passworded"] = Response[Offset++].ToString();
            Params["secureserver"] = Response[Offset++].ToString();
            Params["version"] = ReadNextParam();
        }

        private void ParseRules()
        {
            int ruleCount = BitConverter.ToInt16(Response, 5);
            Offset = 7;

            for (var i = 0; i < ruleCount * 2; i++)
            {
                var key = ReadNextParam();
                var val = ReadNextParam();
                if (key.Length == 0) continue;
                Params[key] = val;
            }
        }

        private void ParsePlayers()
        {
            var numPlayers = Response[5];
            Params["numplayers"] = numPlayers.ToString();

            var i = 6;
            for (var ii = 0; ii < numPlayers; ii++)
            {
                var newPlayer = new LivePlayer();

                // ReSharper disable once UnusedVariable
                var index = Response[i++];

                var playerNameArr = new List<byte>();

                while (Response[i] != 0x00) playerNameArr.Add(Response[i++]);

                i++;

                newPlayer.Name = Encoding.UTF8.GetString(playerNameArr.ToArray());
                newPlayer.Score = BitConverter.ToInt32(Response, i);

                i += 4;

                newPlayer.Time = new TimeSpan(0, 0, (int) BitConverter.ToSingle(Response, i));

                i += 4;

                playerCollection.Add(newPlayer);
            }
        }
    }
}