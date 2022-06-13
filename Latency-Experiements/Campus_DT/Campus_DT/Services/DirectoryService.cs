using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;
using Newtonsoft.Json;
using DataAccess.Models;

namespace Services
{
    public class DirectoryService
    {
        private List<DigitalTwin> digitalTwinsList = new List<DigitalTwin>();
        private LoadExcel loadExcel = new LoadExcel();
        Communication.ClientSocket myClient = new Communication.ClientSocket();
        public async Task InitialiseDirectoryServiceAsync()
        {
            digitalTwinsList = loadExcel.LoadDigitalTwins();
            var mesModel = new MessageModel { DataType = "DigitalTwins", MessageType = "ChildDTList" };
            string mes = JsonConvert.SerializeObject(mesModel);

            foreach (var dt in digitalTwinsList)
            {
                var tempData = await myClient.sendMessageAsync(mes, dt.Port);
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
