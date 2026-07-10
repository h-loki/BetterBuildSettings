using System.IO;
using BetterBuildSettings.Core.Output.Naming;
using UnityEditor;

namespace BetterBuildSettings.Core.Output
{
    public static class BuildOutputResolver
    {
        public static bool TryResolve(BuildContext context)
        {
            var output = context.Config.Output;

            if (output == null)
            {
                output = new BuildOutputSettings();
                context.Config.Output = output;
            }

            var outputRoot = output.OutputRoot;

            if (output.AskForOutputRootBeforeBuild)
            {
                var selected = EditorUtility.OpenFolderPanel(
                    "Select build output folder",
                    string.IsNullOrWhiteSpace(outputRoot) ? "Builds" : outputRoot,
                    "");

                if (string.IsNullOrWhiteSpace(selected))
                    return false;

                outputRoot = selected;
                output.OutputRoot = selected;
            }

            if (string.IsNullOrWhiteSpace(outputRoot))
                outputRoot = "Builds";

            var folderName = BuildNameResolver.Resolve(
                output.BuildFolderPattern,
                context);

            folderName = BuildNameResolver.SanitizeDirectoryName(
                folderName,
                "Build");

            var executableName = BuildNameResolver.Resolve(
                output.ExecutableNamePattern,
                context);

            executableName = BuildNameResolver.SanitizeFileName(
                executableName,
                "Game");

            executableName = BuildNameResolver.EnsureExecutableExtension(
                executableName,
                context);

            var buildRoot = Path.Combine(outputRoot, folderName);

            Directory.CreateDirectory(buildRoot);

            var buildPath = Path.Combine(buildRoot, executableName);

            context.OutputRoot = outputRoot;
            context.BuildFolderName = folderName;
            context.ExecutableName = executableName;
            context.SetBuildPath(buildPath);

            return true;
        }
    }
}