﻿using System;
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
        private Campus campus = null;

        public void SetupServer(int port, Building build, Precinct prec, Campus camp)
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
            client.GetStream().Flush();
        }
        public async Task<string> MessageHandlerAsync(string message)
        {
            var tempMessage = JsonConvert.DeserializeObject<MessageModel>(message);
            string mes = "";
            if(building != null)
            {
                mes = await building.ServiceHandlerAsync(tempMessage);
            }
            else if(precinct != null)
            {
                mes = await precinct.ServiceHandlerAsync(tempMessage);
            }
            else if(campus != null)
            {
                mes = await campus.ServiceHandlerAsync(tempMessage);
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
    }
}