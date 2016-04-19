using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philosopher.Models
{
    public class SavedServer
    {
        public string FriendlyName { get; set; }
        public string Hostname { get; set; }
        public int PortNumber { get; set; }
        public List<ServerScript> CachedScripts { get; set; }
    }
}
