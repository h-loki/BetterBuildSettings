using UnityEditor;

namespace BetterBuildSettings.Core.Output.Naming
{
    [InitializeOnLoad]
    public static class DefaultBuildNameTokenProviderRegistration
    {
        static DefaultBuildNameTokenProviderRegistration()
        {
            BuildNameTokenRegistry.Register(new DefaultBuildNameTokenProvider());
        }
    }
}