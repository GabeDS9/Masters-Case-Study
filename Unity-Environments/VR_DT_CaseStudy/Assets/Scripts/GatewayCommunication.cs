using Newtonsoft.Json;
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

        // Checking Stellenbosch
        var mes1 = new MessageModel
        {
            DataType = "Energy",
            InformationType = "CurrentData",
            DisplayType = "Collective",
            DigitalTwin = "Stellenbosch",
            LowestDTLevel = "All",
            startDate = "2022-5-5",
            endDate = "2022-5-5",
            timePeriod = "Day"
        };
        var mes = JsonConvert.SerializeObject(mes1);
        Debug.Log($"Sending Message: {mes}");
        string text = Task.Run(() => myClient.sendMessageAsync(mes, 9000)).Result;
        textBox.text = text;

        /*var mes1 = new MessageModel
        {
            DataType = "Energy",
            InformationType = "Averages",
            DisplayType = "Individual",
            DigitalTwin = "Stellenbosch",
            LowestDTLevel = "All",
            startDate = "2022-5-5",
            endDate = "2022-5-5",
            timePeriod = "Day"
        };
        var mes = JsonConvert.SerializeObject(mes1);
        var text = Task.Run(() => myClient.sendMessageAsync(mes, 9000)).Result;
        textBox.text = text;*/

        /*var mes1 = new MessageModel
        {
            DataType = "Energy",
            InformationType = "CurrentData",
            DisplayType = "Individual",
            DigitalTwin = "Stellenbosch",
            LowestDTLevel = "Building",
            startDate = "2022-5-5",
            endDate = "2022-5-5",
            timePeriod = "Day"
        };
        var mes = JsonConvert.SerializeObject(mes1);
        var text = Task.Run(() => myClient.sendMessageAsync(mes, 9000)).Result;
        textBox.text = text;*/

        /*var mes1 = new MessageModel
        {
            DataType = "Energy",
            InformationType = "Averages",
            DisplayType = "Collective",
            DigitalTwin = "Stellenbosch",
            LowestDTLevel = "Building",
            startDate = "2022-5-5",
            endDate = "2022-5-5",
            timePeriod = "Day"
        };
        var mes = JsonConvert.SerializeObject(mes1);
        var text = Task.Run(() => myClient.sendMessageAsync(mes, 9000)).Result;
        textBox.text = text;*/

        // Checking Coetzenburg
        /*var mes1 = new MessageModel
        {
            DataType = "Energy",
            InformationType = "CurrentData",
            DisplayType = "Collective",
            DigitalTwin = "Coetzenburg 11kV",
            LowestDTLevel = "All",
            startDate = "2022-5-5",
            endDate = "2022-5-5",
            timePeriod = "Day"
        };
        var mes = JsonConvert.SerializeObject(mes1);
        var text = Task.Run(() => myClient.sendMessageAsync(mes, 9000)).Result;
        textBox.text = text;*/

        /*var mes1 = new MessageModel
        {
            DataType = "Energy",
            InformationType = "Averages",
            DisplayType = "Individual",
            DigitalTwin = "Coetzenburg 11kV",
            LowestDTLevel = "None",
            startDate = "2022-5-5",
            endDate = "2022-5-5",
            timePeriod = "Day"
        };
        var mes = JsonConvert.SerializeObject(mes1);
        var text = Task.Run(() => myClient.sendMessageAsync(mes, 9000)).Result;
        textBox.text = text;*/

        /*var mes1 = new MessageModel
        {
            DataType = "Energy",
            InformationType = "CurrentData",
            DisplayType = "Individual",
            DigitalTwin = "Coetzenburg 11kV",
            LowestDTLevel = "Precinct",
            startDate = "2022-5-5",
            endDate = "2022-5-5",
            timePeriod = "Day"
        };
        var mes = JsonConvert.SerializeObject(mes1);
        var text = Task.Run(() => myClient.sendMessageAsync(mes, 9000)).Result;
        textBox.text = text;*/

        /*var mes1 = new MessageModel
        {
            DataType = "Energy",
            InformationType = "Averages",
            DisplayType = "Collective",
            DigitalTwin = "Coetzenburg 11kV",
            LowestDTLevel = "Precinct",
            startDate = "2022-5-5",
            endDate = "2022-5-5",
            timePeriod = "Day"
        };
        var mes = JsonConvert.SerializeObject(mes1);
        var text = Task.Run(() => myClient.sendMessageAsync(mes, 9000)).Result;
        textBox.text = text;*/

        // Checking Danie Craven
        /*var mes1 = new MessageModel
        {
            DataType = "Energy",
            InformationType = "CurrentData",
            DisplayType = "Collective",
            DigitalTwin = "Danie Craven Rugby Stadion",
            LowestDTLevel = "All",
            startDate = "2022-5-5",
            endDate = "2022-5-5",
            timePeriod = "Day"
        };
        var mes = JsonConvert.SerializeObject(mes1);
        var text = Task.Run(() => myClient.sendMessageAsync(mes, 9000)).Result;
        textBox.text = text;*/

        /*var mes1 = new MessageModel
        {
            DataType = "Energy",
            InformationType = "Averages",
            DisplayType = "Individual",
            DigitalTwin = "Danie Craven Rugby Stadion",
            LowestDTLevel = "All",
            startDate = "2022-5-5",
            endDate = "2022-5-5",
            timePeriod = "Day"
        };
        var mes = JsonConvert.SerializeObject(mes1);
        var text = Task.Run(() => myClient.sendMessageAsync(mes, 9000)).Result;
        textBox.text = text;*/

        /*var mes1 = new MessageModel
        {
            DataType = "Energy",
            InformationType = "CurrentData",
            DisplayType = "Individual",
            DigitalTwin = "Danie Craven Rugby Stadion",
            LowestDTLevel = "None",
            startDate = "2022-5-5",
            endDate = "2022-5-5",
            timePeriod = "Day"
        };
        var mes = JsonConvert.SerializeObject(mes1);
        var text = Task.Run(() => myClient.sendMessageAsync(mes, 9000)).Result;
        textBox.text = text;

        var mes1 = new MessageModel
        {
            DataType = "Energy",
            InformationType = "Averages",
            DisplayType = "Collective",
            DigitalTwin = "Danie Craven Rugby Stadion",
            LowestDTLevel = "None",
            startDate = "2022-5-5",
            endDate = "2022-5-5",
            timePeriod = "Day"
        };
        var mes = JsonConvert.SerializeObject(mes1);
        var text = Task.Run(() => myClient.sendMessageAsync(mes, 9000)).Result;
        textBox.text = text;*/
    }
}
