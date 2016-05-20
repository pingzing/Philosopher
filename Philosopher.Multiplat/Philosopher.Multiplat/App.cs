using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Philosopher.Multiplat.Pages;
using Philosopher.Multiplat.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

//[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Philosopher.Multiplat
{
    public class App : Application
    {
        public App()
        {
            Device.OnPlatform(
                iOS: DependencyService.Register<DataService>,
                Android: DependencyService.Register<DataService>);
            var idiom = Device.Idiom;
            // The root page of your application                     
            MainPage = new MainPage(new MainTabbedPage());
        }

        protected override void OnStart()
        {
            // Handle when your app starts            
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
