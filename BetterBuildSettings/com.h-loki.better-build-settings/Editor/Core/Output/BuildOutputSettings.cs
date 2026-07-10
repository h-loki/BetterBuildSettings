using System;

namespace BetterBuildSettings.Core.Output
{
    [Serializable]
    public class BuildOutputSettings
    {
        public string OutputRoot = "Builds";
        public bool AskForOutputRootBeforeBuild = true;

        public string BuildFolderPattern = "%profile%_%date:yyyyMMdd_HHmmss%";
        public string ExecutableNamePattern = "%productName%";
    }
}