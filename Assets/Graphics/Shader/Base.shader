Shader "ToyShader/Base"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend",Float) = 0
        [Enum(Off,0,On,1)] _ZWrite("Z Write",Float) = 1
        [Toggle(_ALPHACLIP)] _AlphaClip("Alpha Clip", Float) = 0
        _AlphaClipOff("Alpha Clip Off", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags 
        { 
            "LightMode" = "GBuffer"
        }
        
        ZWrite [_ZWrite]
        Blend [_SrcBlend][_DstBlend]
        
        HLSLINCLUDE
        
        #include "Assets/ToyRenderPipeline/ShaderLibrary/Common.hlsl"
        
        UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
            UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
            UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
            UNITY_DEFINE_INSTANCED_PROP(float, _AlphaClip)
            UNITY_DEFINE_INSTANCED_PROP(float, _AlphaClipOff)
        UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)
        
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        ENDHLSL

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile_instancing
            
            #pragma shader_feature _ALPHACLIP

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                float4 mainTex_ST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _MainTex_ST);
                o.uv = v.uv * mainTex_ST.xy + mainTex_ST.zw;
                o.normal = v.normal;
                return o;
            }

            void frag (v2f i, out float4 GT0 : SV_Target0, out float4 GT1 : SV_Target1, out float4 GT2 : SV_Target2, out float4 GT3 : SV_Target3)
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
                float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                
                #ifdef _ALPHACLIP
                clip(color.a * baseColor.a - _AlphaClipOff);
                #endif
                
                float3 normal = i.normal;
                GT0 = color * baseColor;
                GT1 = float4(normal * 0.5 + 0.5, 0);
                GT2 = float4(1, 1, 0, 1);
                GT3 = float4(0, 0, 1, 1);
            }
            ENDHLSL
        }
    }
}
