using UnityEditor;

namespace BetterBuildSettings.Editor.Core
{
    public class BuildContext
    {
        public readonly BuildTarget Target;
        public readonly string BuildPath;

        public BuildContext(string buildPath, BuildTarget target)
        {
            BuildPath = buildPath;
            Target = target;
        }
    }
}