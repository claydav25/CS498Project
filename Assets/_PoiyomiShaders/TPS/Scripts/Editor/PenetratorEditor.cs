using System;
using System.Collections;
using System.Collections.Generic;
using Thry.TPS.Editors;
using UnityEditor;
#if UNITY_2022_1_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEngine;

namespace Thry.TPS
{
    [CustomEditor(typeof(Penetrator))]
    public class PenetratorEditor : TPSComponentEditor
    {
        protected override string TPSComponentName => "Penetrator";
        Penetrator _penetrator;
        public override void OnInspectorGUI()
        {
            if(this.target == null) return;
            EditorGUI.BeginChangeCheck();

            base.OnInspectorGUI();

            if(PrefabStageUtility.GetCurrentPrefabStage() != null) return;

            _penetrator = (Penetrator)target;

            PenetratorSetup.ResolveRoot(_penetrator);
            _penetrator.HandlesOnlyPosition = false;

            using (new SectionScope("3. Settings", 20, new Color(0.0f, 1.0f, 1.0f, 0.5f)))
            {
                GUILayout.Space(5);
                SettingsGUI();
            }

            #if VRC_SDK_VRCSDK3 && !UDON
            using (new SectionScope("5. Contact Senders", 20, new Color(1.0f, 0.0f, 1.0f, 0.5f)))
            {
                GUILayout.Space(5);
            }

            using (new SectionScope("6. Animator", 20, new Color(1.0f, 0.0f, 0.0f, 0.5f)))
            {
                GUILayout.Space(5);
            }
            #endif

            if(EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_component);
            }
        }

        public void OnSceneGUI()
        {

        }
    }
}