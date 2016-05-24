using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Philosopher.Multiplat.Pages
{
    public partial class MainPage : NavigationPage
    {
        public MainPage()
        {
            InitializeComponent();            
        }

        public MainPage(Page root) : base(root)
        {            
            InitializeComponent();
        }
    }
}
