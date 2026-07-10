using System;
using UnityEditor;

namespace BetterBuildSettings.Core
{
    public sealed class BuildContext
    {
        public BuildConfig Config { get; }
        public string ProfileName { get; }
        public BuildTarget Target { get; }
        public DateTime BuildTime { get; }

        public string OutputRoot { get; set; }
        public string BuildFolderName { get; set; }
        public string ExecutableName { get; set; }
        public string BuildPath { get; private set; }

        public string ProductName => PlayerSettings.productName;

        public BuildContext(
            BuildConfig config,
            string profileName,
            BuildTarget target,
            DateTime buildTime)
        {
            Config = config;
            ProfileName = profileName;
            Target = target;
            BuildTime = buildTime;
        }

        public void SetBuildPath(string buildPath)
        {
            BuildPath = buildPath;
        }
    }
}