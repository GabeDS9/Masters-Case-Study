using Mapbox.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

public class APICaller
{
    // This script will be used to access the various APIs for the utilities

    // General API Caller
    // This function will receive an API call and make that call to receive information from the API
    // Input: API URL
    // Output: API Response (string)
    public static String CallAPI(String url)
    {
        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

        Stream stream = response.GetResponseStream();
        StreamReader strReader = new StreamReader(stream);
        String text = strReader.ReadToEnd();

        return text;
    }

    // Gives current date and time for current meter readings
    public static (String, String) GetCurrentDateTime()
    {
        String to_date = "";
        String from_date = "";
        DateTime currDate = DateTime.Now;
        DateTime fromDate = currDate.AddMinutes(-15);

        to_date = currDate.Year + "-" + currDate.Month + "-" + currDate.Day + "%20" + currDate.Hour + ":" + currDate.Minute + ":00";
        from_date = fromDate.Year + "-" + fromDate.Month + "-" + fromDate.Day + "%20" + fromDate.Hour + ":" + fromDate.Minute + ":00";

        return (to_date, from_date);
    }

    // Water API Caller

    // Occupancy API Caller

    // Solar API Caller
}
