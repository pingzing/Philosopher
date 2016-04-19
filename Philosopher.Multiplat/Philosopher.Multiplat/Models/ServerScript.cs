using Newtonsoft.Json;

namespace Philosopher.Multiplat.Models
{
    public class ServerScript
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("relative_path")]
        public string RelativePath { get; set; }
        [JsonProperty("script_kind")]
        public Enums.ScriptKind ScriptKind { get; set; }                
    }
}
