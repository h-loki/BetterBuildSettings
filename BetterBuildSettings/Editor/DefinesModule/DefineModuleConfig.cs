using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Build;

namespace BetterBuildSettings.Editor.DefinesModule
{
    [Serializable]
    public sealed class DefineModuleConfig
    {
        public bool restoreAfterBuild = true;

        [ValueDropdown(nameof(GetProjectDefines), IsUniqueList = true, DrawDropdownForListElements = false)]
        [ListDrawerSettings(DefaultExpandedState = true)]
        public List<DefineEntry> enabledDefines = new();

        private IEnumerable<DefineEntry> GetProjectDefines()
        {
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup
            );

            var defines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);

            return defines
                .Split(';')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(x => x)
                .Select(x => new DefineEntry(x));
        }
    }
}