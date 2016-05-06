﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Philosopher.Multiplat.Helpers;
using Philosopher.Multiplat.Models;
using Philosopher.Multiplat.Services;
using Xamarin.Forms;

namespace Philosopher.Multiplat.Pages
{
    public partial class ScriptsPage : ContentPage, INotifyPropertyChanged
    {
        public static int ResponseShrunkHeight => 25;

        private IDataService _dataService;
        public IDataService DataService
        {
            get { return _dataService; }
            set
            {
                if (_dataService != value)
                {
                    _dataService = value;
                    OnPropertyChanged(nameof(DataService));
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
                    OnPropertyChanged();
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
                    OnPropertyChanged(nameof(MostRecentServerResponse));
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
                OnPropertyChanged(nameof(ServerResponseBlockHeight));
            }
        }

        private double _responseLabelHeight;
        public double ResponseLabelHeight
        {
            get { return _responseLabelHeight; }
            set
            {
                if (_responseLabelHeight != value)
                {
                    _responseLabelHeight = value;
                    OnPropertyChanged(nameof(ResponseLabelHeight));
                }
            }
        }

        public ScriptsPage()
        {
            InitializeComponent();
            DataService = DependencyService.Get<IDataService>()
                .Create(Settings.HostnameSetting, (uint) Settings.PortSetting);
            this.BindingContext = this;
            this.Appearing += MainPage_OnAppearing;
            this.Disappearing -= MainPage_Disappearing;
        }

        private async void MainPage_OnAppearing(object sender, EventArgs e)
        {
            this.ConnectedToLink.Clicked += ConnectedToLink_OnClicked;
            this.Disappearing += MainPage_Disappearing;

            await UpdateScripts();
        }

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
                                    ConnectedToLink_OnClicked(null, null);
                                    return;
                                }
                                List<ServerScriptVm> bindableScripts = scripts.Select(x => new ServerScriptVm(x)).ToList();
                                ScriptList = new ObservableCollection<ServerScript>(bindableScripts);
                                return;
                            }

                            catch (Exception ex)
                            {
                                ConnectedToLink_OnClicked(null, null);
                                return;
                            }
                        }
                        else if ((HttpStatusCode) result.ErrorResponse.HttpStatusCode == HttpStatusCode.Unauthorized)
                        {
                            var dialogResult = await UserDialogs.Instance.LoginAsync("Authorization required",
                                $"Access denied. The server says \"{result.ErrorResponse.ResponseMessage}\"");
                            if (dialogResult != null
                                && dialogResult.Ok
                                && !String.IsNullOrWhiteSpace(dialogResult.LoginText)
                                && !String.IsNullOrWhiteSpace(dialogResult.Password))
                            {
                                DataService.Login(dialogResult.LoginText, dialogResult.Password);
                                //loop and try again
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
        }

        private void MainPage_Disappearing(object sender, EventArgs e)
        {
            if (DataService.BaseUrl.Contains("://"))
            {
                string[] urlComponents = DataService.BaseUrl.Split(':');
                Settings.HostnameSetting = urlComponents[0] + ":" + urlComponents[1];
                Settings.PortSetting = UInt32.Parse(urlComponents[2]);
            }
            else
            {
                string[] urlComponents = DataService.BaseUrl.Split(':');
                Settings.HostnameSetting = urlComponents[0];
                Settings.PortSetting = UInt32.Parse(urlComponents[1]);
            }

            this.ConnectedToLink.Clicked -= ConnectedToLink_OnClicked;
        }

        private async void ConnectedToLink_OnClicked(object sender, EventArgs e)
        {
            PromptResult result = await UserDialogs.Instance.PromptAsync(HostnamePromptConfig);
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

        //todo: replace event with commanding model. we've probably got memory leaks on this event subscription
        private async void ScriptsListView_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            ServerScriptVm item = e.Item as ServerScriptVm;
            if (item == null)
            {
                return;
            }

            try
            {
                using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(20)))
                {
                    ListView list = sender as ListView;
                    Cell viewCell = ((IReadOnlyList<Cell>) e.Group).FirstOrDefault(x => x.BindingContext == item);
                    item.IsLoading = true;
                    try
                    {
                        string output = (await DataService.CallServerScript(item, cts.Token)).Trim();
                        MostRecentServerResponse = output;
                        item.LastServerResponse = output;
                    }
                    catch (Exception ex)
                    {
                        item.IsLoading = false;
                        item.LastServerResponse = $"Error: {ex.ToString()}";
                    }
                    finally
                    {
                        item.IsLoading = false;
                        await Task.Delay(TimeSpan.FromSeconds(10));
                        item.LastServerResponse = "";
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine("Changing server timed out. Details:\n" + ex.ToString());                
            }
        }

        private void ExpandHideButton_OnClicked(object sender, EventArgs e)
        {
            ServerResponseBlockHeight = ServerResponseBlockHeight.Value == ResponseShrunkHeight
                ? GridLength.Auto
                : ResponseShrunkHeight;
        }
    }

    public enum HostnamePrefix
    {
        None,
        Http,
        Https
    }

    public class ServerScriptVm : ServerScript, INotifyPropertyChanged
    {
        private bool _isLoading = false;
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                if (value != _isLoading)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _lastServerResponse;
        public string LastServerResponse
        {
            get { return _lastServerResponse; }
            set
            {
                if (_lastServerResponse != value)
                {
                    _lastServerResponse = value;
                    OnPropertyChanged(nameof(LastServerResponse));
                    OnPropertyChanged(nameof(ShouldShow));
                }
            }
        }

        public bool ShouldShow => !String.IsNullOrEmpty(LastServerResponse);

        public ServerScriptVm(ServerScript b)
        {
            this.Name = b.Name;
            this.RelativePath = b.RelativePath;
            this.ScriptKind = b.ScriptKind;
            this.IsLoading = false;
            this.LastServerResponse = "";
        }

        public ServerScriptVm WithOutput(string output)
        {
            this.LastServerResponse = output;
            return this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}