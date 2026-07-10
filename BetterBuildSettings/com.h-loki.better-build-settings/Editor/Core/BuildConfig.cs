using System;
using System.Collections.Generic;
using BetterBuildSettings.Core.Output;

namespace BetterBuildSettings.Core
{
    [Serializable]
    public class BuildConfig
    {
        public BuildOutputSettings Output = new();
        public List<ModuleConfig> Modules = new();
    }
}