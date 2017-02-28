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
using System.Security.Cryptography;
using System.IO;
using Newtonsoft.Json;
using System.IO.Compression;

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
            byte[] decryptedBytes = Decrypt(data, "password");
            byte[] decompressedBytes = DecompressLZMA(decryptedBytes);
            string decompressed = Encoding.ASCII.GetString(decompressedBytes);
            return JsonConvert.DeserializeObject<InputMessage>(decompressed);
        }

        string decodeData(string data)
        {
            return Encoding.ASCII.GetString(Convert.FromBase64String(data));
        }

        private static byte[] DecompressLZMA(byte[] data)
        {
            SevenZip.Compression.LZMA.Decoder coder = new SevenZip.Compression.LZMA.Decoder();
            MemoryStream input = new MemoryStream(data);
            MemoryStream output = new MemoryStream();

            // Read the decoder properties
            byte[] properties = new byte[5];
            input.Read(properties, 0, 5);

            // Read in the decompress file size.
            byte[] fileLengthBytes = new byte[8];
            input.Read(fileLengthBytes, 0, 8);
            long len = BitConverter.ToInt64(fileLengthBytes, 0);

            coder.SetDecoderProperties(properties);
            coder.Code(input, output, input.Length, len, null);
            output.Flush();

            return output.GetBuffer();
        }

        public static byte[] Decrypt(byte[] cipherTextBytesWithSaltAndIv, string passPhrase)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                return plainTextBytes;
                            }
                        }
                    }
                }
            }
        }


    }
}
