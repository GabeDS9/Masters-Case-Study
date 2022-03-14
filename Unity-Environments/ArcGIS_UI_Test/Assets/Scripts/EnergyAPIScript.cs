using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using Mapbox.Json;

public static class EnergyAPIScript
{

    public static List<MeterList> GetMeterList()
    {
        //Debug.Log("Getting Meter List");
        String apikey = "68b408399bdcbf3d5d4b3485c76596e8015c9f797414a83e3aa626d04d070abe"; //"[YOUR API KEY HERE]";
        String url = "https://api.indivo.co.za/Energy/MeterList?key=" + apikey;

        String task = GetMeterListResponse(url);
        List<MeterList> meterlist = JsonConvert.DeserializeObject<List<MeterList>>(task);
        int pos = 0;
        foreach(var record in meterlist)
        {
            if (record.meterid == 8656)
            {
                Debug.Log("Meter " + record.meterid + " removed");
                pos = meterlist.IndexOf(record);
            }
            else
            {
                record.longitude = (UnityEngine.Random.Range(18.8600f, 18.8700f)).ToString().Replace(',', '.');
                record.latitude = (UnityEngine.Random.Range(-33.9400f, -33.9500f)).ToString().Replace(',', '.');
            }           

            //Debug.Log(record.meterid + " new coordinates - Lat: " + record.latitude + " - Long: " + record.longitude);
        }

        meterlist.RemoveAt(pos);

        return meterlist;
    }

    public static MeterData GetMeterData(String from_date, String to_date, String meterid, String interval)
    {
        Debug.Log("Getting Meter Data");
        String apikey = "68b408399bdcbf3d5d4b3485c76596e8015c9f797414a83e3aa626d04d070abe"; //"[YOUR API KEY HERE]";

        String url = $"https://api.indivo.co.za/Energy/EnergyData?id={meterid}&from_date={from_date}&to_date={to_date}&interval={interval}&key={apikey}";

        String task = GetMeterListResponse(url);
        MeterData meterdata = JsonConvert.DeserializeObject<MeterData>(task);
        List<Data> data = meterdata.data;

        return meterdata;
    }

    public static String GetMeterListResponse(string url)
    {
        //Debug.Log("Getting meter response");
        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

        Stream stream = response.GetResponseStream();
        StreamReader strReader = new StreamReader(stream);
        string text = strReader.ReadToEnd();

        //Debug.Log(text);

        return text;
    }
}
