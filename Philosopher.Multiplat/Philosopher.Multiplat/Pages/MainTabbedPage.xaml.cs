using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Philosopher.Multiplat.Pages
{
    public partial class MainTabbedPage : TabbedPage
    {
        public MainTabbedPage()
        {
            InitializeComponent();
        }

        private void MenuItem_OnClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("RefreshItem clicked.");
            this.ScriptsPage.Refresh_OnClicked(sender, e);
        }
    }
}
