using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services_Communication
{
    class ServerSocket
    {
        private static Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        TcpListener server = null;
        private static List<Socket> clientSockets = new List<Socket>();
        private static byte[] buffer = new byte[1024];
        private static int clientPort;
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private TcpListener listener = null;

        public void SetupServer(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"Setting up DT server on {((IPEndPoint)listener.LocalEndpoint).Port}...");
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            _ = Task.Run(async () => {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    var tcpClient = await listener.AcceptTcpClientAsync();
                    _ = this.HandleTcpClientAsync(tcpClient);
                }
            });

            /*server = new TcpListener(IPAddress.Any, port);
            server.Start();
            Console.WriteLine($"Setting up DT server on {((IPEndPoint)server.LocalEndpoint).Port}...");
            _ = Task.Run(async () => {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    var tcpClient = await server.AcceptTcpClientAsync();
                    _ = this.HandleTcpClientAsync(tcpClient);
                }
            });*/
            //server.AcceptTcpClientAsync(new AsyncCallback(AcceptCallback), null);
            //serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            //Console.WriteLine($"Setting up DT server on {((IPEndPoint)serverSocket.LocalEndPoint).Port}...");
            //serverSocket.Listen(1);
            //serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private async Task HandleTcpClientAsync(TcpClient client)
        {
            Console.WriteLine("Service gateway connected on port: " + ((IPEndPoint)listener.LocalEndpoint).Port);

            /*NetworkStream ns = client.GetStream();

            StreamReader sr = new StreamReader(ns);
            string message = await sr.ReadToEndAsync();

            Console.WriteLine($"Message received from client on {((IPEndPoint)listener.LocalEndpoint).Port}: {message}");

            StreamWriter sw = new StreamWriter(ns);
            await sw.WriteLineAsync("YO DAWG WASSAP?");

            await ns.FlushAsync();*/
        }

        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket = serverSocket.EndAccept(AR);
            clientSockets.Add(socket);
            clientPort = ((IPEndPoint)socket.RemoteEndPoint).Port;
            Console.WriteLine("Service gateway connected on port: " + ((IPEndPoint)socket.RemoteEndPoint).Port.ToString());
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            int received = socket.EndReceive(AR);
            byte[] dataBuf = new byte[received];
            Array.Copy(buffer, dataBuf, received);

            string text = Encoding.ASCII.GetString(dataBuf);
            Console.WriteLine("Text received: " + text);

            string response = string.Empty;

            if (text.ToLower() == "dt")
            {
                response = "From DT";
            }
            else if (text.ToLower() == "ui")
            {
                response = "From UI";
            }

            byte[] data = Encoding.ASCII.GetBytes(response);
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);

            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }

        private static void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }

        /*public Socket listener;
        public IPEndPoint localEndPoint;
        public void StartServer()
        {
            // Establish the local endpoint
            // for the socket. Dns.GetHostName
            // returns the name of the host
            // running the application.
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[0];
            localEndPoint = new IPEndPoint(ipAddr, 11111);

            // Creation TCP/IP Socket using
            // Socket Class Constructor
            listener = new Socket(ipAddr.AddressFamily,
                         SocketType.Stream, ProtocolType.Tcp);

            // Using Bind() method we associate a
            // network address to the Server Socket
            // All client that will connect to this
            // Server Socket must know this network
            // Address
            listener.Bind(localEndPoint);

            // Using Listen() method we create
            // the Client list that will want
            // to connect to Server
            listener.Listen(30);
        }

        public (String, Socket) ListenForMessages()
        {
            try
            {
                while (true)
                {

                    Console.WriteLine("Waiting connection ... ");

                    // Suspend while waiting for
                    // incoming connection Using
                    // Accept() method the server
                    // will accept connection of client
                    Socket clientSocket = listener.Accept();

                    // Data buffer
                    byte[] bytes = new Byte[1024];
                    string data = null;

                    while (true)
                    {

                        int numByte = clientSocket.Receive(bytes);

                        data += Encoding.ASCII.GetString(bytes,
                                                   0, numByte);

                        if (data.IndexOf("<EOF>") > -1)
                            break;
                    }

                    Console.WriteLine("Text received -> {0} ", data);
                    return (data, clientSocket);
                }
            }            
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return (null, null);
        }

        private void Client(Socket client)
        {

        }

        public void SendMessage(String mes, Socket clientSocket)
        {
            byte[] message = Encoding.ASCII.GetBytes(mes);

            // Send a message to Client
            // using Send() method
            clientSocket.Send(message);

            // Close client Socket using the
            // Close() method. After closing,
            // we can use the closed Socket
            // for a new Client Connection
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
    }*/
    }
}
