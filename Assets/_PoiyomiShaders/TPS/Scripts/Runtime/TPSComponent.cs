using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if VRC_NEW_HOOK_API
using VRC.SDKBase;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Thry.TPS
{
    public abstract class TPSComponent : MonoBehaviour
#if VRC_NEW_HOOK_API
    , IEditorOnly
#endif
    {
        public enum TPSChannel
        {
            DPSChannel1,
            Default,
            Channel1,
            Channel2,
            Channel3,
            Channel4,
            Channel5,
            Channel6,
            Channel7,
            Channel8,
            Channel9,
            Channel10,
            Channel11,
            Channel12,
            Channel13,
            Channel14,
            Channel15,
            Channel16,
            Channel17
        }

        const string VersionTxtGUID = "b293613c74a3fef4493e53cba313cfea";
        static string _tpsVersion;
        public static string TPSVersion
        {
            get
            {
                if(string.IsNullOrWhiteSpace(_tpsVersion))
                {
                    #if UNITY_EDITOR
                    try
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(VersionTxtGUID);
                        string versionText = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath).text;
                        _tpsVersion = versionText;
                    }
                    catch(Exception ex)
                    {
                        Debug.LogError("TPS: Failed to load VERSION.txt");
                        Debug.LogException(ex);
                        _tpsVersion = "Unknown Version";
                    }
                    #else
                    _tpsVersion = "Unknown Version";
                    #endif
                }

                return _tpsVersion;
            }
        }

        public string AnimatorVersion;
        public Transform Root;
        public Renderer Renderer;
        public string Id;
        public Transform MasterTransform; // used to parent lights etc. to
        public bool IsAnimatorDirty;
        public TPSChannel Channel;

        public Vector3 LocalPosition;
        public Quaternion LocalRotation = Quaternion.identity;

        public bool SetupAutomatically = true;
        
        [NonSerialized]
        public bool ShowHandles = false;
        public bool HandlesOnlyPosition = false;
    }
}