#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

[Serializable]
public sealed class AddressablesModule : IBuildModule
{
    public string Id => "addressables";
    public string DisplayName => "Addressables";

    [ShowInInspector]
    [HideLabel]
    [HideReferenceObjectPicker]
    private AddressablesModuleConfig _config = new();

    private readonly Dictionary<string, bool> _originalIncludeInBuildByGroupName = new();

    public JObject CreateDefaultPayload()
    {
        var config = new AddressablesModuleConfig
        {
            restoreAfterBuild = true
        };

        return JObject.FromObject(config);
    }

    public void Deserialize(JObject payload)
    {
        _config = payload.ToObject<AddressablesModuleConfig>() ?? new AddressablesModuleConfig();
        _config.enabledGroups ??= new List<string>();
    }

    public JObject Serialize() =>
        JObject.FromObject(_config);

    public void Apply(BuildContext context)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
            throw new InvalidOperationException("Addressables settings not found.");

        var existingGroupNames = settings.groups
            .Where(x => x != null)
            .Select(x => x.Name)
            .ToHashSet(StringComparer.Ordinal);

        var missingGroupNames = _config.enabledGroups
            .Where(x => !existingGroupNames.Contains(x))
            .ToArray();

        if (missingGroupNames.Length > 0)
        {
            throw new InvalidOperationException(
                "Addressables config contains missing groups: " +
                string.Join(", ", missingGroupNames)
            );
        }

        var enabledGroupNames = new HashSet<string>(
            _config.enabledGroups ?? Enumerable.Empty<string>(),
            StringComparer.Ordinal
        );

        _originalIncludeInBuildByGroupName.Clear();

        foreach (var group in settings.groups)
        {
            if (group == null)
                continue;

            var schema = group.GetSchema<BundledAssetGroupSchema>();
            if (schema == null)
                continue;

            _originalIncludeInBuildByGroupName[group.Name] = schema.IncludeInBuild;

            schema.IncludeInBuild = enabledGroupNames.Contains(group.Name);
        }

        settings.SetDirty(
            AddressableAssetSettings.ModificationEvent.BatchModification,
            null,
            true
        );
        AddressableAssetSettings.CleanPlayerContent();
        AddressableAssetSettings.BuildPlayerContent();
    }

    public void Restore(BuildContext context)
    {
        if (!_config.restoreAfterBuild)
            return;

        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
            return;

        foreach (var group in settings.groups)
        {
            if (group == null)
                continue;

            if (!_originalIncludeInBuildByGroupName.TryGetValue(group.Name, out var originalValue))
                continue;

            var schema = group.GetSchema<BundledAssetGroupSchema>();
            if (schema == null)
                continue;

            schema.IncludeInBuild = originalValue;
        }

        settings.SetDirty(
            AddressableAssetSettings.ModificationEvent.BatchModification,
            null,
            true
        );

        _originalIncludeInBuildByGroupName.Clear();
    }
}


#endif