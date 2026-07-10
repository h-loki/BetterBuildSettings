#if UNITY_EDITOR

using BetterBuildSettings.Core;
using UnityEditor;

[InitializeOnLoad]
public class DefineModuleRegistration
{
    static DefineModuleRegistration() =>
        BuildModuleRegistry.Register<DefineModule>();
}

#endif