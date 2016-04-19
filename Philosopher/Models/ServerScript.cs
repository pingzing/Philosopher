using Newtonsoft.Json;
using static Philosopher.Models.Enums;

namespace Philosopher.Models
{
    public class ServerScript
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("relative_path")]
        public string RelativePath { get; set; }
        [JsonProperty("script_kind")]
        public ScriptKind ScriptKind { get; set; }                
    }
}
