using System;
using UnityEngine;
#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && !UDON
using VRC.SDK3.Dynamics.Contact.Components;
using UnityEditor.Animations;
using UnityEditor;
#endif

namespace Thry.TPS
{
    public class Penetrator : TPSComponent
    {
        public float Radius = 0.05f;
        public float Length = 0.3f;

#if UNITY_EDITOR && VRC_SDK_VRCSDK3 && !UDON

        private void OnDrawGizmosSelected() 
        {

            if(Root == null || !ShowHandles || HandlesOnlyPosition)
                return;

            Vector3 globalPosition = Root.TransformPoint(LocalPosition);
            Quaternion globalRotation = Root.rotation * LocalRotation;
            Vector3 forward = globalRotation * Vector3.forward;
            Vector3 middle = globalPosition + forward * Length / 2;
            // Draw a cylinder at the transform's position
            Gizmos.color = Color.yellow;
            Quaternion cylinderRotation = Quaternion.LookRotation(globalRotation * Vector3.up, globalRotation * Vector3.forward);

            Handles.color = Color.yellow;
            HandlesUtil.DrawWireCapsule(middle, cylinderRotation, Length, Radius);
        }
#endif
    }
}