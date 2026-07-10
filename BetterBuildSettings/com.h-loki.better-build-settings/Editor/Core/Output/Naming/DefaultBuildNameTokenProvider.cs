using System.Collections.Generic;

namespace BetterBuildSettings.Core.Output.Naming
{
    public sealed class DefaultBuildNameTokenProvider : IBuildNameTokenProvider
    {
        public bool TryResolve(
            string token,
            string argument,
            BuildContext context,
            out string value)
        {
            switch (token)
            {
                case "configName":
                    value = context.ProfileName;
                    return true;

                case "productName":
                    value = context.ProductName;
                    return true;

                case "target":
                    value = context.Target.ToString();
                    return true;

                case "date":
                    value = context.BuildTime.ToString(
                        string.IsNullOrWhiteSpace(argument)
                            ? "yyyyMMdd"
                            : argument);
                    return true;

                case "time":
                    value = context.BuildTime.ToString(
                        string.IsNullOrWhiteSpace(argument)
                            ? "HHmmss"
                            : argument);
                    return true;

                default:
                    value = null;
                    return false;
            }
        }
    
        public IEnumerable<BuildNameTokenDescriptor> GetTokenDescriptors()
        {
            yield return new BuildNameTokenDescriptor(
                "configName",
                "%configName%",
                "Current Better Build Settings config name.");

            yield return new BuildNameTokenDescriptor(
                "productName",
                "%productName%",
                "Unity PlayerSettings product name.");

            yield return new BuildNameTokenDescriptor(
                "target",
                "%target%",
                "Current Unity build target.");

            yield return new BuildNameTokenDescriptor(
                "date",
                "%date% or %date:yyyyMMdd_HHmmss%",
                "Build date. Supports custom DateTime format after ':'.");

            yield return new BuildNameTokenDescriptor(
                "time",
                "%time% or %time:HH-mm-ss%",
                "Build time. Supports custom DateTime format after ':'.");
        }
    }
}
