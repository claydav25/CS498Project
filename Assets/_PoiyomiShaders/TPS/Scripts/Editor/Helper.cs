using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Thry.TPS
{
    public class Helper
    {
        const string VERSION_GUID = "b293613c74a3fef4493e53cba313cfea";

        private static readonly Type[] s_tps_script_types = new Type[] 
        {
            typeof(Thry.TPS.Orifice)
        };

        public static Type[] TPS_SCRIPT_TYPES
        {
            get
            {
                return s_tps_script_types;
            }
        }

        static string s_version = null;
        public static string Version
        {
            get
            {
                if(s_version == null)
                {
                    string path = AssetDatabase.GUIDToAssetPath(VERSION_GUID);
                    s_version = AssetDatabase.LoadAssetAtPath<TextAsset>(path).text;
                }
                return s_version;
            }
        }

        public static void MakeUniqueId(TPSComponent component)
        {
            // Get avatar root and all ids of other TPS Components
            Transform root = GetAvatarRoot(component.transform);
            IEnumerable<string> otherIds = root.GetComponentsInChildren<TPSComponent>(true).
                Where(c => c != component).
                Select(c => c.Id);
            // make sure the id is valid
            string id = component.Id;
            if(string.IsNullOrWhiteSpace(id))
            {
                id = "tps";
            }
            id = id.Replace(" ", "_");
            component.Id = id;
            // return if already unique
            if(!otherIds.Contains(id)) return;
            // match _\d+$ and increment the number
            int i = 0;
            if(Regex.Match(id, @"_\d+$").Success)
            {
                i = int.Parse(Regex.Match(id, @"_\d+$").Value.Substring(1));
                i += 1;
                id = Regex.Replace(id, @"_\d+$", "");
            }
            // make sure the id is unique
            while(otherIds.Contains(id + "_" + i))
            {
                i += 1;
            }
            // set the id
            component.Id = id + "_" + i;
        }

        static string[] GetAllIds(Transform root)
        {
            return root.GetComponentsInChildren<Orifice>(true).Select(o => o.Id).ToArray();
        }

        public static Transform GetAvatarRoot(Transform t)
        {
            Transform avatarRoot = t;
            while(avatarRoot.parent != null)
            {
                #if VRC_SDK_VRCSDK3 && !UDON
                if(avatarRoot.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>() != null)
                {
                    break;
                }
                #endif
                avatarRoot = avatarRoot.parent;
            }
            return avatarRoot;
        }

        public static string FindAvatarDirectory(Transform _avatarRoot)
        {
            string path = AssetDatabase.GetAssetPath(_avatarRoot);
            if (string.IsNullOrEmpty(path) && _avatarRoot.GetComponent<Animator>()) path = AssetDatabase.GetAssetPath(_avatarRoot.GetComponent<Animator>().avatar);
            if (string.IsNullOrEmpty(path) && _avatarRoot != null) path = AssetDatabase.GetAssetPath(_avatarRoot);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("[TPS] Could not find avatar file path. Using Assets folder. Make sure your avatar is a prefab or your animator has an avatar assigned in the future.");
                return "Assets";
            }
            return Path.GetDirectoryName(path);
        }

        public static string GetTPSDirectory(TPSComponent component, AnimatorController animator)
        {
            return Path.GetDirectoryName(AssetDatabase.GetAssetPath(animator)) + "/TPS_" + component.Id;
        }

        public static void AssertFolderExists(string path)
        {
            // get folder from path
            string folder = path;
            if(Path.HasExtension(folder))
            {
                folder = Path.GetDirectoryName(folder);
            }
            // create folder recursively if it does not exist
            if(!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }
    }

    public class Style
    {
        public static GUIStyle RichtText { get; private set; } = new GUIStyle(EditorStyles.boldLabel) { richText = true };
        public static GUIStyle RichTextCenter { get; private set; } = new GUIStyle(EditorStyles.boldLabel) { richText = true, alignment = TextAnchor.LowerCenter };
    }

    public struct SectionScope : IDisposable
    {
        public SectionScope(string label, float labelHeight, Color color)
        {
            Color prev = GUI.backgroundColor;
            GUI.backgroundColor = color;
            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(label, Style.RichtText, GUILayout.Height(labelHeight));
            GUI.backgroundColor = prev;
            }

        public void Dispose()
        {
            GUILayout.EndVertical();
        }
    }
}
