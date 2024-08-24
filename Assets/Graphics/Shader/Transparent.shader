Shader "ToyShader/Transparent"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags 
        { 
            "LightMode" = "GBuffer"
            "Queue" = "Transparent"
        }
        
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        HLSLINCLUDE
        
        #include "Assets/ToyRenderPipeline/ShaderLibrary/Common.hlsl"
        
        cbuffer UnityPerMaterial
        {
            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _BaseColor;
        }
        ENDHLSL

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = v.normal;
                return o;
            }

            void frag (v2f i, out float4 GT0 : SV_Target0, out float4 GT1 : SV_Target1, out float4 GT2 : SV_Target2, out float4 GT3 : SV_Target3)
            {
                float4 color = tex2D(_MainTex, i.uv);
                float3 normal = i.normal;
                GT0 = color * _BaseColor;
                GT1 = float4(normal * 0.5 + 0.5, 0);
                GT2 = float4(1, 1, 0, 1);
                GT3 = float4(0, 0, 1, 1);
            }
            ENDHLSL
        }
    }
}
