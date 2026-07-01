using System;
using Newtonsoft.Json.Linq;

namespace BetterBuildSettings.Editor.Core
{
    [Serializable]
    public class ModuleConfig
    {
        public string ModuleId;
        public bool Enabled;
        public JObject JsonPayload;
    }
}