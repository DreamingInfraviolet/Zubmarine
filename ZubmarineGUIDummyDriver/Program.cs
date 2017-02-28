using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ZubmarineGUI;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CSCommon;

namespace ZubmarineGUIDummyDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press ESC to stop");

            var address = IPAddress.Loopback;
            var endpoint = new IPEndPoint(address, 54361);
            var socket = new Socket(AddressFamily.InterNetwork,
                                    SocketType.Stream,
                                    ProtocolType.Tcp);

            do
            {
                while (!Console.KeyAvailable)
                {
                    try
                    {
                        if (!socket.Connected)
                        {
                            socket.Connect(endpoint);
                        }
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Could not connect to server: " + e.ToString());
                        continue;
                    }
                    
                    try
                    {
                        byte[][] messages = getMessages();
                        foreach (var message in messages)
                        {
                            socket.Send(message);
                        }
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Error sending: " + e.ToString());
                    }
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }

        static byte[][] getMessages()
        {
            var message = new InputMessage();
            var messageJson = JsonConvert.SerializeObject(message);
            message.type = InputMessage.MessageType.Test;
            var datastr = "I am kawaii data!!!";
            var data = Encoding.ASCII.GetBytes(datastr);
            message.data64 = encodeData(data);

            return new byte[][] { encodeMessage(message) };
        }
        
        /** Encrypts data and converts it to base64 */
        static string encodeData(byte[] data)
        {
            return Convert.ToBase64String(data);
        }

        /** Converts message to json */
        static byte[] encodeMessage(InputMessage message)
        {
            // Compress first and then encrypt, as the encryption
            // entropy negates compression.
            string messagestr = JsonConvert.SerializeObject(message);
            byte[] messageData = Encoding.ASCII.GetBytes(messagestr);
            
            byte[] compressedMessageData = Compression.CompressLZMA(messageData);

            byte[] encryptedMessageData = Crypto.Encrypt(compressedMessageData, "password");

            int messagelen = encryptedMessageData.Length;
            byte[] messageHeader = BitConverter.GetBytes(messagelen);

            byte[] res = messageHeader.Concat(encryptedMessageData).ToArray();

            Console.WriteLine("Before compression: " + messageData.Length);
            Console.WriteLine("After compressedMessageData: " + compressedMessageData.Length);
            Console.WriteLine("After encryption: " + encryptedMessageData.Length);

            return res;
        }

        public static string base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}
