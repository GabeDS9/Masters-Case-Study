using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class ClientSocket
{
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
                Debug.Log("Connected to Gateway on port " + port);

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
                Debug.Log($"Connecting attempts to {port}: " + attempts.ToString());
            }
        }

        //Console.WriteLine("Connected to server");
    }
}
