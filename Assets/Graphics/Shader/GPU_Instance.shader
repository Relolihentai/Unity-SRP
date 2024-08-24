Shader "ToyShader/GPU_Instance"
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
        }
        
        HLSLINCLUDE
        
        #include "Assets/ToyRenderPipeline/ShaderLibrary/Common.hlsl"
        sampler2D _MainTex;
        UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
            UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
            UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
        UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)
        ENDHLSL

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

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
                float3 color = tex2D(_MainTex, i.uv).rgb;
                float3 normal = i.normal;
                GT0 = float4(color * baseColor.rgb, 1);
                GT1 = float4(normal * 0.5 + 0.5, 0);
                GT2 = float4(1, 1, 0, 1);
                GT3 = float4(0, 0, 1, 1);
            }
            ENDHLSL
        }
    }
}
