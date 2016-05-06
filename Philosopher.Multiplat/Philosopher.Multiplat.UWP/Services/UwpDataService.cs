using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Security.Cryptography.Certificates;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Newtonsoft.Json;
using Philosopher.Multiplat.Models;
using Philosopher.Multiplat.Services;

namespace Philosopher.Multiplat.UWP.Services
{
    public class UwpDataService : IDataService
    {        
        private const string GET_SCRIPT_ENDPOINT = "/scr";
        private const string CALL_SCRIPT_ENDPOINT = "/scr/{0}";

        private HttpBaseProtocolFilter _allowSelfSignedCertFilter;

        private string _baseUrl;
        public string BaseUrl
        {
            get { return _baseUrl; }
            set
            {
                if (_baseUrl != value)
                {
                    _baseUrl = value;
                    OnPropertyChanged();
                }
            }
        }

        private HttpClient _client;

        public IDataService Create()
        {
            return Create("http://localhost", 3000);
        }

        public IDataService Create(string hostname, uint portNumber)
        {
            BaseUrl = $"{hostname}:{portNumber}";
            _allowSelfSignedCertFilter = new HttpBaseProtocolFilter();
            _allowSelfSignedCertFilter.AllowUI = false;
            _allowSelfSignedCertFilter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
            _allowSelfSignedCertFilter.IgnorableServerCertificateErrors.Add(ChainValidationResult.InvalidName);

            _client = new HttpClient(_allowSelfSignedCertFilter);
            return this;
        }

        public void ChangeHostName(string hostname, uint portNumber = 3000)
        {
            BaseUrl = $"{hostname}:{portNumber}";
        }

        public void Login(string user, string pass)
        {
            PasswordCredential credential = new PasswordCredential
            {
                Password = pass,
                UserName = user
            };
            _allowSelfSignedCertFilter.ServerCredential = credential;
            _client = new HttpClient(_allowSelfSignedCertFilter);
        }

        public async Task<string> CallServerScript(ServerScript script, CancellationToken token)
        {
            string uriString = BaseUrl + String.Format(CALL_SCRIPT_ENDPOINT, script.Name);
            Uri callScriptUri = new Uri(uriString);
            try
            {
                HttpResponseMessage response = await _client.GetAsync(callScriptUri).AsTask(token);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return responseBody;
                }
                else
                {
                    return "";
                }
            }
            catch (COMException ex)
            {
                System.Diagnostics.Debug.WriteLine("CallScripts failed because: " + ex.ToString());
                return ex.ToString();
            }
        }                

        public async Task<ResultOrErrorResponse<List<ServerScript>>>  GetScripts(CancellationToken token)
        {
            Uri getScriptUri = new Uri($"{BaseUrl}{GET_SCRIPT_ENDPOINT}");
            try
            {
                HttpResponseMessage response = await _client.GetAsync(getScriptUri).AsTask(token);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    List<ServerScript> scripts = JsonConvert.DeserializeObject<List<ServerScript>>(responseBody);
                    return new ResultOrErrorResponse<List<ServerScript>>(scripts);
                }
                else
                {
                    string responseString = response.Headers?.WwwAuthenticate?.FirstOrDefault().Parameters?.FirstOrDefault().Value ?? "nothing";
                    return new ResultOrErrorResponse<List<ServerScript>>(new GenericHttpResponse
                    {
                        HttpStatusCode = (int)response.StatusCode,
                        ResponseMessage = responseString
                    });
                }
            }
            catch (Exception ex) when (ex is COMException || ex is WebException)
            {
                System.Diagnostics.Debug.WriteLine("GetScripts failed because: " + ex.ToString());
                return new ResultOrErrorResponse<List<ServerScript>>(new GenericHttpResponse {
                    ResponseMessage = "Network request failed. Details: " + ex.ToString(),
                    HttpStatusCode = 404
                });
            }
        }                

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}