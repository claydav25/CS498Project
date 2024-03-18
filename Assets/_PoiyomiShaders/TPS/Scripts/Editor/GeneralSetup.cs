using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Thry.TPS
{
    public class GeneralSetup
    {
        const float ORF_HOLE_RANGE_ID = 0.41f;
        const float ORF_RING_RANGE_ID = 0.42f;
        const float ORF_NORM_RANGE_ID = 0.45f;

        const float ORF_RANGE_ID_MIN = 0.452f;
        const float ORF_RANGE_ID_STEP = 0.002f;

        const float ORF_HOLE_RANGE_DPS_CHANNEL = 0.43f;
        const float ORF_RING_RANGE_DPS_CHANNEL = 0.44f;
        const float ORF_NORM_RANGE_DPS_CHANNEL = 0.46f;

        public const string CONTACT_ORF_ROOT = "TPS_Orf_Root";
        public const string CONTACT_ORF_NORM = "TPS_Orf_Norm";

        public const string CONTACT_PEN_PENETRATING = "TPS_Pen_Penetrating";
        public const string CONTACT_PEN_WIDTH = "TPS_Pen_Width";

        public static void ValidateMasterTransform(TPSComponent component)
        {
            if(component.Root == null || string.IsNullOrWhiteSpace(component.Id)) return;
            if(component.MasterTransform == null)
            {
                component.MasterTransform = new GameObject(component.Id).transform;
            }
            // Affirm parent, but not part of the prefab
            component.MasterTransform.parent = component.Root;
            // Affirm position
            component.MasterTransform.localPosition = component.LocalPosition;
            component.MasterTransform.localRotation = component.LocalRotation;
            component.MasterTransform.localScale = Vector3.one;
            // Affirm name
            component.MasterTransform.name = component.Id;
        }

        public static float GetOrificeRangeId(int channel, OrificeType type)
        {
            switch(type)
            {
                case OrificeType.Hole:
                    return GetHoleRangeId(channel);
                case OrificeType.Ring:
                    return GetRingRangeId(channel);
            }
            return 0;
        }

        static float GetChanneledRange(int channel)
        {
            return (channel - 1) * ORF_RANGE_ID_STEP + ORF_RANGE_ID_MIN;
        }

        public static float GetHoleRangeId(int channel)
        {
            return channel == 0 ? ORF_HOLE_RANGE_ID : (channel == -1 ? ORF_HOLE_RANGE_DPS_CHANNEL : GetChanneledRange(channel));
        }

        public static float GetRingRangeId(int channel)
        {
            return channel == 0 ? ORF_RING_RANGE_ID : (channel == -1 ? ORF_RING_RANGE_DPS_CHANNEL : GetChanneledRange(channel));
        }

        public static float GetNormRangeId(int channel)
        {
            return channel == 0 ? ORF_NORM_RANGE_ID : (channel == -1 ? ORF_NORM_RANGE_DPS_CHANNEL : GetChanneledRange(channel));
        }

        public static Color GetOrificeColor(int channel, int type)
        {
            if(channel <= 0) return Color.black;
            int check = 64;
            int typeInt = (int)type << 4;
            float alpha = (check + typeInt) / 255f;
            return new Color(0, 0, 0, alpha);
        }
    }
}