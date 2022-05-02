using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace Services_Communication
{
    class ClientSocket
    {
        private Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public int LoopConnect()
        {
            int attempts = 0;

            while (!clientSocket.Connected)
            {
                try
                {
                    attempts++;

                    clientSocket.Connect(IPAddress.Loopback, 100);

                    Console.WriteLine("Client: My local IpAddress is :" + IPAddress.Parse(((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString()) +
                    "I am connected on port number " + ((IPEndPoint)clientSocket.RemoteEndPoint).Port.ToString());
                }
                catch (SocketException)
                {
                    Console.WriteLine("Connecting attempts: " + attempts.ToString());
                }
            }

            return ((IPEndPoint)clientSocket.LocalEndPoint).Port;
        }

        public void SendMessageToServer(string name)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(name);
            clientSocket.Send(buffer);

            byte[] receivedBuf = new byte[1024];
            int rec = clientSocket.Receive(receivedBuf);
            byte[] data = new byte[rec];
            Array.Copy(receivedBuf, data, rec);
            string message = Encoding.ASCII.GetString(data);

            Console.WriteLine("Received: " + message + " for " + name);
        }

        /*public void ExecuteClient(string message)
        {
            ConnectToGateway();
            SendMessageToGateway(message);
            CloseClient();
        }
        private void ConnectToGateway()
        {
            try
            {
                // Establish the remote endpoint
                // for the socket. This example
                // uses port 11111 on the local
                // computer.
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddr = ipHost.AddressList[0];
                IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11111);

                // Creation TCP/IP Socket using
                // Socket Class Constructor
                sender = new Socket(ipAddr.AddressFamily,
                           SocketType.Stream, ProtocolType.Tcp);
                // Connect Socket to the remote
                // endpoint using method Connect()
                sender.Connect(localEndPoint);

                // We print EndPoint information
                // that we are connected
                Console.WriteLine("Socket connected to -> {0} ",
                              sender.RemoteEndPoint.ToString());
            }
            catch (ArgumentNullException ae)
            {
                Console.WriteLine("ArgumentNullException : {0}", ae.ToString());
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

        // ExecuteClient() Method
        private void SendMessageToGateway(String name)
        {
            // Creation of message that
            // we will send to Server
            byte[] messageSent = Encoding.ASCII.GetBytes(name + "<EOF>");
            int byteSent = sender.Send(messageSent);

            Console.WriteLine("Message sent: " + name);

            // Data buffer
            /*byte[] messageReceived = new byte[1024];

            // We receive the message using
            // the method Receive(). This
            // method returns number of bytes
            // received, that we'll use to
            // convert them to string
            int byteRecv = sender.Receive(messageReceived);
            Console.WriteLine("Message from Server -> {0}",
                  Encoding.ASCII.GetString(messageReceived,
                                             0, byteRecv)); 
        }

        private void CloseClient()
        {
            // Close Socket using
            // the method Close()
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }*/
    }
}
