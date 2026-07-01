using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


public static class BuildConfigSerializer
{
    private const string FolderPath = "Assets/BuildConfigs";

    public static BuildConfig LoadOrCreate(string configName)
    {
        var path = GetPath(configName);

        if (!File.Exists(path))
            return new BuildConfig();

        var json = File.ReadAllText(path);

        return Newtonsoft.Json.JsonConvert.DeserializeObject<BuildConfig>(json)
               ?? new BuildConfig();
    }

    public static void Save(string configName, BuildConfig config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        var path = GetPath(configName);

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(
            config,
            Newtonsoft.Json.Formatting.Indented
        );

        File.WriteAllText(path, json);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    public static List<string> GetExistingConfigNames()
    {
        Directory.CreateDirectory(FolderPath);

        return Directory
            .GetFiles(FolderPath, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .OrderBy(x => x)
            .ToList();
    }

    private static string GetPath(string configName)
    {
        if (string.IsNullOrWhiteSpace(configName))
            throw new ArgumentException("Config name cannot be empty.", nameof(configName));

        var invalidChars = Path.GetInvalidFileNameChars();
        if (configName.IndexOfAny(invalidChars) >= 0)
            throw new ArgumentException($"Config name contains invalid characters: {configName}", nameof(configName));

        return Path.Combine(FolderPath, $"{configName}.json");
    }
}