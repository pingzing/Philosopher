using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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

        private DataService _dataService;
        public DataService DataService
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
            DataService = new DataService(Settings.HostnameSetting, (uint)Settings.PortSetting);
            this.BindingContext = this;
            this.Appearing += MainPage_OnAppearing;
            this.ConnectedToLink.Clicked += ConnectedToLink_OnClicked;
            this.Disappearing += MainPage_Disappearing;
        }

        private async void MainPage_OnAppearing(object sender, EventArgs e)
        {
            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(20)))
            {
                try
                {
                    List<ServerScript> scripts = await DataService.GetScripts(cts.Token);
                    List<ServerScriptVm> bindableScripts = scripts.Select(x => new ServerScriptVm(x)).ToList();
                    ScriptList = new ObservableCollection<ServerScript>(bindableScripts);
                }
                catch (Exception ex)
                {
                    ConnectedToLink_OnClicked(null, null);
                }
            }
        }

        private void MainPage_Disappearing(object sender, EventArgs e)
        {
            string[] urlComponents = DataService.BaseUrl.Replace("http://", "").Split(':');
            Settings.HostnameSetting = urlComponents[0];
            Settings.PortSetting = UInt32.Parse(urlComponents[1]);

            this.Appearing -= MainPage_OnAppearing;
            this.ConnectedToLink.Clicked -= ConnectedToLink_OnClicked;
            this.Disappearing -= MainPage_Disappearing;
        }

        private async void ConnectedToLink_OnClicked(object sender, EventArgs e)
        {
            PromptResult result = await UserDialogs.Instance.PromptAsync(HostnamePromptConfig);            
            if (result != null && result.Ok && !String.IsNullOrWhiteSpace(result.Text))
            {
                string text = result.Text.Replace("http://", "");
                string[] inputs = text.Split(':');
                string hostname = inputs.FirstOrDefault();
                if (hostname == null)
                {
                    await UserDialogs.Instance.AlertAsync("You must enter a hostname.", "Invalid hostname");
                    return;
                }
                hostname = $"http://{hostname}";
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
            DataService.ChangeHostname(hostname, portNum);
            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(20)))
            {
                List<ServerScript> newScripts = await DataService.GetScripts(cts.Token);
                List<ServerScriptVm> bindableScripts = newScripts.Select(x => new ServerScriptVm(x)).ToList();
                ScriptList.Clear();
                foreach (var scr in bindableScripts)
                {
                    ScriptList.Add(scr);
                }
                Settings.HostnameSetting = hostname;
                Settings.PortSetting = portNum;
            }
        }

        //todo: replace event with commanding model. we've probably got memory leaks on this event subscription
        private async void ScriptsListView_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            ServerScriptVm item = e.Item as ServerScriptVm;
            if (item == null)
            {
                return;
            }

            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(20)))
            {
                ListView list = sender as ListView;
                Cell viewCell = ((IReadOnlyList<Cell>)e.Group).FirstOrDefault(x => x.BindingContext == item);
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

        private void ExpandHideButton_OnClicked(object sender, EventArgs e)
        {
            ServerResponseBlockHeight = ServerResponseBlockHeight.Value == ResponseShrunkHeight
                ? GridLength.Auto
                : ResponseShrunkHeight;
        }
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
