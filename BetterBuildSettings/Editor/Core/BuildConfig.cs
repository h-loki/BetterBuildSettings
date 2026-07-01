using System;
using System.Collections.Generic;

namespace BetterBuildSettings.Editor.Core
{
    [Serializable]
    public class BuildConfig
    {
        public List<ModuleConfig> Modules = new();
    }
}