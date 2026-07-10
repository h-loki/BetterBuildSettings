using BetterBuildSettings.Core;
using UnityEditor;

namespace BetterBuildSettings.com.h_loki.better_build_settings.scenes.Editor
{
    [InitializeOnLoad]
    internal static class ScenesModuleRegistration
    {
        static ScenesModuleRegistration() =>
            BuildModuleRegistry.Register<ScenesModule>();
    }
}