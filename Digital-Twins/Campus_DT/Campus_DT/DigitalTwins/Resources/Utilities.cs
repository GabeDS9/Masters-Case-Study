using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class Utilities
    {
        public string DecodeTimestamp(string timestamp, string type)
        {
            string newTime = "";

            if (type == "Day")
            {
                String[] temp = timestamp.Split('-');

                String yr = temp[0];
                String mon = temp[1];
                String dy = temp[2];

                if (dy.Length >= 2)
                {
                    String[] daytemp = dy.Split(' ');
                    dy = daytemp[0];
                    if (dy[0].Equals('0'))
                    {
                        dy = dy[1].ToString();
                    }
                }
                if (mon[0].Equals('0'))
                {
                    mon = mon[1].ToString();
                }


                newTime = yr + "-" + mon + "-" + dy;
            }
            else if (type == "Month")
            {
                String[] temp = timestamp.Split('-');

                String yr = temp[0];
                String mon = temp[1];

                if (mon[0].Equals('0'))
                {
                    mon = mon[1].ToString();
                }


                newTime = yr + "-" + mon;
            }
            else if (type == "Year")
            {
                String[] temp = timestamp.Split('-');

                String yr = temp[0];

                newTime = yr;
            }

            return newTime;
        }
        public List<string> GenerateDateList(string startDate, string endDate, string type)
        {
            List<string> dateList = new List<string>();            
            DateTime start = DateTime.ParseExact(startDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            DateTime end = DateTime.ParseExact(endDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

            List<DateTime> allDates = new List<DateTime>();
            for (DateTime date = start; date <= end; date = date.AddDays(1))
            {
                allDates.Add(date);
            }
            int prevDay = allDates[0].Day;
            int prevMonth = allDates[0].Month;
            int prevYear = allDates[0].Year;

            if (type == "Day")
            {
                foreach (var date in allDates)
                {
                    dateList.Add(DecodeTimestamp(date.ToString("yyyy-MM-dd"), type));
                }
            }
            else if (type == "Month")
            {
                foreach (var date in allDates)
                {
                    if (date.Month == prevMonth)
                    {
                        prevMonth++;
                        dateList.Add(DecodeTimestamp(date.ToString("yyyy-MM-dd"), type));
                    }
                }
            }
            else if (type == "Year")
            {
                foreach (var date in allDates)
                {
                    if (date.Year == prevYear)
                    {
                        prevYear++;
                        dateList.Add(DecodeTimestamp(date.ToString("yyyy-MM-dd"), type));
                    }
                }
            }

            return dateList;
        }
    
        public string ChangeDateFormat(string date)
        {
            string changedDate = "";
            String[] temp = date.Split('-');

            if (temp.Length > 2)
            {
                String yr = temp[0];
                String mon = temp[1];
                String dy = temp[2];

                if (dy.Length > 2)
                {
                    String[] daytemp = dy.Split(' ');
                    dy = daytemp[0];

                }

                if (dy.Length == 1)
                {
                    dy = "0" + dy[0].ToString();
                }
                else if ((dy.Length == 2) && (dy[0].Equals('0')))
                {
                    dy = dy[1].ToString();
                }

                if (mon.Length == 1)
                {
                    mon = "0" + mon[0].ToString();
                }
                else if ((mon.Length == 2) && (mon[0].Equals('0')))
                {
                    mon = mon[1].ToString();
                }

                changedDate = yr + "-" + mon + "-" + dy;
            }
            else if (temp.Length > 1)
            {
                String yr = temp[0];
                String mon = temp[1];

                if (mon.Length == 1)
                {
                    mon = "0" + mon[0].ToString();
                }
                else if ((mon.Length == 2) && (mon[0].Equals('0')))
                {
                    mon = mon[1].ToString();
                }

                changedDate = yr + "-" + mon;
            }
            else if (temp.Length == 1)
            {
                String yr = temp[0];
                changedDate = yr;
            }

                return changedDate;
        }
    }
}
