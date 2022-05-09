using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Client app is the one sending messages to a Server/listener.
// Both listener and client can send messages back and forth once a
// communication is established.
public class SocketClient
{
    private Socket sender;
    private byte[] bytes = new byte[1024];
    public void StartClient()
    {       
        try
        {
            // Connect to a Remote server
            // Get Host IP Address that is used to establish a connection
            // In this case, we get one IP address of localhost that is IP : 127.0.0.1
            // If a host has multiple addresses, you will get a list of addresses
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP  socket.
            sender = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect the socket to the remote endpoint. Catch any errors.
            try
            {
                Debug.Log("Starting client");
                // Connect to Remote EndPoint
                sender.Connect(remoteEP);

                Debug.Log("Socket connected to " + sender.RemoteEndPoint.ToString());

            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public void SendMessage(string message)
    {

        // Encode the data string into a byte array.
        byte[] msg = Encoding.ASCII.GetBytes(message + "<EOF>");

        // Send the data through the socket.
        int bytesSent = sender.Send(msg);
    }

    public string ReceiveMessage()
    {
        int bytesRec = 0;
        string message = "";
        while (true)
        {
            // Receive the response from the remote device.
            bytesRec = sender.Receive(bytes);
            message = Encoding.ASCII.GetString(bytes, 0, bytesRec);

            if(message != null)
            {
                return message;
            }
        }
    }

    public void CloseClient()
    {
        // Release the socket.
        sender.Shutdown(SocketShutdown.Both);
        sender.Close();
    }
}
