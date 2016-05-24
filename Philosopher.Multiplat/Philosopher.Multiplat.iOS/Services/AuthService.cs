using System.Collections;
using System.Collections.Generic;
using Philosopher.Multiplat.iOS.Services;
using Philosopher.Multiplat.Services;

[assembly: Xamarin.Forms.Dependency(typeof(AuthService))]
namespace Philosopher.Multiplat.iOS.Services
{    
    public class AuthService : IAuthService
    {
        public IEnumerable<Account> FindAccountsForService(string serviceId)
        {
            throw new System.NotImplementedException();
        }

        public void Save(Account acount, string serviceId)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(Account account, string serviceId)
        {
            throw new System.NotImplementedException();
        }
    }
}