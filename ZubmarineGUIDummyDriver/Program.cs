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
using System.Configuration;

namespace ZubmarineGUIDummyDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press ESC to stop");

            var address = IPAddress.Loopback;
            int port = int.Parse(ConfigurationManager.AppSettings["port"]);
            var endpoint = new IPEndPoint(address, port);
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
            message.data = "I am kawaii data!!!";

            return new byte[][] { InputMessage.encodeHeaderData(message) };
        }
    }
}
