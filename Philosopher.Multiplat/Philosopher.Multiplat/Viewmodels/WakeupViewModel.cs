using System;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Philosopher.Multiplat.Helpers;
using Philosopher.Multiplat.Models;
using Philosopher.Multiplat.Services;
using Philosopher.Services;

namespace Philosopher.Multiplat.Viewmodels
{
    public class WakeupViewModel : ViewModelBase, INavigable
    {

        private readonly IMagicPacketService _magicPacketService;
        private readonly ISettingsService _settingsService;

        private string _hostname;
        public string HostName
        {
            get { return _hostname; }
            set
            {
                if (_hostname != value)
                {
                    _hostname = value;
                    RaisePropertyChanged(nameof(HostName));
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
                    RaisePropertyChanged(nameof(PortNumber));
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
                    RaisePropertyChanged(nameof(MacAddress));
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
                    RaisePropertyChanged(nameof(SavedWakeupTargets));
                }
            }
        }

        private RelayCommand<WakeupTarget> _wakeupTargetTappedCommand;
        public RelayCommand<WakeupTarget> WakeupTargetTappedCommand =>
            _wakeupTargetTappedCommand
            ?? (_wakeupTargetTappedCommand = new RelayCommand<WakeupTarget>(WakeTargetPrefill));

        private RelayCommand<WakeupTarget> _removeFromListCommand;
        public RelayCommand<WakeupTarget> RemoveFromListCommand =>
            _removeFromListCommand
            ?? (_removeFromListCommand = new RelayCommand<WakeupTarget>(RemoveFromList));

        private void RemoveFromList(WakeupTarget target)
        {
            SavedWakeupTargets.Remove(target);
        }

        private RelayCommand _sendCommand;
        public RelayCommand SendCommand =>
            _sendCommand
            ?? (_sendCommand = new RelayCommand(Send));

        private void Send()
        {
            if (!String.IsNullOrWhiteSpace(HostName)
                && !String.IsNullOrWhiteSpace(PortNumber)
                && !String.IsNullOrWhiteSpace(MacAddress))
            {
                int portNumber = Int32.Parse(PortNumber);
                _magicPacketService.SendMagicPacket(HostName, portNumber, MacAddress);
                WakeupTarget target = new WakeupTarget
                {
                    Hostname = HostName,
                    PortNumber = Int32.Parse(PortNumber),
                    MacAddress = MacAddress
                };
                AddToList(target);
            }
        }

        private void AddToList(WakeupTarget newTarget)
        {
            if (!SavedWakeupTargets.Contains(newTarget))
            {
                SavedWakeupTargets.Insert(0, newTarget);
            }

            _settingsService.SaveWakeupTargets(SavedWakeupTargets.ToList());
        }

        private void WakeTargetPrefill(WakeupTarget target)
        {
            HostName = target.Hostname;
            PortNumber = target.PortNumber.ToString();
            MacAddress = target.MacAddress;
        }

        public WakeupViewModel(IMagicPacketService magicPacketService, ISettingsService settingsService)
        {
            _magicPacketService = magicPacketService;
            _settingsService = settingsService;
            SavedWakeupTargets = new ObservableCollection<WakeupTarget>(_settingsService.GetWakeupTargets());
        }

        public void Appearing()
        {
            
        }

        public void Disappearing()
        {
            
        }
    }
}