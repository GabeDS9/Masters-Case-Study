using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Building_DT
{
    class APICaller
    {
        // Makes a call to the energy API
        public Task<string> CallEnergyAPI(string url)
        {
            var task = GetMeterListResponse(url);
            if(task.Result != null)
            {
                return task;
            }
            else
            {
                return null;
            }
        }

        public static async Task<string> GetMeterListResponse(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                var response = (HttpWebResponse)await Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null);

                Stream stream = response.GetResponseStream();
                StreamReader strReader = new StreamReader(stream);
                string text = await strReader.ReadToEndAsync();

                return text;
            }
            catch (WebException ex)
            {
                string pageContent = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd().ToString();
                return pageContent;
            }
        }
    }
}
