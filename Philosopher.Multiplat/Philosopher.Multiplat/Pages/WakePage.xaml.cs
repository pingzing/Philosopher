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
using Philosopher.Multiplat.Viewmodels;
using Philosopher.Services;

namespace Philosopher.Multiplat.Pages
{
    public partial class WakePage : ContentPage
    {
        private readonly Regex _macRegex = new Regex(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$");
        private readonly Regex _macCharsRegex = new Regex(@"[0-9A-Fa-f:-]{1,17}");            

        public WakePage()
        {
            InitializeComponent();
            this.BindingContext = ((App)Application.Current).Locator.Wakeup;            
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
            if(MacEntry.Text != null && !_macRegex.IsMatch(MacEntry.Text))
            {
                (sender as Entry).Text = "";
            }
        }

        private void PortNumberEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            int _;
            if (!String.IsNullOrWhiteSpace(e.NewTextValue) && !Int32.TryParse(e.NewTextValue, out _))
            {
                (sender as Entry).Text = e.OldTextValue;         
            }
        }

        protected override void OnAppearing()
        {
            var vm = this.BindingContext as WakeupViewModel;
            vm?.Appearing();

            SendButton.Clicked += SendButton_Clicked;            
            PortNumberEntry.TextChanged += PortNumberEntry_TextChanged;
            MacEntry.TextChanged += MacEntry_TextChanged;
            MacEntry.Unfocused += MacEntry_Unfocused;
            RecentListView.ItemTapped += RecentListView_ItemTapped;            
        }       

        protected override void OnDisappearing()
        {
            var vm = this.BindingContext as WakeupViewModel;
            vm?.Disappearing();

            SendButton.Clicked -= SendButton_Clicked;            
            PortNumberEntry.TextChanged -= PortNumberEntry_TextChanged;
            MacEntry.TextChanged -= MacEntry_TextChanged;
            MacEntry.Unfocused -= MacEntry_Unfocused;
            RecentListView.ItemTapped -= RecentListView_ItemTapped;
        }

        private void SendButton_Clicked(object sender, EventArgs eventArgs)
        {
            var vm = this.BindingContext as WakeupViewModel;
            vm?.SendCommand.Execute(null);
        }

        private void RecentListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var vm = this.BindingContext as WakeupViewModel;
            vm?.WakeupTargetTappedCommand.Execute((WakeupTarget)e.Item);
        }       
    }
}
