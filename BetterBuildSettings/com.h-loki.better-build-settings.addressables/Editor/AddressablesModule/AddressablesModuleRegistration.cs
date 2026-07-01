#if UNITY_EDITOR

using UnityEditor;

[InitializeOnLoad]
internal static class AddressablesModuleRegistration
{
    static AddressablesModuleRegistration() =>
        BuildModuleRegistry.Register<AddressablesModule>();
}


#endif