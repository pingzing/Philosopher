using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Philosopher.Multiplat.Models
{
    public class Enums
    {        
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ScriptKind
        {
            Unknown = 0,
            PowerShell,
            Python,
            Shell,
            Binary,
        }
    }
}
