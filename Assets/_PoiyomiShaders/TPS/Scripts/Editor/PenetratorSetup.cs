using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static Thry.TPS.AnimatorHelper;
using static Thry.TPS.Orifice;
#if VRC_SDK_VRCSDK3 && !UDON
using VRC.SDK3.Dynamics.Contact.Components;
using static VRC.SDKBase.VRC_AvatarParameterDriver;
using VRC.SDK3.Avatars.Components;
#endif

namespace Thry.TPS
{
    public class PenetratorSetup
    {

        public static void ResolveRoot(Penetrator penetrator)
        {
            if (penetrator.Root == null)
            {
                bool isAvatarRoot = false;
#if VRC_SDK_VRCSDK3 && !UDON
                isAvatarRoot |= penetrator.transform.GetComponent<VRCAvatarDescriptor>() != null;
#endif
                if(!isAvatarRoot)
                {
                    penetrator.Root = penetrator.transform;
                    penetrator.Renderer = penetrator.Root.GetComponentInChildren<Renderer>(true);
                }
            }
        }
    }
}