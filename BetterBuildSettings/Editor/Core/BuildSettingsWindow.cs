using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BetterBuildSettings.Editor.AddressablesModule;
using BetterBuildSettings.Editor.DefinesModule;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace BetterBuildSettings.Editor.Core
{
    public class BuildSettingsWindow : OdinEditorWindow
    {
        private const string DefaultConfigName = "default";
        private const string ConfigPrefKey = "BBS_ConfigName";

        private string _configName;
        private BuildConfig _config;

        private readonly List<IBuildModule> _modules = new();
        private readonly HashSet<string> _selectedModuleIds = new();

        private IBuildModule _selectedModule;
        private PropertyTree _selectedTree;
        
        private bool _isDirty;
        private string _lastLoadedSnapshotJson;

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
            _configName = EditorPrefs.GetString(ConfigPrefKey, DefaultConfigName);
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

            int currentIndex = Mathf.Max(0, configs.IndexOf(_configName));

            EditorGUI.BeginChangeCheck();

            int newIndex = EditorGUILayout.Popup(
                currentIndex,
                configs.ToArray(),
                EditorStyles.toolbarPopup,
                GUILayout.Width(180)
            );

            if (EditorGUI.EndChangeCheck())
            {
                if (_isDirty)
                {
                    // TODO При смене конфига несохранённые изменения теряются.
                    DiscardChanges();
                }

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
                    GUILayout.Width(160)
                );

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
                bool isChecked = _selectedModuleIds.Contains(module.Id);
                bool isSelected = _selectedModule == module;

                Rect rowRect = GUILayoutUtility.GetRect(220, 24, GUILayout.ExpandWidth(true));

                if (isSelected)
                    EditorGUI.DrawRect(rowRect, new Color(0.24f, 0.36f, 0.52f, 0.35f));

                Rect checkboxRect = new Rect(rowRect.x + 6, rowRect.y + 4, 16, 16);
                Rect labelRect = new Rect(rowRect.x + 30, rowRect.y + 3, rowRect.width - 34, 18);

                GUI.Label(checkboxRect, isChecked ? "☑" : "☐");
                GUI.Label(labelRect, module.DisplayName);

                bool newChecked = GUI.Toggle(checkboxRect, isChecked, GUIContent.none);

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
                
                if (Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition))
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
            Rect rect = GUILayoutUtility.GetRect(
                0,
                48,
                GUILayout.ExpandWidth(true),
                GUILayout.Height(48)
            );

            GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);

            Rect buttonRect = new Rect(
                rect.xMax - 172,
                rect.y + 8,
                160,
                32
            );

            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.0f, 0.8f, 0.3f);


            bool buildClicked = GUI.Button(buttonRect, "BUILD");

            GUI.backgroundColor = oldColor;

            if (buildClicked)
                Build();
        }

        private void LoadConfig()
        {
            NormalizeConfigName();
            EditorPrefs.SetString(ConfigPrefKey, _configName);

            _config = BuildConfigSerializer.LoadOrCreate(_configName);

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
            EditorPrefs.SetString(ConfigPrefKey, _configName);

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
            EditorPrefs.SetString(ConfigPrefKey, _configName);

            _config = new BuildConfig();

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
                Newtonsoft.Json.Formatting.Indented
            );

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
            var config = new BuildConfig();

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

        private void Build()
        {
            SaveConfig();

            var modulesToApply = _modules
                .Where(x => _selectedModuleIds.Contains(x.Id))
                .ToList();

            var buildRoot = Path.Combine("Builds", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Directory.CreateDirectory(buildRoot);
            var buildPath = Path.Combine(buildRoot, "Game.exe");

            var context = new BuildContext(
                buildPath,
                BuildTarget.StandaloneWindows64
            );

            EditorUserBuildSettings.SetBuildLocation(
                BuildTarget.StandaloneWindows64,
                buildPath
            );

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
                    BuildOptions.None
                );
            }
            finally
            {
                for (var i = modulesToApply.Count - 1; i >= 0; i--)
                    modulesToApply[i].Restore(context);
            }
        }

        private void RegisterModules()
        {
            _modules.Add(new AddressablesBuildModule());
            _modules.Add(new DefineModule());
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