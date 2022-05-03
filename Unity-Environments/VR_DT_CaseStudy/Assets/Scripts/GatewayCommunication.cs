using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GatewayCommunication : MonoBehaviour
{
    private ClientSocket myClient = new ClientSocket();
    public Text textBox;
    public void ConnectToGateway()
    {
        Debug.Log("Connecting to gateway...");
        string text = Task.Run(() => myClient.sendMessageAsync("UI sending message to Gateway", 9000)).Result;
        textBox.text = text;
    }
}
