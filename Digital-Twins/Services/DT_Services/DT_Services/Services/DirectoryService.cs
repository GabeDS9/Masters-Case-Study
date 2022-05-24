using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;
using Newtonsoft.Json;
using Resources;

namespace Services
{
    public class DirectoryService : ServiceBaseClass
    {
        private List<DigitalTwin> digitalTwinsList = new List<DigitalTwin>();
        private List<Service> servicesList = new List<Service>();
        private LoadExcel loadExcel = new LoadExcel();
        Communication.ClientSocket myClient = new Communication.ClientSocket();
        Communication.ServerSocket myServer = new Communication.ServerSocket();
        int servicePort = 0;
        public void InitialiseDirectoryServiceAsync()
        {
            digitalTwinsList = loadExcel.LoadDigitalTwins();
            var mesModel = new MessageModel { DataType = "DigitalTwins", MessageType = "ChildDTList" };
            string mes = JsonConvert.SerializeObject(mesModel);
            Task.Run(() => RequestDTChildrenAsync(mes));
            servicesList = loadExcel.LoadServices();
            foreach(var service in servicesList)
            {
                if(service.ServiceName == "Directory Service")
                {
                    servicePort = service.Port;
                    break;
                }
            }
            Console.WriteLine($"Setting up directory service on {servicePort}");
            StartServiceServer(servicePort);
        }
        private void StartServiceServer(int port)
        {
            myServer.SetupServer(port, null, this, null);
        }
        public async Task<string> MessageHandlerAsync(string mes)
        {
            string message = "";
            var tempMessage = JsonConvert.DeserializeObject<UIMessageModel>(mes);
            if(tempMessage.DataType == "List")
            {
                var response = await Task.Run(()=>ReturnDTs(tempMessage.DigitalTwin));
                message = JsonConvert.SerializeObject(response);
            }
            else if (tempMessage.DataType == "IP_Address")
            {
                message = await Task.Run(() => ReturnIPAddress(tempMessage.DigitalTwin));
            }
            else if (tempMessage.DataType == "Port")
            {
                var response = await Task.Run(() => ReturnPortNumber(tempMessage.DigitalTwin));
                message = JsonConvert.SerializeObject(response);
            }
            return message;
        }
        private async Task RequestDTChildrenAsync(string mes)
        {
            foreach (var dt in digitalTwinsList)
            {
                var tempData = await myClient.sendMessageAsync(mes, dt.IP_Address, dt.Port);
                var temp = JsonConvert.DeserializeObject<List<ChildDTModel>>(tempData);
                if (temp != null)
                {
                    var tempChildList = new List<string>();
                    foreach (var item in temp)
                    {
                        tempChildList.Add(item.Name);
                    }
                    dt.Child_DTs = tempChildList;
                }
            }
        }
        public int ReturnPortNumber(string dtName)
        {
            foreach (var dt in digitalTwinsList)
            {
                if (dt.DT_Name == dtName)
                {
                    return dt.Port;
                }
            }

            return 0;
        }
        public string ReturnIPAddress(string dtName)
        {
            foreach (var dt in digitalTwinsList)
            {
                if (dt.DT_Name == dtName)
                {
                    return dt.IP_Address;
                }
            }

            return "";
        }
        public List<string> ReturnDTs(string parentDT)
        {
            List<string> dtList = new List<string>();

            if (parentDT != "")
            {
                foreach (var dt in digitalTwinsList)
                {
                    if (dt.DT_Name == parentDT)
                    {
                        foreach (var childDT in dt.Child_DTs)
                        {
                            dtList.Add(childDT);
                        }
                    }
                }
            }
            else
            {
                dtList.Add(digitalTwinsList[0].DT_Name);
            }

            return dtList;

        }
    }
}
