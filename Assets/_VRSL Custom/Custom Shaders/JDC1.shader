Shader "VRSL/Custom/JDC1"
{
    Properties
    {
        [Toggle] _EnableDMX ("Enable Stream DMX/DMX Control", Int) = 0
        [Toggle] _NineUniverseMode ("Extended Universe Mode", Int) = 0
        _DMXChannel ("Starting DMX Channel", Int) = 1 //raw dmx channel
        [Toggle] _EnableVerticalMode ("Enable Vertical Mode", Int) = 0
        [Toggle] _EnableCompatibilityMode ("Enable Compatibility Mode", Int) = 0
 
        [Toggle] _EnableStrobe ("Enable Strobe", Int) = 0
        [Header(Base Properties)]
        [HDR]_Emission("Base DMX Emission Color", Color) = (1,1,1,1)
        _EmissionMask ("Emission Mask", 2D) = "white" {}
        _GlobalIntensity("Global Intensity", Range(0,1)) = 1
        _FinalIntensity("Final Intensity", Range(0,1)) = 1
        _CurvePlate ("Plate Intensity Multiplier", Range (1,100)) = 20.0
        _CurveBeam ("Beam Intensity Multiplier", Range (1,100)) = 100.0

        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MetallicSmoothness ("Metallic(R) / Smoothness(A) Map", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf StandardDefaultGI
        #include "UnityPBSLighting.cginc"
        #define VRSL_DMX
        #define VRSL_SURFACE

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.5
        struct Input
        {
            float2 uv_MainTex;
            float2 uv_EmissionMask;
            float2 uv_NormalMap;
            float2 uv_MetallicSmoothness;
        };

        #include "Packages/com.acchosen.vr-stage-lighting/Runtime/Shaders/Shared/VRSL-Defines.cginc"
        half _CurveBeam, _CurvePlate;
        #include "Packages/com.acchosen.vr-stage-lighting/Runtime/Shaders/Shared/VRSL-DMXFunctions.cginc"
        //#include "Packages/com.acchosen.vr-stage-lighting/Runtime/Shaders/VRSLDMX.cginc"
        #include "./JDC1-Functions.cginc"

        sampler2D _EmissionMask, _NormalMap, _MetallicSmoothness;

        inline half4 LightingStandardDefaultGI(SurfaceOutputStandard s, half3 viewDir, UnityGI gi)
        { 
            return LightingStandard(s, viewDir, gi);
        }
        inline void LightingStandardDefaultGI_GI(SurfaceOutputStandard s, UnityGIInput data, inout UnityGI gi)
        {
            LightingStandard_GI(s, data, gi);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {             
            int dmx = getDMXChannel();
            // TODO: Channel 1: Coarse Tilt (MSB)

            // TODO: Channel 2: Fine Tilt (LSB)

            // Channel 3: Beam Intensity
            float beamIntensity = getValueAtCoords(dmx+2, _Udon_DMXGridRenderTexture);//ReadDMX(dmx+2, _MainTex); // 0-1
            // TODO: Channel 4: Beam Duration

            // Channel 5: Beam Rate
            float beamStrobe = GetStrobeOutput(dmx+4-6);
            // TODO: Channel 6: Beam Shutter

            // TODO: Channel 7: Special/Control

            // Channel 8: Plate Intensity
            float plateIntensity = getValueAtCoords(dmx+7, _Udon_DMXGridRenderTexture);//ReadDMX(dmx+7, _MainTex);
            // TODO: Channel 9: Plate Flash Duration

            // Channel 10: Plate Flash Rate
            float plateStrobe = GetStrobeOutput(dmx+9-6);
            // TODO: Channel 11: Plate Shutter

            // Channel 12: Plates Red
            float platesRed = getValueAtCoords(dmx+11, _Udon_DMXGridRenderTexture);
            // Channel 13: Plates Green
            float platesGreen = getValueAtCoords(dmx+12, _Udon_DMXGridRenderTexture);
            // Channel 14: Plates Blue
            float platesBlue = getValueAtCoords(dmx+13, _Udon_DMXGridRenderTexture);
            half4 platesColorIntensity = GetDMXColor_Fixed(dmx+11);
            // Channels 15-50: Plate Pixels
            float4 platePixelColor = GetDMXColor_Fixed(Get12PanelCh(IN.uv_EmissionMask, dmx+14));
            // Channels 51-62: Beam Pixels
            float4 beamPixelIntensity = getValueAtCoords(Get12BeamCh(IN.uv_EmissionMask, dmx+50), _Udon_DMXGridRenderTexture);
                
            float4 beamFinalColor = (beamIntensity*beamPixelIntensity) 
                * beamStrobe * _CurveBeam * _CurveBeam;
            float4 plateFinalColor = ((plateIntensity*platesColorIntensity)*platePixelColor)
                * plateStrobe * _CurvePlate * _CurvePlate;

            float3 emission = beamFinalColor + plateFinalColor;
            emission = isDMX() ? emission : getEmissionColor();//getBaseEmission();
            emission *= getGlobalIntensity() * getFinalIntensity();


            o.Emission += tex2D(_EmissionMask, IN.uv_EmissionMask) * emission;
            
            
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            fixed4 ms = tex2D (_MetallicSmoothness, IN.uv_MetallicSmoothness);
 
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic * ms.r;
            o.Smoothness = _Glossiness * ms.a;
            o.Alpha = c.a;
            o.Normal = UnpackNormal (tex2D (_NormalMap, IN.uv_NormalMap));
        }
        ENDCG
    }
    FallBack "Diffuse"
}
