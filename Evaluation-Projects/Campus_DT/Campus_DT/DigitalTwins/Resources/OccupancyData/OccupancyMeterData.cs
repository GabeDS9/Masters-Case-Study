﻿using System.Collections.Generic;
    public class OccupancyMeterData
    {
        public int meterid { get; set; }
        public string description { get; set; }
        public string make { get; set; }
        public string manufacturer { get; set; }
        public string type { get; set; }
        public string serial_no { get; set; }
        public string model { get; set; }
        public string yard_no { get; set; }
        public string building_no { get; set; }
        public string floor { get; set; }
        public string room_no { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public List<OccupancyData> data { get; set; }
    }
