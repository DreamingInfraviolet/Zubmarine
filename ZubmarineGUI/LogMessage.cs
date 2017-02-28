using CSCommon;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZubmarineGUI
{
    public struct LogMessage
    {
        private LogMessage(MessageType type, string name, string payload)
        {
            this.type = type;
            this.data = new Dictionary<string, string>();
            this.data["name"] = name;
            this.data["payload"] = payload;
        }

        public enum MessageType
        {
            String
        }

        public MessageType type;
        public Dictionary<string, string> data;


        public static LogMessage fromString(string str, string name)
        {
            return new LogMessage(MessageType.String, name, str);
        }

        public string asString()
        {
            throwIfNotType(MessageType.String);
            return data["payload"];
        }

        private void throwIfNotType(MessageType expected)
        {
            if (type != expected)
                throw new ArgumentException("Expected message type to be "
                    + expected + ", found " + type);
        }

        public static byte[] encodeHeaderData(LogMessage message)
        {
            var oldData = message.data;
            message.encodeData();

            string json = JsonConvert.SerializeObject(message);
            byte[] jsonBytes = Encoding.ASCII.GetBytes(json);
            byte[] compressedJson = Compression.CompressLZMA(jsonBytes);
            byte[] encrypted = Crypto.Encrypt(compressedJson, "password");
            int messagelen = encrypted.Length;
            byte[] messageHeader = BitConverter.GetBytes(messagelen);
            byte[] res = messageHeader.Concat(encrypted).ToArray();
            message.data = oldData;

            return res;
        }

        public static LogMessage decodeData(byte[] data)
        {
            byte[] decryptedBytes = Crypto.Decrypt(data, "password");
            byte[] decompressedBytes = Compression.DecompressLZMA(decryptedBytes);
            string decompressed = Encoding.ASCII.GetString(decompressedBytes);
            var obj = JsonConvert.DeserializeObject<LogMessage>(decompressed);
            obj.decodeData();
            return obj;
        }

        private void encodeData()
        {
            var copy = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> entry in data)
            {
                copy[entry.Key] = Convert.ToBase64String(Encoding.ASCII.GetBytes(entry.Value));
            }
            data = copy;
        }

        private void decodeData()
        {
            var copy = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> entry in data)
            {
                copy[entry.Key] = Encoding.ASCII.GetString(Convert.FromBase64String(entry.Value));
            }
            data = copy;
        }
    }
}
