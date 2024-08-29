#ifndef TOY_SHADOW_INCLUDE
#define TOY_SHADOW_INCLUDE

#include "Common.hlsl"

float4x4 Toy_ShadowMatrixVP_0;
float4x4 Toy_ShadowMatrixVP_1;
float4x4 Toy_ShadowMatrixVP_2;
float4x4 Toy_ShadowMatrixVP_3;

TEXTURE2D(_ShadowMap_0);
SAMPLER(sampler_ShadowMap_0);
TEXTURE2D(_ShadowMap_1);
SAMPLER(sampler_ShadowMap_1);
TEXTURE2D(_ShadowMap_2);
SAMPLER(sampler_ShadowMap_2);
TEXTURE2D(_ShadowMap_3);
SAMPLER(sampler_ShadowMap_3);

float _CSM_Split_0;
float _CSM_Split_1;
float _CSM_Split_2;
float _CSM_Split_3;

float ShadowMap01(float4 worldPos, Texture2D _ShadowMap, SamplerState sampler_ShadowMap, float4x4 _ShadowMatrixVP)
{
    float4 shadowNdc = mul(_ShadowMatrixVP, worldPos);
    shadowNdc /= shadowNdc.w;
    float2 uv = shadowNdc.xy * 0.5 + 0.5;

    if(uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) return 1.0f;

    float d = shadowNdc.z;
    float d_sample = SAMPLE_TEXTURE2D(_ShadowMap, sampler_ShadowMap, uv).r;

    #if defined (UNITY_REVERSED_Z)
    if(d_sample > d) return 0.0f;
    #else
    if(d_sample < d) return 0.0f;
    #endif

    return 1.0f;
}

float GetShadow(float4 worldPos, float depthLinear)
{
    float shadow = 1;
    if(depthLinear < _CSM_Split_0) 
        shadow *= ShadowMap01(worldPos, _ShadowMap_0, sampler_ShadowMap_0, Toy_ShadowMatrixVP_0);
    else if(depthLinear < _CSM_Split_0 + _CSM_Split_1) 
        shadow *= ShadowMap01(worldPos, _ShadowMap_1, sampler_ShadowMap_1, Toy_ShadowMatrixVP_1);
    else if(depthLinear < _CSM_Split_0 + _CSM_Split_1 + _CSM_Split_2) 
        shadow *= ShadowMap01(worldPos, _ShadowMap_2, sampler_ShadowMap_2, Toy_ShadowMatrixVP_2);
    else if(depthLinear < _CSM_Split_0 + _CSM_Split_1 + _CSM_Split_2 + _CSM_Split_3)
        shadow *= ShadowMap01(worldPos, _ShadowMap_3, sampler_ShadowMap_3, Toy_ShadowMatrixVP_3);
    return shadow;
}


#endif