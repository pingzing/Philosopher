using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Philosopher.Multiplat.Models;
using Xamarin.Forms;
using Philosopher.Multiplat.Services;
using Philosopher.Services;

namespace Philosopher.Multiplat.Pages
{
    public partial class WakePage : ContentPage
    {
        private readonly Regex _macRegex = new Regex(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$");
        private readonly Regex _macCharsRegex = new Regex(@"[0-9A-Fa-f:-]{1,17}");
        private IMagicPacketService _magicPacketService;
        private SettingsService _settingsService;

        private string _hostname;
        public string HostName
        {
            get { return _hostname; }
            set
            {
                if (_hostname != value)
                {
                    _hostname = value;
                    OnPropertyChanged(nameof(HostName));
                }
            }
        }

        private string _portNumber;
        public string PortNumber
        {
            get { return _portNumber; }
            set
            {
                if (_portNumber != value)
                {
                    _portNumber = value;
                    OnPropertyChanged(nameof(PortNumber));
                }
            }
        }

        private string _macAddress;
        public string MacAddress
        {
            get { return _macAddress; }
            set
            {
                if (_macAddress != value)
                {
                    _macAddress = value;
                    OnPropertyChanged(nameof(MacAddress));
                }
            }
        }

        private ObservableCollection<WakeupTarget> _savedWakeupTargets = new ObservableCollection<WakeupTarget>();
        public ObservableCollection<WakeupTarget> SavedWakeupTargets
        {
            get { return _savedWakeupTargets; }
            set
            {
                if (_savedWakeupTargets != value)
                {
                    _savedWakeupTargets = value;
                    OnPropertyChanged(nameof(SavedWakeupTargets));
                }
            }
        }

        public WakePage()
        {
            InitializeComponent();
            this.BindingContext = this;
            _magicPacketService = new MagicPacketService();
            _settingsService = new SettingsService();
            SavedWakeupTargets = new ObservableCollection<WakeupTarget>(_settingsService.GetWakeupTargets());
        }

        private void MacEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(e.NewTextValue) && !_macCharsRegex.IsMatch(e.NewTextValue))
            {
                (sender as Entry).Text = e.OldTextValue;
            }
        }

        private void MacEntry_Unfocused(object sender, FocusEventArgs e)
        {
            if(!_macRegex.IsMatch(MacEntry.Text))
            {
                MacAddress = "";
            }
        }

        private void PortNumberEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            int _;
            if (!String.IsNullOrWhiteSpace(e.NewTextValue) && !Int32.TryParse(e.NewTextValue, out _))
            {
                PortNumber = e.OldTextValue;         
            }
        }

        protected override void OnAppearing()
        {
            SendButton.Clicked += SendButton_Clicked;            
            PortNumberEntry.TextChanged += PortNumberEntry_TextChanged;
            MacEntry.TextChanged += MacEntry_TextChanged;
            MacEntry.Unfocused += MacEntry_Unfocused;
            RecentListView.ItemTapped += RecentListView_ItemTapped;
        }       

        protected override void OnDisappearing()
        {
            SendButton.Clicked -= SendButton_Clicked;            
            PortNumberEntry.TextChanged -= PortNumberEntry_TextChanged;
            MacEntry.TextChanged -= MacEntry_TextChanged;
            MacEntry.Unfocused -= MacEntry_Unfocused;
            RecentListView.ItemTapped -= RecentListView_ItemTapped;
        }

        private void SendButton_Clicked(object sender, EventArgs eventArgs)
        {
            if (!String.IsNullOrWhiteSpace(HostName)
                && !String.IsNullOrWhiteSpace(PortNumber)
                && !String.IsNullOrWhiteSpace(MacAddress))
            {
                int portNumber = Int32.Parse(PortNumber);
                _magicPacketService.SendMagicPacket(HostName, portNumber, MacAddress);
                AddEntryToRecentList(HostName, PortNumber, MacAddress);
            }
        }

        private void RecentListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            WakeupTarget target = (WakeupTarget)e.Item;
            HostName = target.Hostname;
            PortNumber = target.PortNumber.ToString();
            MacAddress = target.MacAddress;
        }

        private void AddEntryToRecentList(string hostName, string portNumber, string macAddress)
        {
            WakeupTarget newTarget = new WakeupTarget
            {
                Hostname = hostName,
                PortNumber = Int32.Parse(portNumber),
                MacAddress = macAddress
            };
            if (!SavedWakeupTargets.Contains(newTarget))
            {
                SavedWakeupTargets.Insert(0, newTarget);
            }

            _settingsService.SaveWakeupTargets(SavedWakeupTargets.ToList());
        }
    }
}
