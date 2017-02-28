using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.IO.Compression;
using CSCommon;
using System.Threading;
using System.Configuration;

namespace ZubmarineGUI
{
    class Server
    {
        Thread serverThread;
        MainWindow mainWindow;

        public Server(MainWindow window)
        {
            this.mainWindow = window;
        }

        public void start()
        {
            serverThread = new Thread(serverThreadFunction);
            serverThread.Start();
        }

        void serverThreadFunction()
        {
            int port = int.Parse(ConfigurationManager.AppSettings["port"]);
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 54361);
            Socket newsock = new Socket(AddressFamily.InterNetwork,
                                SocketType.Stream, ProtocolType.Tcp);

            Console.WriteLine("Bound to port " + localEndPoint.Port);

            while (true)
            {
                newsock.Bind(localEndPoint);
                newsock.Listen(10);
                Socket client = newsock.Accept();

                Console.WriteLine("Client connected " + client);

                while (client.Connected)
                {
                    Console.WriteLine("Data recieved.");

                    byte[] header = readSpecificAmount(client, 4);

                    int len = BitConverter.ToInt32(header, 0);
                    Console.WriteLine("Data len: " + len);

                    byte[] data = readSpecificAmount(client, len);

                    string datastr = System.Text.ASCIIEncoding.Default.GetString(data);
                    Console.WriteLine("Received data: " + datastr);

                    var message = decodeMessage(data);

                    Console.WriteLine("Decoded: " + message);

                    var decodedData = decodeData(message.data64);

                    Console.WriteLine("Decoded data: " + decodedData);
                }

                Console.WriteLine("Lost client");
            }
        }

        byte[] readSpecificAmount(Socket socket, int amount)
        {
            int received = 0;
            byte[] data = new byte[amount];

            while (received < amount)
            {
                int leftToRead = amount - received;
                received += socket.Receive(data, received, leftToRead, SocketFlags.None);
            }

            return data;
        }

        InputMessage decodeMessage(byte[] data)
        {
            byte[] decryptedBytes = Crypto.Decrypt(data, "password");
            byte[] decompressedBytes = Compression.DecompressLZMA(decryptedBytes);
            string decompressed = Encoding.ASCII.GetString(decompressedBytes);
            return JsonConvert.DeserializeObject<InputMessage>(decompressed);
        }

        string decodeData(string data)
        {
            return Encoding.ASCII.GetString(Convert.FromBase64String(data));
        }
    }
}
