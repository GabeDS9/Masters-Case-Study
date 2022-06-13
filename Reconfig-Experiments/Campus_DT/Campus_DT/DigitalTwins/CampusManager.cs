using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DataAccess;
using DataAccess.Models;

namespace Campus_DT
{
    class CampusManager
    {
        /*private string startingDate = "2022-05-12 00:00:00";
        private Campus campus;
        private Stopwatch stopWatch = new Stopwatch();
        Services_Communication.ClientSocket myClient = new Services_Communication.ClientSocket();
        
        public void InitialiseCampus()
        {
            LoadExcel excel = new LoadExcel();
            string Campus_name = "Stellenbosch University";

            var camp = excel.LoadCampusData(startingDate);

            campus = camp[0];

            stopWatch.Start();

            while (true)
            {
                double ts = stopWatch.Elapsed.TotalSeconds;
                if (ts >= 60)
                {
                    campus.GetUpdatedData();
                    stopWatch.Restart();
                }
            }            
        }*/
    }
}
