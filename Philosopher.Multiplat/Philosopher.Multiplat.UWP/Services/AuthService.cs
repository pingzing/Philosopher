using Philosopher.Multiplat.Services;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Security.Credentials;

namespace Philosopher.Multiplat.UWP.Services
{
    public class AuthService : IAuthService
    {
        private PasswordVault _vault;        

        private void Initialize()
        {
            _vault = new PasswordVault();                        
        }

        public void Save(Account account, string serviceId)
        {
            if(_vault == null)
            {
                Initialize();
            }

            PasswordCredential credential = new PasswordCredential(serviceId, account.Username, account.Password);                        
            _vault.Add(credential);            
        }

        public void Delete(Account account, string serviceId)
        {
            if (_vault == null)
            {
                Initialize();
            }

            PasswordCredential cred = _vault.Retrieve(serviceId, account.Username);
            _vault.Remove(cred);
        }

        public IEnumerable<Account> FindAccountsForService(string serviceId)
        {
            if (_vault == null)
            {
                Initialize();
            }

            List<Account> accounts = new List<Account>();
            try
            {
                var credentials = _vault.FindAllByResource(serviceId);                
                foreach (var cred in credentials)
                {
                    string username = (string)cred.UserName;
                    cred.RetrievePassword();
                    string password = cred.Password;
                    accounts.Add(new Account { Username = username, Password = password });
                }
                return accounts;
            }
            catch(COMException ex)
            {
                System.Diagnostics.Debug.WriteLine("UWP.AuthService: Unable to find ServiceID in PasswordVault.");
                return accounts; 
            }            
        }
    }
}
