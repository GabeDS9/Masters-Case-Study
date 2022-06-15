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
using Services;
using Resources;

namespace Communication
{
    class ServerSocket
    {
        private TcpListener server = null;
        ServiceGateway myGateway = null;
        DirectoryService myDirectory = null;
        ExploratoryAnalyticsService myExplore = null;
        EnergyCostService myEnergyCost = null;

        public void SetupServer(int port, ServiceGateway gateway, DirectoryService directory, ExploratoryAnalyticsService explore, EnergyCostService cost)
        {
            myGateway = gateway;
            myDirectory = directory;
            myExplore = explore;
            myEnergyCost = cost;
            server = new TcpListener(IPAddress.Any, port);
            server.Start();            
            CancellationTokenSource cancellationTokenSource = new();

            //_ = Task.Run(async () => {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    var tcpClient = server.AcceptTcpClientAsync().Result;
                    _ = HandleTcpClientAsync(tcpClient);
                }
            //});
        }

        private async Task HandleTcpClientAsync(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            string request = await StreamToMessage(stream, client);
            
            if (request != null)
            {
                string responseMessage = await MessageHandlerAsync(request);
                SendMessage(responseMessage, client);
            }
        }

        private static void SendMessage(string message, TcpClient client)
        {
            // messageToByteArray- discussed later
            byte[] bytes = MessageToByteArray(message);
            client.GetStream().Write(bytes, 0, bytes.Length);
            client.GetStream().Flush();
        }

        public async Task<string> MessageHandlerAsync(string mes)
        {
            string message = "";
            if(myGateway != null)
            {
                Console.WriteLine("UI requeesting information on port");
                message = await myGateway.MessageHandlerAsync(mes);                
            }
            else if(myDirectory != null)
            {
                message = await myDirectory.MessageHandlerAsync(mes);
            }
            else if(myExplore != null)
            {
                message = await myExplore.MessageHandlerAsync(mes);
            }
            else if (myEnergyCost != null)
            {
                message = await myEnergyCost.MessageHandlerAsync(mes);
            }
            return message;
        }

        // using UTF8 encoding for the messages
        static readonly Encoding encoding = Encoding.UTF8;
        private static byte[] MessageToByteArray(string message)
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

        private async Task<string> StreamToMessage(NetworkStream stream, TcpClient client)
        {
            string result = null;
            StringBuilder sb = new StringBuilder();
            do
            {
                //Debug.Log(stream.DataAvailable);
                // size bytes have been fixed to 4
                byte[] sizeBytes = new byte[4];
                // read the content length
                await stream.ReadAsync(sizeBytes, 0, 4);
                int messageSize = BitConverter.ToInt32(sizeBytes, 0);
                // create a buffer of the content length size and read from the stream
                byte[] messageBytes = new byte[messageSize];
                await stream.ReadAsync(messageBytes, 0, messageSize);
                // convert message byte array to the message string using the encoding
                string message = encoding.GetString(messageBytes);
                foreach (var c in message)
                {
                    sb.Append(c);
                }
            }
            while (stream.DataAvailable);
            result = sb.ToString();
            return result;
        }
    }
}
