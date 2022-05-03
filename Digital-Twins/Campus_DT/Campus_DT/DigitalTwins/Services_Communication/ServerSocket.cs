using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataAccess;
using Newtonsoft.Json;
using Models;
using Building_DT;
using Precinct_DT;
using Campus_DT;

namespace Services_Communication
{
    class ServerSocket
    {
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private TcpListener listener = null;
        private Building building = null;
        private Precinct precinct = null;
        private CampusManager campus = null;

        public void SetupServer(int port, Building build, Precinct prec, CampusManager camp)
        {
            building = build;
            precinct = prec;
            campus = camp;
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"Setting up DT server on {((IPEndPoint)listener.LocalEndpoint).Port}...");
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            _ = Task.Run(async () => {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    var tcpClient = await listener.AcceptTcpClientAsync();
                    _ = HandleTcpClientAsync(tcpClient);
                }
            });
        }

        private async Task HandleTcpClientAsync(TcpClient client)
        {
            Console.WriteLine("Service gateway connected on port: " + ((IPEndPoint)listener.LocalEndpoint).Port);

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
            Console.WriteLine("DT Received message: " + message);
            var tempMessage = JsonConvert.DeserializeObject<MessageModel>(message);
            string mes = "";
            if(building != null)
            {
                mes = await building.AccessDatabaseAsync(tempMessage.MeterID, tempMessage.Date);
            }
            else if(precinct != null)
            {

            }
            else if(campus != null)
            {

            }
            
            return mes;
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

        /*private static void AcceptCallback(IAsyncResult AR)
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
