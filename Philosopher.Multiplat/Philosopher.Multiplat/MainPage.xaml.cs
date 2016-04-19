using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Philosopher.Multiplat.Models;
using Philosopher.Multiplat.Services;
using Xamarin.Forms;

namespace Philosopher.Multiplat
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private CancellationTokenSource _cts;

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

        private ObservableCollection<ServerScript> _scriptList;
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


        public MainPage()
        {
            DataService = new DataService();
            InitializeComponent();
            this.BindingContext = this;
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
        }

        private async void MainPage_OnAppearing(object sender, EventArgs e)
        {
            _cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            List<ServerScript> scripts = await DataService.GetScripts(_cts.Token);
            List<ServerScriptVm> bindableScripts = scripts.Select(x => new ServerScriptVm(x)).ToList();
            ScriptList = new ObservableCollection<ServerScript>(bindableScripts);
        }

        private void ConnectedToLink_OnClicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void ScriptsListView_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            ServerScriptVm item = e.Item as ServerScriptVm;
            if (item == null)
            {
                return;
            }

            int index = ScriptList.IndexOf(item);

            _cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            ListView list = sender as ListView;
            Cell viewCell = ((IReadOnlyList<Cell>) e.Group).FirstOrDefault(x => x.BindingContext == item);
            viewCell.IsEnabled = false;
            item.IsLoading = true;            
            try
            {
                string output = (await DataService.CallServerScript(item, _cts.Token)).Trim();
                ScriptList.Remove(item);
                ScriptList.Insert(index, item.WithOutput(output));
            }
            finally
            {
                item.IsLoading = false;
                viewCell.IsEnabled = true;
                await Task.Delay(TimeSpan.FromSeconds(10));
                ScriptList.Remove(item);
                ScriptList.Insert(index, item.WithOutput(""));
            }
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
                    NotifyPropertyChanged();
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
                    NotifyPropertyChanged(nameof(LastServerResponse));
                    NotifyPropertyChanged(nameof(ShouldShow));
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
        private void NotifyPropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
