using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.IO.Compression;
using CSCommon;

namespace ZubmarineGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ImageSource videoImageSource0 { get; set; }
        Thread serverThread;

        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 256;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            videoImageSource0 = new BitmapImage();
            serverThread = new Thread(serverThreadFunction);
            serverThread.Start();
        }

        void serverThreadFunction()
        {
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
