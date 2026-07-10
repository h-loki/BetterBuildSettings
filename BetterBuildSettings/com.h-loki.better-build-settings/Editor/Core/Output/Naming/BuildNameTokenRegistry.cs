using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterBuildSettings.Core.Output.Naming
{
    public static class BuildNameTokenRegistry
    {
        private static readonly List<IBuildNameTokenProvider> Providers = new();

        public static void Register(IBuildNameTokenProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            if (!Providers.Contains(provider))
                Providers.Add(provider);
        }

        public static bool TryResolve(
            string token,
            string argument,
            BuildContext context,
            out string value)
        {
            for (var i = Providers.Count - 1; i >= 0; i--)
            {
                if (Providers[i].TryResolve(token, argument, context, out value))
                    return true;
            }

            value = null;
            return false;
        }
        public static IReadOnlyList<BuildNameTokenDescriptor> GetTokenDescriptors()
        {
            return Providers
                .SelectMany(x => x.GetTokenDescriptors())
                .GroupBy(x => x.Token)
                .Select(x => x.Last())
                .OrderBy(x => x.Token)
                .ToList();
        }
    }
}