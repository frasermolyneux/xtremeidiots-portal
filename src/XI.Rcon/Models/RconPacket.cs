using System;
using System.Collections;
using System.Text;
using XI.Rcon.Clients;

// ReSharper disable InconsistentNaming

namespace XI.Rcon.Models
{
    internal class RconPacket
    {
        public enum SERVERDATA_rec
        {
            SERVERDATA_RESPONSE_VALUE = 0,
            SERVERDATA_AUTH_RESPONSE = 2,
            None = 255
        }

        public enum SERVERDATA_sent
        {
            SERVERDATA_AUTH = 3,
            SERVERDATA_EXECCOMMAND = 2,
            None = 255
        }

        internal int RequestId;
        internal SERVERDATA_rec ServerDataReceived;
        internal SERVERDATA_sent ServerDataSent;
        internal string String1;
        internal string String2;

        internal RconPacket()
        {
            RequestId = 0;
            String1 = "blah";
            String2 = string.Empty;
            ServerDataSent = SERVERDATA_sent.None;
            ServerDataReceived = SERVERDATA_rec.None;
        }

        internal byte[] OutputAsBytes()
        {
            var utf = new UTF8Encoding();

            var bstring1 = utf.GetBytes(String1);
            var bstring2 = utf.GetBytes(String2);

            var serverdata = BitConverter.GetBytes((int) ServerDataSent);
            var reqid = BitConverter.GetBytes(RequestId);

            // Compose into one packet.
            var finalPacket = new byte[4 + 4 + 4 + bstring1.Length + 1 + bstring2.Length + 1];
            var packetsize = BitConverter.GetBytes(finalPacket.Length - 4);

            var bPtr = 0;
            packetsize.CopyTo(finalPacket, bPtr);
            bPtr += 4;

            reqid.CopyTo(finalPacket, bPtr);
            bPtr += 4;

            serverdata.CopyTo(finalPacket, bPtr);
            bPtr += 4;

            bstring1.CopyTo(finalPacket, bPtr);
            bPtr += bstring1.Length;

            finalPacket[bPtr] = 0;
            bPtr++;

            bstring2.CopyTo(finalPacket, bPtr);
            bPtr += bstring2.Length;

            finalPacket[bPtr] = 0;
            // ReSharper disable once RedundantAssignment
            bPtr++;

            return finalPacket;
        }

        internal void ParseFromBytes(byte[] bytes, InsurgencyRconClient parent)
        {
            var bPtr = 0;
            var utf = new UTF8Encoding();

            // First 4 bytes are ReqId.
            RequestId = BitConverter.ToInt32(bytes, bPtr);
            bPtr += 4;
            // Next 4 are server data.
            ServerDataReceived = (SERVERDATA_rec) BitConverter.ToInt32(bytes, bPtr);
            bPtr += 4;
            // string1 till /0
            var stringcache = new ArrayList();
            while (bytes[bPtr] != 0)
            {
                stringcache.Add(bytes[bPtr]);
                bPtr++;
            }

            String1 = utf.GetString((byte[]) stringcache.ToArray(typeof(byte)));
            bPtr++;

            // string2 till /0

            stringcache = new ArrayList();
            while (bytes[bPtr] != 0)
            {
                stringcache.Add(bytes[bPtr]);
                bPtr++;
            }

            String2 = utf.GetString((byte[]) stringcache.ToArray(typeof(byte)));
            bPtr++;

            // Repeat if there's more data?
            if (bPtr != bytes.Length) throw new Exception("Extra data in response");
        }
    }
}