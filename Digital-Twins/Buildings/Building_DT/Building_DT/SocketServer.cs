using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

// Socket Listener acts as a server and listens to the incoming
// messages on the specified port and protocol.
public class SocketServer
{
    Socket handler = null;

    public void StartServer()
    {
        // Get Host IP Address that is used to establish a connection
        // In this case, we get one IP address of localhost that is IP : 127.0.0.1
        // If a host has multiple addresses, you will get a list of addresses
        IPHostEntry host = Dns.GetHostEntry("localhost");
        IPAddress ipAddress = host.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

        try
        {

            // Create a Socket that will use Tcp protocol
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            // A Socket must be associated with an endpoint using the Bind method
            listener.Bind(localEndPoint);
            // Specify how many requests a Socket can listen before it gives Server busy response.
            // We will listen 10 requests at a time
            listener.Listen(10);

            Console.WriteLine("Waiting for a connection...");
            handler = listener.Accept();

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public void SendMessage(string data)
    {
        byte[] msg = Encoding.ASCII.GetBytes(data);
        handler.Send(msg);
    }

    public string ReceiveMessage()
    {
        // Incoming data from the client.
        string data = null;
        byte[] bytes = null;

        while (true)
        {
            bytes = new byte[1024];
            int bytesRec = handler.Receive(bytes);
            data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
            if (data.IndexOf("Send meter list") > -1)
            {
                return "MeterList requested";
            }
            if(data.IndexOf("Next") > -1)
            {
                return "Next Meter";
            }
        }
    }

    public void CloseServer()
    {
        handler.Shutdown(SocketShutdown.Both);
        handler.Close();
    }
}
