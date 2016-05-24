using System.Runtime.InteropServices.WindowsRuntime;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using Philosopher.Multiplat.Models;
using Philosopher.Multiplat.Services;
using Philosopher.Multiplat.Viewmodels;
using Philosopher.Services;
using Xamarin.Forms;

namespace Philosopher.Multiplat.Helpers
{
    public class Locator
    {
        public Locator()
        {            
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            
            //Viewmodels
            SimpleIoc.Default.Register<ScriptsViewModel>();            
            SimpleIoc.Default.Register<WakeupViewModel>();            

            //Services
            SimpleIoc.Default.Register(() => DependencyService.Get<IDataService>());        
            SimpleIoc.Default.Register(() => DependencyService.Get<IAuthService>());
            SimpleIoc.Default.Register<IMagicPacketService>(() => new MagicPacketService());
            SimpleIoc.Default.Register<ISettingsService>(() => new SettingsService());
        }

        //getters
        public ScriptsViewModel Scripts => ServiceLocator.Current.GetInstance<ScriptsViewModel>();
        public WakeupViewModel Wakeup => ServiceLocator.Current.GetInstance<WakeupViewModel>();
    }
}