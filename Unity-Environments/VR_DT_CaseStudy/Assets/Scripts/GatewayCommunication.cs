using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GatewayCommunication : MonoBehaviour
{
    private ClientSocket myClient = new ClientSocket();
    public void ConnectToGateway()
    {
        Debug.Log("Connecting to gateway...");
        _ = myClient.LoopConnectAsync(9000);
    }
}
