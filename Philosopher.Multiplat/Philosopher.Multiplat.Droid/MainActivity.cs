using System;
using Acr.UserDialogs;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.Settings;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Philosopher.Multiplat.Droid
{
    [Activity(Label = "Philosopher", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            UserDialogs.Init(this);            
            LoadApplication(new App());
            System.Net.ServicePointManager.ServerCertificateValidationCallback += OnServerCertValidation;
        }

        private static bool OnServerCertValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

