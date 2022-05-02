using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Communication
{
    class ClientSocket
    {
        private Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public async Task LoopConnectAsync(int port)
        {
            int attempts = 0;
            TcpClient client = new TcpClient();

            while (!client.Connected)
            {
                try
                {

                    attempts++;

                    await client.ConnectAsync(IPAddress.Loopback, port);
                    Console.WriteLine("Connected to DT on port " + port);

                    /*NetworkStream ns = client.GetStream();

                    StreamWriter sw = new StreamWriter(ns);
                    await sw.WriteLineAsync("Client sending message");

                    await ns.FlushAsync();

                    StreamReader sr = new StreamReader(ns);

                    string message = await sr.ReadToEndAsync();

                    Console.WriteLine(message);*/
                }
                catch (SocketException)
                {
                    Console.WriteLine($"Connecting attempts to {port}: " + attempts.ToString());
                }
            }

            //Console.WriteLine("Connected to server");
        }

        public void SendMessageToServer(string name)
        {
            string req = "get time";
            byte[] buffer = Encoding.ASCII.GetBytes(req);
            clientSocket.Send(buffer);

            byte[] receivedBuf = new byte[1024];
            int rec = clientSocket.Receive(receivedBuf);
            byte[] data = new byte[rec];
            Array.Copy(receivedBuf, data, rec);
            Console.WriteLine("Received: " + Encoding.ASCII.GetString(data) + " for " + name);
        }
    }
}
