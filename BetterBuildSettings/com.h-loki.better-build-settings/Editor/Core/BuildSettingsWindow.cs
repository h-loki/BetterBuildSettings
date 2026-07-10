using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BetterBuildSettings.Core.Output;
using BetterBuildSettings.Core.Output.Naming;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace BetterBuildSettings.Core
{
    public sealed class BuildSettingsWindow : OdinEditorWindow
    {
        private const string DefaultConfigName = "default";
        private const string ConfigPrefKeyPrefix = "BBS_ConfigName_";

        private string _configName;
        private BuildConfig _config;

        private readonly List<IBuildModule> _modules = new();
        private readonly HashSet<string> _selectedModuleIds = new();

        private IBuildModule _selectedModule;
        private PropertyTree _selectedTree;

        private bool _isDirty;
        private string _lastLoadedSnapshotJson;
        
        private string _pendingBuildFolderPatternInsert;
        private string _pendingExecutableNamePatternInsert;

        private string _newConfigName = "";
        private bool _showCreateConfig;
    

        [MenuItem("Tools/Build Settings")]
        public static void Open()
        {
            var window = GetWindow<BuildSettingsWindow>();
            window.titleContent = new GUIContent("Better Build Settings");
            window.minSize = new Vector2(760, 420);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _configName = EditorPrefs.GetString(
                GetProjectScopedConfigPrefKey(),
                DefaultConfigName);

            LoadConfig();
        }

        protected override void OnDisable()
        {
            _selectedTree?.Dispose();
            _selectedTree = null;

            base.OnDisable();
        }

        protected override void OnImGUI()
        {
            DrawTopBar();

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));

            DrawLeftPanel();
            DrawRightPanel();

            EditorGUILayout.EndHorizontal();

            DrawBottomBar();
        }

        private void DrawTopBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label("Profile", GUILayout.Width(45));

            var configs = BuildConfigSerializer.GetExistingConfigNames();

            if (configs.Count == 0)
                configs.Add(DefaultConfigName);

            var currentIndex = Mathf.Max(0, configs.IndexOf(_configName));

            EditorGUI.BeginChangeCheck();

            var newIndex = EditorGUILayout.Popup(
                currentIndex,
                configs.ToArray(),
                EditorStyles.toolbarPopup,
                GUILayout.Width(180));

            if (EditorGUI.EndChangeCheck())
            {
                if (_isDirty)
                    DiscardChanges();

                _configName = configs[newIndex];
                LoadConfig();
            }

            GUI.enabled = _isDirty;

            if (GUILayout.Button("Update Config", EditorStyles.toolbarButton, GUILayout.Width(100)))
                SaveConfig();

            if (GUILayout.Button("Revert", EditorStyles.toolbarButton, GUILayout.Width(70)))
                DiscardChanges();

            GUI.enabled = true;

            GUILayout.Space(12);

            if (GUILayout.Button("+ New", EditorStyles.toolbarButton, GUILayout.Width(60)))
                _showCreateConfig = !_showCreateConfig;

            if (_showCreateConfig)
            {
                _newConfigName = GUILayout.TextField(
                    _newConfigName,
                    EditorStyles.toolbarTextField,
                    GUILayout.Width(160));

                GUI.enabled = !string.IsNullOrWhiteSpace(_newConfigName);

                if (GUILayout.Button("Create", EditorStyles.toolbarButton, GUILayout.Width(70)))
                    CreateNewConfig();

                GUI.enabled = true;
            }

            GUILayout.FlexibleSpace();

            if (_isDirty)
                GUILayout.Label("Unsaved changes", EditorStyles.miniLabel);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(240));

            GUILayout.Label("Build Modules", EditorStyles.boldLabel);

            foreach (var module in _modules)
            {
                var isChecked = _selectedModuleIds.Contains(module.Id);
                var isSelected = _selectedModule == module;

                var rowRect = GUILayoutUtility.GetRect(220, 24, GUILayout.ExpandWidth(true));

                if (isSelected)
                    EditorGUI.DrawRect(rowRect, new Color(0.24f, 0.36f, 0.52f, 0.35f));

                var checkboxRect = new Rect(rowRect.x + 6, rowRect.y + 4, 16, 16);
                var labelRect = new Rect(rowRect.x + 30, rowRect.y + 3, rowRect.width - 34, 18);

                GUI.Label(checkboxRect, isChecked ? "☑" : "☐");
                GUI.Label(labelRect, module.DisplayName);

                var newChecked = GUI.Toggle(checkboxRect, isChecked, GUIContent.none);

                if (newChecked != isChecked)
                {
                    if (newChecked)
                        _selectedModuleIds.Add(module.Id);
                    else
                        _selectedModuleIds.Remove(module.Id);

                    SelectModule(module);
                    MarkDirty();
                    Repaint();
                }

                if (Event.current.type == EventType.MouseDown &&
                    labelRect.Contains(Event.current.mousePosition))
                {
                    SelectModule(module);
                    Event.current.Use();
                    Repaint();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawRightPanel()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical();

            if (_selectedModule == null)
            {
                EditorGUILayout.HelpBox("Select module.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            GUILayout.Label(_selectedModule.DisplayName, EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(!_selectedModuleIds.Contains(_selectedModule.Id)))
            {
                _selectedTree?.Draw(false);
            }

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
                MarkDirty();
        }

        private void DrawBottomBar()
        {
            DrawOutputSettings();

            var rect = GUILayoutUtility.GetRect(
                0,
                48,
                GUILayout.ExpandWidth(true),
                GUILayout.Height(48));

            GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);

            var buttonRect = new Rect(
                rect.xMax - 172,
                rect.y + 8,
                160,
                32);

            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.0f, 0.8f, 0.3f);

            var buildClicked = GUI.Button(buttonRect, "BUILD");

            GUI.backgroundColor = oldColor;

            if (buildClicked)
                Build();
        }

        private void DrawOutputSettings()
        {
            if (_config.Output == null)
                _config.Output = new BuildOutputSettings();

            ApplyPendingOutputPatternInserts();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Build Output", EditorStyles.boldLabel);

            _config.Output.AskForOutputRootBeforeBuild =
                EditorGUILayout.Toggle(
                    "Ask Folder Before Build",
                    _config.Output.AskForOutputRootBeforeBuild);

            using (new EditorGUILayout.HorizontalScope())
            {
                _config.Output.OutputRoot =
                    EditorGUILayout.TextField(
                        "Output Root",
                        _config.Output.OutputRoot);

                if (GUILayout.Button("Browse", GUILayout.Width(80)))
                {
                    var selected = EditorUtility.OpenFolderPanel(
                        "Select build output folder",
                        _config.Output.OutputRoot,
                        "");

                    if (!string.IsNullOrWhiteSpace(selected))
                        _config.Output.OutputRoot = selected;
                }
            }
        
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.SetNextControlName("BBS_BuildFolderPattern");

                _config.Output.BuildFolderPattern =
                    EditorGUILayout.TextField(
                        "Build Folder",
                        _config.Output.BuildFolderPattern);

                if (GUILayout.Button("Variables", GUILayout.Width(80)))
                {
                    ClearTextFieldFocus();

                    ShowTokenMenu(token =>
                    {
                        _pendingBuildFolderPatternInsert = token;
                        ClearTextFieldFocus();
                        Repaint();
                    });
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.SetNextControlName("BBS_ExecutableNamePattern");

                _config.Output.ExecutableNamePattern =
                    EditorGUILayout.TextField(
                        "Executable Name",
                        _config.Output.ExecutableNamePattern);

                if (GUILayout.Button("Variables", GUILayout.Width(80)))
                {
                    ClearTextFieldFocus();

                    ShowTokenMenu(token =>
                    {
                        _pendingExecutableNamePatternInsert = token;
                        ClearTextFieldFocus();
                        Repaint();
                    });
                }
            }

            DrawOutputPreview();

            EditorGUILayout.Space();

            if (EditorGUI.EndChangeCheck())
                MarkDirty();
        }

        private void DrawOutputPreview()
        {
            var context = new BuildContext(
                _config,
                _configName,
                BuildTarget.StandaloneWindows64,
                DateTime.Now);

            var output = _config.Output;

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

            var root = string.IsNullOrWhiteSpace(output.OutputRoot)
                ? "Builds"
                : output.OutputRoot;

            var preview = Path.Combine(root, folderName, executableName);

            EditorGUILayout.HelpBox(
                "Preview: " + preview,
                MessageType.None);
        }
    
        private void ShowTokenMenu(Action<string> insert)
        {
            var menu = new GenericMenu();

            var descriptors = BuildNameTokenRegistry.GetTokenDescriptors();

            if (descriptors == null || descriptors.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("No variables registered"));
                menu.ShowAsContext();
                return;
            }

            foreach (var descriptor in descriptors)
            {
                var tokenText = descriptor.Usage;

                menu.AddItem(
                    new GUIContent($"{descriptor.Usage}    {descriptor.Description}"),
                    false,
                    () => insert(tokenText));
            }

            menu.ShowAsContext();
        }

        private string GetProjectScopedConfigPrefKey()
        {
            var projectPath = Path.GetFullPath(Application.dataPath)
                .Replace("\\", "/")
                .ToLowerInvariant();

            return ConfigPrefKeyPrefix + StableHash(projectPath);
        }

        private static string StableHash(string value)
        {
            unchecked
            {
                var hash = 23;

                for (var i = 0; i < value.Length; i++)
                    hash = hash * 31 + value[i];

                return hash.ToString("X8");
            }
        }
        
        private void ClearTextFieldFocus()
        {
            GUI.FocusControl(null);
            EditorGUIUtility.editingTextField = false;
            GUIUtility.keyboardControl = 0;
        }
        
        private void ApplyPendingOutputPatternInserts()
        {
            if (_config?.Output == null)
                return;

            if (!string.IsNullOrEmpty(_pendingBuildFolderPatternInsert))
            {
                _config.Output.BuildFolderPattern =
                    (_config.Output.BuildFolderPattern ?? string.Empty) +
                    _pendingBuildFolderPatternInsert;

                _pendingBuildFolderPatternInsert = null;
                MarkDirty();
            }

            if (!string.IsNullOrEmpty(_pendingExecutableNamePatternInsert))
            {
                _config.Output.ExecutableNamePattern =
                    (_config.Output.ExecutableNamePattern ?? string.Empty) +
                    _pendingExecutableNamePatternInsert;

                _pendingExecutableNamePatternInsert = null;
                MarkDirty();
            }
        }
    
        private void LoadConfig()
        {
            NormalizeConfigName();
            EditorPrefs.SetString(GetProjectScopedConfigPrefKey(), _configName);

            _config = BuildConfigSerializer.LoadOrCreate(_configName);

            if (_config.Output == null)
                _config.Output = new BuildOutputSettings();

            _modules.Clear();
            _selectedModuleIds.Clear();

            RegisterModules();

            foreach (var module in _modules)
            {
                var stored = _config.Modules.FirstOrDefault(x => x.ModuleId == module.Id);

                if (stored == null || stored.JsonPayload == null)
                {
                    module.Deserialize(module.CreateDefaultPayload());
                    continue;
                }

                if (stored.Enabled)
                    _selectedModuleIds.Add(module.Id);

                module.Deserialize(stored.JsonPayload);
            }

            SelectModule(_modules.FirstOrDefault());

            CaptureSnapshot();
        }

        private void SaveConfig()
        {
            NormalizeConfigName();
            EditorPrefs.SetString(GetProjectScopedConfigPrefKey(), _configName);

            _config = CreateCurrentConfigSnapshot();

            BuildConfigSerializer.Save(_configName, _config);

            CaptureSnapshot();

            Debug.Log($"Updated build config: {_configName}");
        }

        private void CreateNewConfig()
        {
            var sanitizedName = SanitizeConfigName(_newConfigName);

            if (BuildConfigSerializer.GetExistingConfigNames().Contains(sanitizedName))
            {
                Debug.LogWarning($"Build config already exists: {sanitizedName}");
                return;
            }

            _configName = sanitizedName;
            EditorPrefs.SetString(GetProjectScopedConfigPrefKey(), _configName);

            _config = new BuildConfig
            {
                Output = new BuildOutputSettings()
            };

            _modules.Clear();
            _selectedModuleIds.Clear();

            RegisterModules();

            foreach (var module in _modules)
            {
                module.Deserialize(module.CreateDefaultPayload());

                _config.Modules.Add(new ModuleConfig
                {
                    ModuleId = module.Id,
                    Enabled = false,
                    JsonPayload = module.Serialize()
                });
            }

            SelectModule(_modules.FirstOrDefault());

            BuildConfigSerializer.Save(_configName, CreateCurrentConfigSnapshot());

            _newConfigName = "";
            _showCreateConfig = false;

            CaptureSnapshot();
            Repaint();
        }

        private void CaptureSnapshot()
        {
            _lastLoadedSnapshotJson = Newtonsoft.Json.JsonConvert.SerializeObject(
                CreateCurrentConfigSnapshot(),
                Newtonsoft.Json.Formatting.Indented);

            _isDirty = false;
        }

        private void MarkDirty()
        {
            _isDirty = true;
        }

        public override void DiscardChanges()
        {
            LoadConfig();
            _isDirty = false;
        }

        private BuildConfig CreateCurrentConfigSnapshot()
        {
            var config = new BuildConfig
            {
                Output = CloneOutputSettings(_config.Output)
            };

            foreach (var module in _modules)
            {
                config.Modules.Add(new ModuleConfig
                {
                    ModuleId = module.Id,
                    Enabled = _selectedModuleIds.Contains(module.Id),
                    JsonPayload = module.Serialize()
                });
            }

            return config;
        }

        private BuildOutputSettings CloneOutputSettings(BuildOutputSettings source)
        {
            source ??= new BuildOutputSettings();

            return new BuildOutputSettings
            {
                AskForOutputRootBeforeBuild = source.AskForOutputRootBeforeBuild,
                OutputRoot = source.OutputRoot,
                BuildFolderPattern = source.BuildFolderPattern,
                ExecutableNamePattern = source.ExecutableNamePattern
            };
        }

        private void Build()
        {
            SaveConfig();

            var modulesToApply = _modules
                .Where(x => _selectedModuleIds.Contains(x.Id))
                .ToList();

            var target = BuildTarget.StandaloneWindows64;

            var context = new BuildContext(
                _config,
                _configName,
                target,
                DateTime.Now);

            if (!BuildOutputResolver.TryResolve(context))
                return;

            SaveConfig();

            EditorUserBuildSettings.SetBuildLocation(
                target,
                context.BuildPath);

            try
            {
                foreach (var module in modulesToApply)
                    module.Apply(context);

                BuildPipeline.BuildPlayer(
                    EditorBuildSettings.scenes
                        .Where(x => x.enabled)
                        .Select(x => x.path)
                        .ToArray(),
                    context.BuildPath,
                    context.Target,
                    BuildOptions.None);
            }
            finally
            {
                for (var i = modulesToApply.Count - 1; i >= 0; i--)
                    modulesToApply[i].Restore(context);
            }
        }

        private void RegisterModules()
        {
            foreach (var module in BuildModuleRegistry.CreateModules())
                _modules.Add(module);
        }

        private void SelectModule(IBuildModule module)
        {
            _selectedTree?.Dispose();
            _selectedTree = null;

            _selectedModule = module;

            if (_selectedModule != null)
                _selectedTree = PropertyTree.Create(_selectedModule);
        }

        private void NormalizeConfigName()
        {
            _configName = SanitizeConfigName(_configName);
        }

        private string SanitizeConfigName(string value)
        {
            value = string.IsNullOrWhiteSpace(value)
                ? DefaultConfigName
                : value.Trim();

            foreach (var c in Path.GetInvalidFileNameChars())
                value = value.Replace(c.ToString(), "");

            return string.IsNullOrWhiteSpace(value)
                ? DefaultConfigName
                : value;
        }
    }
}