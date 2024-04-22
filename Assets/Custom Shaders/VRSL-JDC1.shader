Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [Toggle] _EnableDMX ("Enable Stream DMX/DMX Control", Int) = 0
        [Toggle] _NineUniverseMode ("Extended Universe Mode", Int) = 0
        [Toggle] _EnableVerticalMode ("Enable Vertical Mode", Int) = 0
        [Toggle] _EnableCompatibilityMode ("Enable Compatibility Mode", Int) = 0

        [Toggle] _EnableStrobe ("Enable Strobe", Int) = 0
        [Header(Base Properties)]
        [HDR] _Emission ("Base DMX Emission Color", Color) = (1,1,1,1)
        _GlobalIntensity ("Global Intensity", Range(0,1)) = 1
        _FinalIntensity ("Final Intensity", Range(0,1)) = 1

        _DMXChannel ("Starting DMX Channel", Int) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            // always read dmx from the vertex shader
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 finalColor : TEXCOORD1;
            };

            #include "Packages/com.acchosen.vr-stage-lighting/Runtime/Shaders/VRSLDMX.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);

                // intensity 1
                // rgb 2, 3, 4
                // strobe 5
                
                int dmx = GetDMXChannel();
                float intensity = ReadDMX(dmx, _Udon_DMXGridRenderTexture); // 0-1
                //float4 color = float4(ReadDMX(dmx+1, _Udon_DMXGridRenderTexture), ReadDMX(dmx+2, _Udon_DMXGridRenderTexture), ReadDMX(dmx+3, _Udon_DMXGridRenderTexture), 1);
                float4 color = GetDMXColor(dmx+1);
                float strobe = GetStrobeOutput(dmx+4);
                o.finalColor = (intensity * color) * strobe;
                o.finalColor = isDMX() ? o.finalColor : getBaseEmission();
                o.finalColor *= getGlobalIntensity() * getFinalIntensity();

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * i.finalColor;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
