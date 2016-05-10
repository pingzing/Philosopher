using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Philosopher.Multiplat.Models;

namespace Philosopher.Multiplat.Services
{
    public interface IDataService : INotifyPropertyChanged
    {
        IDataService Create();
        IDataService Create(string hostname, uint portNumber);
        string BaseUrl { get; set; }
        uint PortNumber { get; set; }
        void ChangeHostName(string hostname, uint portNumber = 3000);
        void Login(string user, string pass);
        Task<ResultOrErrorResponse<List<ServerScript>>> GetScripts(CancellationToken token);
        Task<string> CallServerScript(ServerScript script, CancellationToken token);
    }    
}