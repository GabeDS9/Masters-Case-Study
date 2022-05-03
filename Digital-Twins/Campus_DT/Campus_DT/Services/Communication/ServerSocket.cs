using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Models;
using Newtonsoft.Json;

namespace Communication
{
    class ServerSocket
    {
        private TcpListener server = null;
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        Communication.ClientSocket myClient = new Communication.ClientSocket();

        public void SetupServer(int port)
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            Console.WriteLine($"Setting up DT server on {((IPEndPoint)server.LocalEndpoint).Port}...");
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            _ = Task.Run(async () => {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    var tcpClient = await server.AcceptTcpClientAsync();
                    _ = HandleTcpClientAsync(tcpClient);
                }
            });
        }

        private async Task HandleTcpClientAsync(TcpClient client)
        {
            Console.WriteLine("UI connected on port: " + ((IPEndPoint)server.LocalEndpoint).Port);

            string request = streamToMessage(client.GetStream());

            if (request != null)
            {
                string responseMessage = await MessageHandlerAsync(request);
                sendMessage(responseMessage, client);
            }
        }

        private static void sendMessage(string message, TcpClient client)
        {
            // messageToByteArray- discussed later
            byte[] bytes = messageToByteArray(message);
            client.GetStream().Write(bytes, 0, bytes.Length);
        }

        public async Task<string> MessageHandlerAsync(string message)
        {
            Console.WriteLine("Gateway Received message from UI: " + message);
            Console.WriteLine("Gateway sending message on port 8005");
            var mesModel = new MessageModel { MessageType = "Energy", MeterID = 2955, Date = "2022-4-20" };
            string mes = JsonConvert.SerializeObject(mesModel);
            return await myClient.sendMessageAsync(mes, 8005);
        }

        // using UTF8 encoding for the messages
        static Encoding encoding = Encoding.UTF8;
        private static byte[] messageToByteArray(string message)
        {
            // get the size of original message
            byte[] messageBytes = encoding.GetBytes(message);
            int messageSize = messageBytes.Length;
            // add content length bytes to the original size
            int completeSize = messageSize + 4;
            // create a buffer of the size of the complete message size
            byte[] completemsg = new byte[completeSize];

            // convert message size to bytes
            byte[] sizeBytes = BitConverter.GetBytes(messageSize);
            // copy the size bytes and the message bytes to our overall message to be sent 
            sizeBytes.CopyTo(completemsg, 0);
            messageBytes.CopyTo(completemsg, 4);
            return completemsg;
        }

        private static string streamToMessage(Stream stream)
        {
            // size bytes have been fixed to 4
            byte[] sizeBytes = new byte[4];
            // read the content length
            stream.Read(sizeBytes, 0, 4);
            int messageSize = BitConverter.ToInt32(sizeBytes, 0);
            // create a buffer of the content length size and read from the stream
            byte[] messageBytes = new byte[messageSize];
            stream.Read(messageBytes, 0, messageSize);
            // convert message byte array to the message string using the encoding
            string message = encoding.GetString(messageBytes);
            string result = null;
            foreach (var c in message)
                if (c != '\0')
                    result += c;

            return result;
        }

        /*private static Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static List<Socket> clientSockets = new List<Socket>();
        private static byte[] buffer = new byte[1024];
        public int port = 0;

        public void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 100));
            serverSocket.Listen(10);
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private void AcceptCallback(IAsyncResult AR)
        {
            Socket socket = serverSocket.EndAccept(AR);
            clientSockets.Add(socket);
            Console.WriteLine("Gateway: Client connected on port: " + ((IPEndPoint)socket.LocalEndPoint).Port.ToString());
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            int received = socket.EndReceive(AR);
            byte[] dataBuf = new byte[received];
            Array.Copy(buffer, dataBuf, received);

            string text = Encoding.ASCII.GetString(dataBuf);
            Console.WriteLine("Text received: " + text);

            foreach(var item in clientSockets)
            {
                    Console.WriteLine(item.RemoteEndPoint);
            }

            string response = string.Empty;

            response = text;
            port = Int32.Parse(response);

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
