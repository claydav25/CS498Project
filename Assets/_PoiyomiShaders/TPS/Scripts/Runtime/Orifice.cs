using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && !UDON
using VRC.SDK3.Dynamics.Contact.Components;
using UnityEditor.Animations;
using UnityEditor;
#endif

namespace Thry.TPS
{
    public enum OrificeType
    {
        Hole,
        Ring
    }

    public class Orifice : TPSComponent
    {
        public float Radius = 0.1f;
        public float Depth = 0.3f; // Max Opening Width
        public OrificeType Type = OrificeType.Hole;

        public Light LightPosition;
        public Light LightNormal;
        public bool UseNormalLight = true;

#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && !UDON
        public VRCContactSender ContactSenderPosition;
        public VRCContactSender ContactSenderNormal;
        public bool DoAnimatorSetup;
        public VRCContactReceiver ContactReceiver_IsPenetrating;
        public VRCContactReceiver ContactReceiver_Width0;
        public VRCContactReceiver ContactReceiver_Width1;
        public bool ScaleBlendshapesByWidth = true;
        public List<ShapekeyConfig> OpeningShapekeys = new List<ShapekeyConfig>();

        public string LayerName_Width;
        public string LayerName_Depth;
        [NonSerialized]
        public AnimatorControllerLayer Layer_Width;
        [NonSerialized]
        public AnimatorControllerLayer Layer_Depth;
#endif

        public string Param_DepthIn => $"TPS_Internal/Orf/{Id}/Depth_In";
        public string Param_Width1In => $"TPS_Internal/Orf/{Id}/Width1_In";
        public string Param_Width2In => $"TPS_Internal/Orf/{Id}/Width2_In";
        public string Param_Depth => $"TPS_Orf_{Id}_Depth";
        public string Param_Width => $"TPS_Orf_{Id}_Width";
        public string Param_IsPenetrating => $"TPS_Orf_{Id}_IsPenetrated";
        public string ClipPath => $"TPS/Orf/{Id}";
        public string FilePrefix => $"TPS_Orf_{Id}";

        static Mesh s_cylinderMesh;

#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && !UDON
        private void OnDrawGizmosSelected() 
        {
            if(s_cylinderMesh == null)
            {
                s_cylinderMesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
            }

            if(Root == null || !ShowHandles || HandlesOnlyPosition)
                return;

            Vector3 globalPosition = Root.TransformPoint(LocalPosition);
            Quaternion globalRotation = Root.rotation * LocalRotation;
            Vector3 forward = globalRotation * Vector3.forward;
            Vector3 middle = globalPosition - forward * Depth / 2;
            // Draw a cylinder at the transform's position
            Gizmos.color = Color.yellow;
            Quaternion cylinderRotation = Quaternion.LookRotation(globalRotation * Vector3.up, globalRotation * Vector3.forward);
            Gizmos.DrawWireMesh(s_cylinderMesh, 0, middle, cylinderRotation, new Vector3(Radius, Depth / 2, Radius));
        }
#endif
    }
}