using Philosopher.Models;
using Philosopher.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Philosopher
{    
    public sealed partial class MainPage : Page, INotifyPropertyChanged
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
                    NotifyPropertyChanged(nameof(DataService));
                }
            }
        }

        private ObservableCollection<ServerScript> _scriptList;
        public ObservableCollection<ServerScript> ScriptList
        {
            get { return _scriptList; }
            set
            {
                if(value != _scriptList)
                {
                    _scriptList = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public MainPage()
        {
            DataService = new DataService();
            this.InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName]string property ="")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {                  
            _cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            List<ServerScript> scripts = await DataService.GetScripts(_cts.Token);
            List<ServerScriptVm> bindableScripts = scripts.Select(x => new ServerScriptVm(x)).ToList();
            ScriptList = new ObservableCollection<ServerScript>(bindableScripts);
        }

        private async void ScriptsGrid_ItemClick(object sender, ItemClickEventArgs e)
        {

            ServerScriptVm item = e.ClickedItem as ServerScriptVm;            
            _cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            GridView gView = sender as GridView;
            GridViewItem clickedVisual = (GridViewItem)gView.ContainerFromItem(item);
            clickedVisual.IsEnabled = false;
            item.IsLoading = true;

            try
            {
                string output = await DataService.CallServerScript(item, _cts.Token);
                output = output.Trim();

                TextBlock flyoutTextBlock = new TextBlock();
                flyoutTextBlock.Text = output;
                Flyout response = (Flyout)FlyoutBase.GetAttachedFlyout((FrameworkElement)clickedVisual.ContentTemplateRoot);
                response.Content = flyoutTextBlock;
                response.ShowAt((FrameworkElement)clickedVisual.ContentTemplateRoot);
            }
            finally
            {
                item.IsLoading = false;
                clickedVisual.IsEnabled = true;
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
                if(value != _isLoading)
                {
                    _isLoading = value;
                    NotifyPropertyChanged();
                }
            }        
        }

        public ServerScriptVm(ServerScript b)
        {
            this.Name = b.Name;
            this.RelativePath = b.RelativePath;
            this.ScriptKind = b.ScriptKind;
            this.IsLoading = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
