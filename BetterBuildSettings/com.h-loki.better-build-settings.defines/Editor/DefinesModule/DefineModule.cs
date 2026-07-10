#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using BetterBuildSettings.Core;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Build;


[Serializable]
public class DefineModule : IBuildModule
{
    public string Id => "Defines";
    public string DisplayName => "Defines";

    [ShowInInspector] 
    [HideLabel] 
    [HideReferenceObjectPicker]
    private DefineModuleConfig _config = new();

    private string _originalDefines;

    public JObject CreateDefaultPayload()
    {
        var config = new DefineModuleConfig
        {
            restoreAfterBuild = true
        };

        return JObject.FromObject(config);
    }

    public void Deserialize(JObject payload)
    {
        _config = payload.ToObject<DefineModuleConfig>() ?? new DefineModuleConfig();
        _config.enabledDefines ??= new List<DefineEntry>();
    }

    public JObject Serialize()
    {
        return JObject.FromObject(_config);
    }

    public void Apply(BuildContext context)
    {
        var namedBuildTarget = GetNamedBuildTarget(context);

        _originalDefines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);

        var result = _config.enabledDefines
            .Where(x => x != null)
            .Select(x => x.value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct()
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();

        PlayerSettings.SetScriptingDefineSymbols(
            namedBuildTarget,
            result
        );
    }

    public void Restore(BuildContext context)
    {
        if (!_config.restoreAfterBuild)
            return;

        if (_originalDefines == null)
            return;

        var namedBuildTarget = GetNamedBuildTarget(context);

        PlayerSettings.SetScriptingDefineSymbols(
            namedBuildTarget,
            _originalDefines
        );
    }

    private static NamedBuildTarget GetNamedBuildTarget(BuildContext context)
    {
        var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(context.Target);
        return NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
    }
}

#endif