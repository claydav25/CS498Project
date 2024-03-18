using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.UIElements;
using Poiyomi.ModularShaderSystem;

#if UNITY_2019_4
using UnityEditor.Experimental.SceneManagement;
#endif

namespace Poi.Tools
{
    public class ModularShadersGeneratorElement : VisualElement
    {
        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if(_isErrored) return;
                _isSelected = value;
                _toggle.SetValueWithoutNotify(_isSelected);
            }
        }

        public ModularShader Shader { get; set; }

        private readonly Toggle _toggle;
        private readonly bool _isErrored;
        public ModularShadersGeneratorElement(ModularShader shader)
        {
            Shader = shader;
            style.flexDirection = FlexDirection.Row;
            _toggle = new Toggle();
            _toggle.RegisterValueChangedCallback(evt => IsSelected = evt.newValue);
            Add(_toggle);
            var label = new Label(Shader.Name);
            label.style.flexGrow = 1;
            Add(label);
            var shaderObject = new UnityEditor.UIElements.ObjectField();
            shaderObject.objectType = typeof(ModularShader);
            shaderObject.value = shader;
            shaderObject.style.minWidth = new StyleLength(new Length(50f, LengthUnit.Percent));
            shaderObject.style.maxWidth = new StyleLength(new Length(50f, LengthUnit.Percent));
            Add(shaderObject);
            var issues = ShaderGenerator.CheckShaderIssues(shader);
            if(issues.Count > 0)
            {
                _isErrored = true;
                _toggle.SetEnabled(false);
                VisualElement element = new VisualElement();
                element.AddToClassList("error");
                element.tooltip = "Modular shader has the following errors: \n -" + string.Join("\n -", issues);
                Add(element);
            }
        }
    }

    [Serializable]
    public class ModularShadersGeneratorWindow : EditorWindow
    {
        [MenuItem("Poi/Modular Shaders Generator")]
        private static void ShowWindow()
        {
            var window = GetWindow<ModularShadersGeneratorWindow>();
            window.titleContent = new GUIContent("Modular Shaders Generator");
            window.Show();
        }

        private VisualElement _root;
        internal List<ModularShadersGeneratorElement> _elements;

        private void OnDestroy()
        {
            UnregisterCallbacks();
        }

        private void CreateGUI()
        {
            _root = rootVisualElement;
            Reload();
        }

        private void Reload()
        {
            _root.Clear();

            var styleSheet = Resources.Load<StyleSheet>("Poi/ModularShadersGeneratorStyle");
            _root.styleSheets.Add(styleSheet);

            var view = new ScrollView(ScrollViewMode.Vertical);
#if UNITY_2020_1_OR_NEWER
            view.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
#else
            view.horizontalScroller.visible = false;
#endif

            var selectButtons = new VisualElement();
            selectButtons.AddToClassList("buttons-area");

            var selectAll = new Button();
            selectAll.text = "Select all";
            selectAll.style.flexGrow = 1;
            selectAll.clicked += () =>
            {
                foreach (var element in _elements)
                    element.IsSelected = true;
            };
            selectButtons.Add(selectAll);

            var deselectAll = new Button();
            deselectAll.text = "Deselect all";
            deselectAll.style.flexGrow = 1;
            deselectAll.clicked += () =>
            {
                foreach (var element in _elements)
                    element.IsSelected = false;
            };
            selectButtons.Add(deselectAll);

            var toggleSelections = new Button();
            toggleSelections.text = "Toggle selections";
            toggleSelections.style.flexGrow = 1;
            toggleSelections.clicked += () =>
            {
                foreach (var element in _elements)
                    element.IsSelected = !element.IsSelected;
            };
            selectButtons.Add(toggleSelections);

            var reloadButton = new Button();
            reloadButton.text = "Refresh";
            reloadButton.style.flexGrow = 1;
            reloadButton.clicked += () =>
            {
                AssetDatabase.Refresh();
                Reload();
            };
            selectButtons.Add(reloadButton);

            view.Add(selectButtons);

            // Load all modular shaders
            _elements = new List<ModularShadersGeneratorElement>();
            foreach (var modularShader in FindAssetsByType<ModularShader>())
            {
                var element = new ModularShadersGeneratorElement(modularShader);
                _elements.Add(element);
                view.Add(element);
            }

            var generateButton = new Button();
            generateButton.style.marginLeft = 6;
            generateButton.style.marginRight = 8;
            generateButton.text = "Generate Shaders";
            generateButton.clicked += GenerateShaders;

            VisualElement destinationsList = SetupDestinationsListView();

            view.Add(destinationsList);
            _root.Add(view);
            view.Add(generateButton);

            UnregisterCallbacks();
            RegisterCallbacks();
        }

        VisualElement SetupDestinationsListView()
        {
#if UNITY_2020_1_OR_NEWER
            ListView destinationsList = new ListView()
            {
                headerTitle = "Destinations",
                reorderable = true,
                showAddRemoveFooter = true,
                reorderMode = ListViewReorderMode.Animated,
                itemsSource = ShaderDestinationManager.Instance.destinations,
                fixedItemHeight = 50,
                showBorder = true,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                makeItem = () => new ShaderDestinationListElement(),
                bindItem = (elem, index) =>
                {
                    (elem as ShaderDestinationListElement).BindListItem(ShaderDestinationManager.Instance.destinations[index]);
                },
                unbindItem = (elem, index) =>
                {
                    (elem as ShaderDestinationListElement).UnbindListItem();
                }
            };
#else
            VisualElement destinationsList = new IMGUIContainer(() =>
            {
                EditorGUILayout.HelpBox("Editing the destinations list isn't supported in Unity 2019. Shaders will go to:\nAssets/_PoiyomiShaders/Shaders/9.0/Toon\nAssets/_PoiyomiShaders/Shaders/9.0/Pro", MessageType.Warning);
            });
#endif
            destinationsList.style.marginLeft = 6;
            destinationsList.style.marginRight = 8;
            destinationsList.style.marginTop = 10;

            return destinationsList;
        }

        void UnregisterCallbacks()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChange;
            EditorSceneManager.sceneOpened -= HandleSceneOpened;
            EditorSceneManager.newSceneCreated -= HandleNewScene;
            PrefabStage.prefabStageOpened -= HandlePrefabSceneOpenOrClose;
            PrefabStage.prefabStageClosing -= HandlePrefabSceneOpenOrClose;
        }

        void RegisterCallbacks()
        {
            EditorApplication.playModeStateChanged += HandlePlayModeStateChange;
            EditorSceneManager.sceneOpened += HandleSceneOpened;
            EditorSceneManager.newSceneCreated += HandleNewScene;
            PrefabStage.prefabStageOpened += HandlePrefabSceneOpenOrClose;
            PrefabStage.prefabStageClosing += HandlePrefabSceneOpenOrClose;
        }

        void HandlePrefabSceneOpenOrClose(PrefabStage obj) => Reload();

        void HandleNewScene(Scene scene, NewSceneSetup setup, NewSceneMode mode)
        {
            if(mode == NewSceneMode.Single)
                Reload();
        }

        void HandleSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if(mode == OpenSceneMode.Single)
                Reload();
        }

        void HandlePlayModeStateChange(PlayModeStateChange obj) => Reload();

        public static void ShowFolderSelector(TextField textField)
        {
            string path = textField.value;
            if(Directory.Exists(path))
            {
                path = Directory.GetParent(path).FullName;
            }
            else
            {
                path = "Assets";
            }
            path = EditorUtility.OpenFolderPanel("Select folder to use", path, "");
            if(path.Length == 0)
                return;

            if(!Directory.Exists(path))
            {
                EditorUtility.DisplayDialog("Error", "The folder does not exist", "Ok");
                return;
            }

            textField.value = path.Replace(Application.dataPath, "Assets");
        }

        internal void GenerateShaders()
        {
            var enabledDestinations = ShaderDestinationManager.Instance.destinations.Where(dest => dest.enabled).ToArray();
            if(enabledDestinations.Length == 0)
            {
                Debug.LogError("Can't generate shaders if no destination folders are set");
                return;
            }

            foreach(ModularShadersGeneratorElement element in _elements.Where(x => x.IsSelected))
            {
                string pathResult = ShaderDestinationManager.Instance.GetDestinationFromShaderName(element.Shader.Name);
                if(string.IsNullOrWhiteSpace(pathResult))
                {
                    EditorUtility.DisplayDialog("Error", $"Couldn't match shader {element.Shader.Name} to any path.", "Ok");
                    return;
                }
                ShaderGenerator.GenerateShader(pathResult, element.Shader);
            }
        }

        private static T[] FindAssetsByType<T>() where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).ToString().Replace("UnityEngine.", "")}");
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                    assets.Add(asset);
            }
            return assets.ToArray();
        }
    }
    public class ModularShadersAutoGen : AssetPostprocessor
    {
#if UNITY_2021_2_OR_NEWER
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
#else
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
#endif
        {
            bool update = false;
            foreach (var importedAssetPath in importedAssets)
            {
                string ext = System.IO.Path.GetExtension(importedAssetPath);
                if (ext == ".poiTemplateCollection" || ext == ".poiTemplate")
                {
                    update = true;
                    break;
                }
            }
            if (update)
            {
                var msgw = Resources.FindObjectsOfTypeAll<ModularShadersGeneratorWindow>().FirstOrDefault();
                if (msgw != null)
                {
                    if (msgw._elements != null && msgw._elements.Count(x => x.IsSelected) > 0)
                    {
                        msgw.GenerateShaders();
                        Thry.ShaderEditor.ReloadActive();
                    }
                }
            }
        }
    }
}