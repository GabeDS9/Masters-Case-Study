using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ClientSocket : MonoBehaviour
{
    public async Task<string> sendMessageAsync(string message, int port)
    {
        string response = "";
        try
        {
            var ipAdd = IPAddress.Parse("146.232.146.140");
            TcpClient client = new TcpClient(); // Create a new connection
            //await client.ConnectAsync(IPAddress.Loopback, port);
            await client.ConnectAsync(ipAdd, port);
            client.NoDelay = true; // please check TcpClient for more optimization
                                   // messageToByteArray- discussed later
            byte[] messageBytes = messageToByteArray(message);

            using (NetworkStream stream = client.GetStream())
            {
                stream.Write(messageBytes, 0, messageBytes.Length);

                // Message sent!  Wait for the response stream of bytes...
                // streamToMessage - discussed later
                response = await streamToMessage(stream, client);
                //var tempMessage = JsonConvert.DeserializeObject<DataAccess.Models.EnergyMeterModel>(response);
                //Debug.Log("Message received from Gateway: " + response);
            }
            client.Close();
        }
        catch (Exception e) { Debug.Log(e.Message); }
        return response;
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

    private async Task<string> streamToMessage(NetworkStream stream, TcpClient client)
    {
        string result = null;
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
                result += c;
                /*if (c != '\0')
                {
                    result += c;
                }
                else
                {
                    Debug.Log(c);
                }*/
            }
        }
        while (stream.DataAvailable);
        //Debug.Log(result);
        return result;
    }
}
