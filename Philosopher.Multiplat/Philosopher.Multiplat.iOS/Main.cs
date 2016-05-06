using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Foundation;
using UIKit;

namespace Philosopher.Multiplat.iOS
{
    public class Application
    {
        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            UIApplication.Main(args, null, "AppDelegate");
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
