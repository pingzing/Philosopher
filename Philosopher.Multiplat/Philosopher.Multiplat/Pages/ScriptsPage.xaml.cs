using System;
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
using Philosopher.Multiplat.Viewmodels;
using Xamarin.Forms;

namespace Philosopher.Multiplat.Pages
{
    public partial class ScriptsPage : ContentPage, INotifyPropertyChanged
    {
        private readonly IAuthService _authService;                       

    
        public ScriptsPage()
        {
            InitializeComponent();           
            this.BindingContext = ((App) Application.Current).Locator.Scripts;
        }

        protected override async void OnAppearing()
        {
            this.ConnectedToLink.Clicked += ConnectedToLink_OnClicked;
            var vm = this.BindingContext as ScriptsViewModel;
            vm?.Appearing();
        }        

        protected override void OnDisappearing()
        {
            var navigable = this.BindingContext as INavigable;
            navigable?.Disappearing();           

            this.ConnectedToLink.Clicked -= ConnectedToLink_OnClicked;            
        }       

        private void ConnectedToLink_OnClicked(object sender, EventArgs e)
        {
            var vm = this.BindingContext as ScriptsViewModel;
            vm?.ServerButtonClickedCommand.Execute(null);
        }        
        
        private void ScriptsListView_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            ServerScriptVm item = e.Item as ServerScriptVm;
            if (item == null)
            {
                return;
            }

            var vm = this.BindingContext as ScriptsViewModel;
            vm?.ServerScriptClickedCommand.Execute(item);
        }

        private void ExpandHideButton_OnClicked(object sender, EventArgs e)
        {
            var vm = this.BindingContext as ScriptsViewModel;
            vm?.ExpandHideButtonCommand.Execute(null);
        }

        public void Refresh_OnClicked(object sender, EventArgs e)
        {
            var vm = this.BindingContext as ScriptsViewModel;
            vm?.RefreshCommand.Execute(null);
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
