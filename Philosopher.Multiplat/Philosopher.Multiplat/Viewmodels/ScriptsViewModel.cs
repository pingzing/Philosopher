using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Philosopher.Multiplat.Helpers;
using Philosopher.Multiplat.Models;
using Philosopher.Multiplat.Pages;
using Philosopher.Multiplat.Services;
using Xamarin.Forms;

namespace Philosopher.Multiplat.Viewmodels
{
    public class ScriptsViewModel : ViewModelBase, INavigable
    {
        private bool _isResponseBoxExpanded = false;
        private bool _firstLoad = true;
        private readonly IAuthService _authService;

        public static int ResponseShrunkHeight => Device.OS == TargetPlatform.WinPhone 
                                                            ? 25 
                                                            : 30;

        private IDataService _dataService;
        public IDataService DataService
        {
            get { return _dataService; }
            set
            {
                if (_dataService != value)
                {
                    _dataService = value;
                    RaisePropertyChanged(nameof(DataService));
                }
            }
        }

        private ObservableCollection<ServerScript> _scriptList = new ObservableCollection<ServerScript>();
        public ObservableCollection<ServerScript> ScriptList
        {
            get { return _scriptList; }
            set
            {
                if (value != _scriptList)
                {
                    _scriptList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _mostRecentServerResponse;
        public string MostRecentServerResponse
        {
            get { return _mostRecentServerResponse; }
            set
            {
                if (_mostRecentServerResponse != value)
                {
                    _mostRecentServerResponse = value;
                    RaisePropertyChanged(nameof(MostRecentServerResponse));
                }
            }
        }

        private GridLength _serverResponseBlockHeight = 25;
        public GridLength ServerResponseBlockHeight
        {
            get { return _serverResponseBlockHeight; }
            set
            {
                _serverResponseBlockHeight = value;                
                RaisePropertyChanged(nameof(ServerResponseBlockHeight));
            }
        }

        private GridLength _scriptButtonsRowHeight = new GridLength(1, GridUnitType.Star);
        public GridLength ScriptButtonsRowHeight
        {
            get { return _scriptButtonsRowHeight; }
            set
            {
                _scriptButtonsRowHeight = value;
                RaisePropertyChanged(nameof(ScriptButtonsRowHeight));
            }
        }

        private double _responseLabelHeight = ResponseShrunkHeight;
        public double ResponseLabelHeight
        {
            get { return _responseLabelHeight; }
            set
            {
                if (_responseLabelHeight != value)
                {
                    _responseLabelHeight = value;
                    RaisePropertyChanged(nameof(ResponseLabelHeight));
                    RaisePropertyChanged(nameof(IsResponseLabelTooTall));
                }
            }
        }

        public bool IsResponseLabelTooTall
        {
            get
            {
                if (Device.OS == TargetPlatform.WinPhone)
                {
                    return ResponseLabelHeight > 28;
                }
                else
                {
                    return ResponseLabelHeight > 62;
                }
            }
        }

        private RelayCommand _serverButtonClickedCommand;
        public RelayCommand ServerButtonClickedCommand => 
            _serverButtonClickedCommand 
            ?? (_serverButtonClickedCommand = new RelayCommand(async () => await ServerButtonClicked()));

        private RelayCommand<ServerScriptVm> _serverScriptClickedCommand;
        public RelayCommand<ServerScriptVm> ServerScriptClickedCommand =>
            _serverScriptClickedCommand
            ?? (_serverScriptClickedCommand = new RelayCommand<ServerScriptVm>(async scriptVm => await ServerScriptClicked(scriptVm)));

        private RelayCommand _expandHideButtonCommand;
        public RelayCommand ExpandHideButtonCommand =>
            _expandHideButtonCommand
            ?? (_expandHideButtonCommand = new RelayCommand(ExpandHideButton));

        private RelayCommand _refreshCommand;

        public RelayCommand RefreshCommand =>
            _refreshCommand
            ?? (_refreshCommand = new RelayCommand(RefreshScripts));        

        public ScriptsViewModel(IDataService dataService, IAuthService authService)
        {
            DataService = dataService.Create(Settings.HostnameSetting, (uint)Settings.PortSetting);
            _authService = authService;
            var accounts = _authService.FindAccountsForService(Constants.APP_SERVICE_ID).ToList();
            if (accounts.Any())
            {
                if (accounts.Count == 1)
                {
                    Account acct = accounts.First();
                    DataService.Login(acct.Username, acct.Password);
                }
                //else pop up UI to allow user to choose saved login or something
            }
        }


        public bool IsListVisible => !_isResponseBoxExpanded;

        private async Task UpdateScripts()
        {            
            try
            {
                using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
                {
                    while (!cts.IsCancellationRequested)
                    {
                        ResultOrErrorResponse<List<ServerScript>> result = await DataService.GetScripts(cts.Token);
                        if (result.IsResult)
                        {
                            try
                            {
                                List<ServerScript> scripts = result.Result;
                                if (scripts == null)
                                {
                                    await ServerButtonClicked();
                                    return;
                                }
                                List<ServerScriptVm> bindableScripts =
                                    scripts.Select(x => new ServerScriptVm(x)).ToList();
                                ScriptList = new ObservableCollection<ServerScript>(bindableScripts);
                                return;
                            }

                            catch (Exception ex)
                            {
                                await ServerButtonClicked();
                                return;
                            }
                        }
                        else if ((HttpStatusCode)result.ErrorResponse.HttpStatusCode == HttpStatusCode.Unauthorized)
                        {
                            var dialogResult = await UserDialogs.Instance.LoginAsync("Authorization required",
                                $"Access denied. The server says \"{result.ErrorResponse.ResponseMessage}\"");
                            if (dialogResult != null
                                && dialogResult.Ok
                                && !String.IsNullOrWhiteSpace(dialogResult.LoginText)
                                && !String.IsNullOrWhiteSpace(dialogResult.Password))
                            {
                                DataService.Login(dialogResult.LoginText, dialogResult.Password);
                                Account acct = new Account { Username = dialogResult.LoginText, Password = dialogResult.Password };
                                _authService.Save(acct, Constants.APP_SERVICE_ID);
                                //loop and try again
                            }
                            else if (dialogResult == null || dialogResult.Ok == false)
                            {
                                cts.Cancel();
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateScripts timed out. Details:\n" + ex.ToString());
            }
            finally
            {
                //isbusy = false
            }
        }

        private async Task ServerButtonClicked()
        {            
            PromptConfig hostnameConfig = new PromptConfig
            {
                Title = "Enter server info",
                Message = "Enter the server's hostname and (optionally) port number.",
                Placeholder = "Hostname"
            };
            PromptResult result = await UserDialogs.Instance.PromptAsync(hostnameConfig);
            if (result != null && result.Ok && !String.IsNullOrWhiteSpace(result.Text))
            {
                string text = result.Text;
                HostnamePrefix prefix;
                if (text.Contains("http://"))
                {
                    prefix = HostnamePrefix.Http;
                }
                else if (text.Contains("https://"))
                {
                    prefix = HostnamePrefix.Https;
                }
                else
                {
                    prefix = HostnamePrefix.None;
                }
                switch (prefix)
                {
                    case HostnamePrefix.Http:
                        text = text.Replace("http://", "");
                        break;
                    case HostnamePrefix.Https:
                        text = text.Replace("https://", "");
                        break;
                    case HostnamePrefix.None:
                    default:
                        break;
                }

                string[] inputs = text.Split(':');
                string hostname = inputs.FirstOrDefault();
                if (hostname == null)
                {
                    await UserDialogs.Instance.AlertAsync("You must enter a hostname.", "Invalid hostname");
                    return;
                }
                switch (prefix)
                {
                    case HostnamePrefix.Http:
                        hostname = $"http://{hostname}";
                        break;
                    case HostnamePrefix.Https:
                        hostname = $"https://{hostname}";
                        break;
                    case HostnamePrefix.None:
                    default:
                        break;
                }

                bool maybeHasPort = inputs.Skip(1).Take(1).FirstOrDefault() != null;
                if (maybeHasPort)
                {
                    uint portNum;
                    if (UInt32.TryParse(inputs[1], out portNum))
                    {
                        await ChangeServer(hostname, portNum);
                    }
                    else
                    {
                        await UserDialogs.Instance.AlertAsync("You must enter a hostname.", "Invalid hostname");
                    }
                }
                else
                {
                    await ChangeServer(hostname, Constants.DEFAULT_PORT);
                }
            }
        }

        private async Task ChangeServer(string hostname, uint portNum)
        {
            DataService.ChangeHostName(hostname, portNum);
            await UpdateScripts();
        }

        private async Task ServerScriptClicked(ServerScriptVm tappedScript)
        {
            try
            {
                using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(20)))
                {
                    tappedScript.IsLoading = true;
                    try
                    {
                        string output = (await DataService.CallServerScript(tappedScript, cts.Token)).Trim();
                        MostRecentServerResponse = output;
                        tappedScript.LastServerResponse = output;
                    }
                    catch (Exception ex)
                    {
                        tappedScript.IsLoading = false;
                        tappedScript.LastServerResponse = $"Error: {ex.ToString()}";
                    }
                    finally
                    {
                        tappedScript.IsLoading = false;
                        await Task.Delay(TimeSpan.FromSeconds(10));
                        tappedScript.LastServerResponse = "";
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine("Changing server timed out. Details:\n" + ex.ToString());
            }

        }

        private void ExpandHideButton()
        {
            _isResponseBoxExpanded = !_isResponseBoxExpanded;

            ServerResponseBlockHeight = _isResponseBoxExpanded
                ? new GridLength(9, GridUnitType.Star)
                : ResponseShrunkHeight;

            RaisePropertyChanged(nameof(IsListVisible));
        }

        private async void RefreshScripts()
        {
            await UpdateScripts();
        }

        public async void Appearing()
        {
            if (_firstLoad)
            {
                _firstLoad = false;
                await UpdateScripts();
            }
        }

        public void Disappearing()
        {
            Settings.HostnameSetting = DataService.BaseUrl;
            Settings.PortSetting = DataService.PortNumber;
        }
    }
}