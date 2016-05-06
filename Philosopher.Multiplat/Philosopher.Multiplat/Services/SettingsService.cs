using Philosopher.Multiplat.Helpers;
using Philosopher.Multiplat.Models;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philosopher.Services
{
    public interface ISettingsService
    {
        List<WakeupTarget> GetWakeupTargets();
        void SaveWakeupTargets(List<WakeupTarget> targets);
    }

    public class SettingsService : ISettingsService
    {
        public List<WakeupTarget> GetWakeupTargets()
        {
            string settingsJson = Settings.WakeupTargets;
            List<WakeupTarget> targets = JsonConvert.DeserializeObject<List<WakeupTarget>>(settingsJson);
            return targets;
        }

        public void SaveWakeupTargets(List<WakeupTarget> targets)
        {
            string targetsJson = JsonConvert.SerializeObject(targets);
            Settings.WakeupTargets = targetsJson;
        }
    }
}
