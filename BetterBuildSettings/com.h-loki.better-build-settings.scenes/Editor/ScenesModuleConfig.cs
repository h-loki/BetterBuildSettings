#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;

[Serializable]
public sealed class ScenesModuleConfig
{
    public bool restoreAfterBuild = true;

    [ValueDropdown(nameof(GetScenePaths), IsUniqueList = true)]
    public List<string> enabledScenes = new();

    private IEnumerable<string> GetScenePaths()
    {
        return AssetDatabase
            .FindAssets("t:Scene")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .OrderBy(path => path, StringComparer.Ordinal);
    }
}
#endif