using System;
using Newtonsoft.Json.Linq;

[Serializable]
public class ModuleConfig
{
    public string ModuleId;
    public bool Enabled;
    public JObject JsonPayload;
}