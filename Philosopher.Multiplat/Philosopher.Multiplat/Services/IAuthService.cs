using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philosopher.Multiplat.Services
{
    public interface IAuthService
    {        
        IEnumerable<Account> FindAccountsForService(string serviceId);
        void Save(Account acount, string serviceId);
        void Delete(Account account, string serviceId);
    }
    
    public class Account
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }        

        public static Account Deserialize(string acct)
        {
            return JsonConvert.DeserializeObject<Account>(acct);
        }
    }    
}
