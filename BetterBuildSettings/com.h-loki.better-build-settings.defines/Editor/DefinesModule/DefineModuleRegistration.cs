#if UNITY_EDITOR

using UnityEditor;

[InitializeOnLoad]
public class DefineModuleRegistration
{
    static DefineModuleRegistration() =>
        BuildModuleRegistry.Register<DefineModule>();
}

#endif