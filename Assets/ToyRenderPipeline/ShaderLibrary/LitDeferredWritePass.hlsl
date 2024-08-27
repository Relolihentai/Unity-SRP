#ifndef TOY_LIT_DEFERRED_WRITE_PASS_INCLUDE
#define TOY_LIT_DEFERRED_WRITE_PASS_INCLUDE

#include "Common.hlsl"

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DEFINE_INSTANCED_PROP(float, _Metallic)
    UNITY_DEFINE_INSTANCED_PROP(float, _Roughness)
    UNITY_DEFINE_INSTANCED_PROP(float, _AlphaClip)
    UNITY_DEFINE_INSTANCED_PROP(float, _AlphaClipOff)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

struct LitDeferredWritePassAttributes
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float3 normal : NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct LitDeferredWritePassVaryings
{
    float4 vertex : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 worldNormal : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

LitDeferredWritePassVaryings LitDeferredWritePassVertex (LitDeferredWritePassAttributes v)
{
    LitDeferredWritePassVaryings o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    o.vertex = TransformObjectToHClip(v.vertex.xyz);
    float4 mainTex_ST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _MainTex_ST);
    o.uv = v.uv * mainTex_ST.xy + mainTex_ST.zw;
    o.worldNormal = TransformObjectToWorldNormal(v.normal);
    return o;
}

void LitDeferredWriteFragment (LitDeferredWritePassVaryings i, out float4 GT0 : SV_Target0, out float4 GT1 : SV_Target1, out float4 GT2 : SV_Target2, out float4 GT3 : SV_Target3)
{
    UNITY_SETUP_INSTANCE_ID(i);
    float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                
    #ifdef _ALPHACLIP
    clip(color.a * baseColor.a - _AlphaClipOff);
    #endif

    float roughness = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Roughness);
    float metallic = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Metallic);
    
    GT0 = baseColor * color;
    GT1 = float4(i.worldNormal * 0.5 + 0.5, 0);
    GT2 = float4(0, 0, roughness, metallic);
    GT3 = float4(0, 0, 1, 1);
}

#endif