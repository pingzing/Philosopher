﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Philosopher.Multiplat.Models;

namespace Philosopher.Multiplat.Services
{
    public class DataService
    {
        private const string GET_SCRIPT_ENDPOINT = "/scr";
        private const string CALL_SCRIPT_ENDPOINT = "/scr/{0}";
        
        public string BaseUrl { get; }
        private readonly HttpClient _client;

        public DataService() : this("http://localhost", 3000) { }

        public DataService(string hostname, int portNumber)
        {
            BaseUrl = $"{hostname}:{portNumber}";
            _client = new HttpClient();
        }

        //Can return null.
        public async Task<List<ServerScript>> GetScripts(CancellationToken token)
        {
            Uri getScriptUri = new Uri($"{BaseUrl}{GET_SCRIPT_ENDPOINT}");
            try
            {
                HttpResponseMessage response = await _client.GetAsync(getScriptUri, token);
                if(response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
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
                HttpResponseMessage response = await _client.GetAsync(callScriptUri, token);
                if(response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
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
