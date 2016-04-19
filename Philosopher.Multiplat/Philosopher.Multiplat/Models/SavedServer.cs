using System.Collections.Generic;

namespace Philosopher.Multiplat.Models
{
    public class SavedServer
    {
        public string FriendlyName { get; set; }
        public string Hostname { get; set; }
        public int PortNumber { get; set; }
        public List<ServerScript> CachedScripts { get; set; }
    }
}
