using System.Collections.Generic;

    public class EnergyMeterData
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
        public double latest_power { get; set; }
        public string latest_timestamp { get; set; }
        public string previous_timestamp { get; set; }
        public List<EnergyData> data { get; set; }
        public List<EnergyAverageData> day_average { get; set; }
        public List<EnergyAverageData> month_average { get; set; }
}