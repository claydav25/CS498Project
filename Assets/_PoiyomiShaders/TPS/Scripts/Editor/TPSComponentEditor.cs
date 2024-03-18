using System.IO;
using UnityEditor;
#if UNITY_2022_1_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEngine;

namespace Thry.TPS.Editors
{
    public class TPSComponentEditor : Editor
    {
        protected bool _isMissingConfigurations = false;
    	bool _showDefaultInspector = false;
        protected bool _showDebugInformation = false;

        protected TPSComponent _component;
        protected Transform _avatarRoot;
        protected string _avatarDirectory;

        protected const float INPUT_HEIGHT = 20;
        protected const float BUTTON_HEIGHT = 30;

        protected bool _isPrefabEditing;

        protected virtual string TPSComponentName { get; } = "TPS Component";

        public override void OnInspectorGUI()
        {
            // Don't do anything while in prefab edit mode

            _component = (TPSComponent)target;
            _avatarRoot = Helper.GetAvatarRoot(_component.transform);
            _isMissingConfigurations = false;
            _isPrefabEditing = PrefabStageUtility.GetCurrentPrefabStage() != null
                || Selection.activeGameObject.scene.isLoaded == false;

            _showDefaultInspector = EditorGUILayout.BeginFoldoutHeaderGroup(_showDefaultInspector, "Show Default Inspector");
            if(_showDefaultInspector)
            {
                DrawDefaultInspector();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            _showDebugInformation = !EditorGUILayout.Toggle("Simple UI", !_showDebugInformation, GUILayout.Height(INPUT_HEIGHT));

            EditorGUILayout.LabelField($"<color=#ff00ff><size=25> {TPSComponentName} </size></color>", Style.RichTextCenter, GUILayout.Height(30));
            EditorGUILayout.LabelField($"<color=#ff80ff><size=20> Thry's Penetration System </size><size=12>v{Helper.Version}</size></color>", Style.RichTextCenter, GUILayout.Height(25));

            
            if(_isPrefabEditing)
            {
                EditorGUILayout.HelpBox("Editing prefabs is not supported!", MessageType.Error);
                return;
            }

            using (new SectionScope("1. References", 20, new Color(1.0f, 1.0f, 0.0f, 0.5f)))
            {
                GUILayout.Space(5);
                IdGUI();
                RootGUI();
                GeneralSetup.ValidateMasterTransform(_component);
                OtherReferencesGUI();
            }

            using (new SectionScope("2. Placement", 20, new Color(0.0f, 1.0f, 0.0f, 0.5f)))
            {
                GUILayout.Space(5);
                GUIGizmosPosition();
            }
        }

        protected virtual void SettingsGUI()
        {
            if(_showDebugInformation || _component.Channel != 0)
                ChannelGUI();
        }

        void GUIGizmosPosition()
        {
            if(GUILayout.Button(_component.ShowHandles ? "Finish" : "Edit Position", GUILayout.Height(BUTTON_HEIGHT)))
            {
                _component.ShowHandles = !_component.ShowHandles;
                _component.HandlesOnlyPosition = true;
                Tools.current = _component.ShowHandles ? Tool.None : Tool.Move;
                SceneView.RepaintAll();
            }
        }

        protected void GUIGizmosAll()
        {
            if(GUILayout.Button(_component.ShowHandles ? "Finish" : "Edit Bounds", GUILayout.Height(BUTTON_HEIGHT)))
            {
                _component.ShowHandles = !_component.ShowHandles;
                _component.HandlesOnlyPosition = false;
                Tools.current = _component.ShowHandles ? Tool.None : Tool.Move;
                SceneView.RepaintAll();
            }
        }

        void RootGUI()
        {
            EditorGUI.BeginChangeCheck();
            Transform newRoot = (Transform)EditorGUILayout.ObjectField("Root / Bone", _component.Root, typeof(Transform), true, GUILayout.Height(INPUT_HEIGHT));
            if(_component.Root == null)
            {
                EditorGUILayout.HelpBox("Root / Bone is required!", MessageType.Error);
                _isMissingConfigurations = true;
            }
            // check that root is somewhere under the avatar root
            if(_avatarRoot != null && _component.Root != null && !_component.Root.IsChildOf(_avatarRoot))
            {
                EditorGUILayout.HelpBox("Root / Bone must be somewhere under the avatar root!", MessageType.Error);
                _isMissingConfigurations = true;
            }
            if(EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_component, "Changed Root");
                _component.Root = newRoot;
                _component.IsAnimatorDirty = true;
            }
        }

        static readonly string[] CHANNEL_NAMES = new string[]{ "DPS Channel 1", "Default", "Channel 1", "Channel 2", "Channel 3", "Channel 4", "Channel 5", "Channel 6", "Channel 7", "Channel 8", "Channel 9", "Channel 10", "Channel 11", "Channel 12", "Channel 13", "Channel 14", "Channel 15", "Channel 16", "Channel 17" };
        void ChannelGUI()
        {
            EditorGUI.BeginChangeCheck();
            //int newChannel = EditorGUILayout.Popup("Channel", _component.Channel + 1, CHANNEL_NAMES, GUILayout.Height(INPUT_HEIGHT));
            if(EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_component, "Changed Channel");
                //_component.Channel = (TPSComponent.TPSChannel)newChannel-1;
                _component.IsAnimatorDirty = true;
            }
            if((int)_component.Channel == -1)
            {
                EditorGUILayout.HelpBox("DPS Channel 1 is deprecated! It might be removed in the future and animator functions will not work. Please use the new channels!", MessageType.Warning);
            }
        }

        protected void RendererGUI()
        {
            EditorGUI.BeginChangeCheck();
            Renderer newRenderer = (Renderer)EditorGUILayout.ObjectField("Renderer", _component.Renderer, typeof(Renderer), true, GUILayout.Height(INPUT_HEIGHT));
            if(EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_component, "Changed Renderer");
                _component.Renderer = newRenderer;
                _component.IsAnimatorDirty = true;
            }
        }

        void IdGUI()
        {
            if(string.IsNullOrEmpty(_component.Id))
            {
                _component.Id = (_component is Orifice) ? "tps_orifice_0" : "tps_penetrator_0";
                Helper.MakeUniqueId(_component);
            }

            EditorGUI.BeginChangeCheck();
            string newId = EditorGUILayout.DelayedTextField("Id", _component.Id, GUILayout.Height(INPUT_HEIGHT));
            if(EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_component, "Changed Id");
                _component.Id = newId;
                Helper.MakeUniqueId(_component);
                _component.IsAnimatorDirty = true;
            }
        }

        protected virtual void OtherReferencesGUI()
        {

        }
    }
}