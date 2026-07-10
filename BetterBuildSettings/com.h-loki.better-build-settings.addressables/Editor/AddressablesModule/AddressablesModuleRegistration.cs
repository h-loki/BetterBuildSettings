#if UNITY_EDITOR

using BetterBuildSettings.Core;
using UnityEditor;

[InitializeOnLoad]
internal static class AddressablesModuleRegistration
{
    static AddressablesModuleRegistration() =>
        BuildModuleRegistry.Register<AddressablesModule>();
}


#endif