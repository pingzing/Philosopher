using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Philosopher.Multiplat.Models;
using Xamarin.Forms;

namespace Philosopher.Multiplat.Pages
{
    public partial class WakePage : ContentPage
    {
        private readonly Regex _macRegex = new Regex(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$");        

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

        private ObservableCollection<WakeupTarget> _savedWakeupTargets;
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
            SendButton.Clicked += SendButton_Clicked;
            HostnameEntry.TextChanged += HostnameEntry_TextChanged;
            PortNumberEntry.TextChanged += PortNumberEntry_TextChanged;
            MacEntry.TextChanged += MacEntry_TextChanged;            
        }

        private void MacEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_macRegex.IsMatch(e.NewTextValue))
            {
                (sender as Entry).Text = e.OldTextValue;
            }
        }

        private void PortNumberEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            int _;
            if (!Int32.TryParse(e.NewTextValue, out _))
            {
                (sender as Entry).Text = e.OldTextValue;
            }
        }

        private void HostnameEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            Uri resultUri;
            bool isValid = Uri.TryCreate(e.NewTextValue, UriKind.Absolute, out resultUri) &&
                           (resultUri.Scheme == "http" || resultUri.Scheme == "https");
            if (!isValid)
            {
                (sender as Entry).Text = e.OldTextValue;                
            }
        }

        protected override void OnDisappearing()
        {
            SendButton.Clicked -= SendButton_Clicked;
            HostnameEntry.TextChanged -= HostnameEntry_TextChanged;
            PortNumberEntry.TextChanged -= PortNumberEntry_TextChanged;
            MacEntry.TextChanged -= MacEntry_TextChanged;
        }

        private void SendButton_Clicked(object sender, EventArgs eventArgs)
        {
            if (!String.IsNullOrWhiteSpace(HostName)
                && !String.IsNullOrWhiteSpace(PortNumber)
                && !String.IsNullOrWhiteSpace(MacAddress))
            {
                //validate hostname
                //validate port number
                //validate MAC address
            }
        }
    }
}
