using CSCommon;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZubmarineGUI
{
    public struct InputMessage
    {
        public enum MessageType
        {
            Test
        }

        public MessageType type;
        public string data;

        public static byte[] encodeHeaderData(InputMessage message)
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

        public static InputMessage decodeData(byte[] data)
        {
            byte[] decryptedBytes = Crypto.Decrypt(data, "password");
            byte[] decompressedBytes = Compression.DecompressLZMA(decryptedBytes);
            string decompressed = Encoding.ASCII.GetString(decompressedBytes);
            var obj = JsonConvert.DeserializeObject<InputMessage>(decompressed);
            obj.decodeData();
            return obj;
        }

        private void encodeData()
        {
            data = Convert.ToBase64String(Encoding.ASCII.GetBytes(data));
        }

        private void decodeData()
        {
            data = Encoding.ASCII.GetString(Convert.FromBase64String(data));
        }
    }
}
