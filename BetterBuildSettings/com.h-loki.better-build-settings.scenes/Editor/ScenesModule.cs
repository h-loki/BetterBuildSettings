#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEditor;

[Serializable]
public sealed class ScenesModule : IBuildModule
{
    public string Id => "scenes";
    public string DisplayName => "Scenes";

    [ShowInInspector]
    [HideLabel]
    [HideReferenceObjectPicker]
    private ScenesModuleConfig _config = new();

    private EditorBuildSettingsScene[] _originalScenes;

    public JObject CreateDefaultPayload()
    {
        var config = new ScenesModuleConfig
        {
            restoreAfterBuild = true,
            enabledScenes = EditorBuildSettings.scenes
                .Where(x => x != null && x.enabled)
                .Select(x => x.path)
                .ToList()
        };

        return JObject.FromObject(config);
    }

    public void Deserialize(JObject payload)
    {
        _config = payload.ToObject<ScenesModuleConfig>() ?? new ScenesModuleConfig();
        _config.enabledScenes ??= new List<string>();
    }

    public JObject Serialize() =>
        JObject.FromObject(_config);

    public void Apply(BuildContext context)
    {
        _originalScenes = EditorBuildSettings.scenes
            .Select(x => new EditorBuildSettingsScene(x.path, x.enabled))
            .ToArray();

        var existingScenePaths = GetAvailableScenePaths()
            .ToHashSet(StringComparer.Ordinal);

        var missingScenePaths = _config.enabledScenes
            .Where(x => !existingScenePaths.Contains(x))
            .ToArray();

        if (missingScenePaths.Length > 0)
        {
            throw new InvalidOperationException(
                "Scenes config contains missing scenes: " +
                string.Join(", ", missingScenePaths)
            );
        }

        var enabledScenePaths = new HashSet<string>(
            _config.enabledScenes ?? Enumerable.Empty<string>(),
            StringComparer.Ordinal
        );

        EditorBuildSettings.scenes = enabledScenePaths
            .Select(path => new EditorBuildSettingsScene(path, true))
            .ToArray();
    }

    public void Restore(BuildContext context)
    {
        if (!_config.restoreAfterBuild)
            return;

        if (_originalScenes == null)
            return;

        EditorBuildSettings.scenes = _originalScenes
            .Select(x => new EditorBuildSettingsScene(x.path, x.enabled))
            .ToArray();

        _originalScenes = null;
    }

    private static IEnumerable<string> GetAvailableScenePaths()
    {
        return AssetDatabase
            .FindAssets("t:Scene")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .OrderBy(path => path, StringComparer.Ordinal);
    }
}
#endif