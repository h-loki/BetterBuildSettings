using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;

namespace BetterBuildSettings.Core.Output.Naming
{
    public static class BuildNameResolver
    {
        private static readonly Regex TokenRegex = new(
            "%(?<name>[a-zA-Z0-9_]+)(:(?<argument>[^%]+))?%",
            RegexOptions.Compiled);

        public static string Resolve(string pattern, BuildContext context)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                return string.Empty;

            return TokenRegex.Replace(pattern, match =>
            {
                var token = match.Groups["name"].Value;
                var argument = match.Groups["argument"].Success
                    ? match.Groups["argument"].Value
                    : null;

                return BuildNameTokenRegistry.TryResolve(
                    token,
                    argument,
                    context,
                    out var value)
                    ? value
                    : match.Value;
            });
        }

        public static string SanitizeFileName(string value, string fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
                value = fallback;

            foreach (var c in Path.GetInvalidFileNameChars())
                value = value.Replace(c, '_');

            value = value.Trim();

            return string.IsNullOrWhiteSpace(value)
                ? fallback
                : value;
        }

        public static string SanitizeDirectoryName(string value, string fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
                value = fallback;

            foreach (var c in Path.GetInvalidPathChars())
                value = value.Replace(c, '_');

            foreach (var c in Path.GetInvalidFileNameChars())
                value = value.Replace(c, '_');

            value = value.Trim();

            return string.IsNullOrWhiteSpace(value)
                ? fallback
                : value;
        }

        public static string EnsureExecutableExtension(string fileName, BuildContext context)
        {
            if (context.Target == BuildTarget.StandaloneWindows ||
                context.Target == BuildTarget.StandaloneWindows64)
            {
                if (!fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    return fileName + ".exe";
            }

            return fileName;
        }
    }
}