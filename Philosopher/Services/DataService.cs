using Philosopher.Models;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Web.Http;
using Newtonsoft.Json;
using Windows.Web.Http.Filters;

namespace Philosopher.Services
{
    public class DataService
    {
        private const string GET_SCRIPT_ENDPOINT = "/scr";
        private const string CALL_SCRIPT_ENDPOINT = "/scr/{0}";
        
        public string BaseUrl { get; private set; }
        private int _portNumber;
        private HttpClient _client;

        public DataService() : this("http://localhost", 3000) { }

        public DataService(string hostname, int portNumber)
        {            
            _portNumber = portNumber;
            BaseUrl = $"{hostname}:{_portNumber}";

            var rootFilter = new HttpBaseProtocolFilter();
            rootFilter.AllowUI = true;
            rootFilter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;            

            _client = new HttpClient(rootFilter);
        }

        //Can return null.
        public async Task<List<ServerScript>> GetScripts(CancellationToken token)
        {
            Uri getScriptUri = new Uri($"{BaseUrl}{GET_SCRIPT_ENDPOINT}");
            try
            {
                HttpResponseMessage response = await _client.GetAsync(getScriptUri).AsTask(token);
                if(response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync().AsTask(token);
                    List<ServerScript> scripts = JsonConvert.DeserializeObject<List<ServerScript>>(responseBody);
                    return scripts;
                }
                else
                {
                    return null;
                }
            }
            catch(COMException ex)
            {
                System.Diagnostics.Debug.WriteLine("GetScripts failed because: " + ex.ToString());
                return null;
            }
        }

        public async Task<string> CallServerScript(ServerScript script, CancellationToken token)
        {
            string uriString = BaseUrl + String.Format(CALL_SCRIPT_ENDPOINT, script.Name);
            Uri callScriptUri = new Uri(uriString);
            try
            {
                HttpResponseMessage response = await _client.GetAsync(callScriptUri).AsTask(token);
                if(response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync().AsTask(token);
                    return responseBody;                    
                }
                else
                {
                    return "";
                }
            }
            catch(COMException ex)
            {
                System.Diagnostics.Debug.WriteLine("CallScripts failed because: " + ex.ToString());
                return ex.ToString();
            }
        }
    }
}
