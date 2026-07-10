using System.Collections.Generic;

namespace BetterBuildSettings.Core.Output.Naming
{
    public interface IBuildNameTokenProvider
    {
        bool TryResolve(string token, string argument, BuildContext context, out string value);

        IEnumerable<BuildNameTokenDescriptor> GetTokenDescriptors();
    }
}