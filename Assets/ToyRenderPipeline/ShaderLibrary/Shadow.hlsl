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

float _CSM_Radius_0;
float _CSM_Radius_1;
float _CSM_Radius_2;
float _CSM_Radius_3;

vector _CSM_SphereCenter_0;
vector _CSM_SphereCenter_1;
vector _CSM_SphereCenter_2;
vector _CSM_SphereCenter_3;

float _CSM_ShadowMapResolution;

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

float ShadowMapPCF(float4 worldPos, Texture2D _ShadowMap, SamplerState sampler_ShadowMap, float4x4 _ShadowMatrixVP)
{
    float4 shadowNDC = mul(_ShadowMatrixVP, worldPos);
    shadowNDC /= shadowNDC.w;

    float2 uv = shadowNDC.xy * 0.5 + 0.5;
    if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) return 1.0f;
    float depth = shadowNDC.z;
    
    
    float shadow = 0.0;
    for (int i = -1; i <= 1; i++)
    {
        for (int j = -1; j <= 1; j++)
        {
            float2 offset = float2(i, j) / _CSM_ShadowMapResolution;
            float2 pcf_uv = uv + offset;
            float depth_sample = SAMPLE_TEXTURE2D(_ShadowMap, sampler_ShadowMap, pcf_uv).r;
            #if defined (UNITY_REVERSED_Z)
            if(depth_sample > depth) shadow += 1;
            #else
            if(depth_sample < depth) shadow += 1;
            #endif
        }
    }
    return 1 - shadow / 9;
}

float2 AverageBlockerDepth(float4 shadowNdc, Texture2D _ShadowMap, SamplerState sampler_ShadowMap, float depth_shadingPoint, float searchWidth)
{
    float2 uv = shadowNdc.xy * 0.5 + 0.5;
    float step = 3.0;
    float d_average = 0.0;
    float count = 0.0005;   // 防止 ÷ 0

    for(int i=-step; i<=step; i++)
    {
        for(int j=-step; j<=step; j++)
        {
            float2 unitOffset = float2(i, j) / step;  // map to [-1, 1]
            float2 offset = unitOffset * searchWidth;
            float2 uvo = uv + offset;

            float depth_sample = SAMPLE_TEXTURE2D(_ShadowMap, sampler_ShadowMap, uvo).r;
            if(depth_sample > depth_shadingPoint)
            {
                count += 1;
                d_average += depth_sample;
            }
        }
    }
    return float2(d_average / count, count);
}

float ShadowMapPCSS(float4 worldPos, Texture2D _ShadowMap, SamplerState sampler_ShadowMap, float4x4 _ShadowMatrixVP,
                    float orthoDistance, float searchRadius, float filterRadius)
{
    float4 shadowNDC = mul(_ShadowMatrixVP, worldPos);
    shadowNDC /= shadowNDC.w;
    float depth_shadingPoint = shadowNDC.z;
    float2 uv = shadowNDC.xy * 0.5 + 0.5;

    float searchWidth = searchRadius / orthoDistance;
    float2 blocker = AverageBlockerDepth(shadowNDC, _ShadowMap, sampler_ShadowMap, depth_shadingPoint, searchWidth);
    float d_average = blocker.x;
    float blockCount = blocker.y;

    if (blockCount < 1) return 1.0;

    float d_receiver = 1.0 - depth_shadingPoint;
    float d_blocker = 1.0 - d_average;

    float w = (d_receiver - d_blocker) * filterRadius / d_blocker;
    float radius = w / orthoDistance;

    float shadow = 0.0;
    float sum = 0;
    float step = 3;     // 半径为 step*2+1 像素的带洞卷积
    for(int i=-step; i<=step; i++)
    {
        for(int j=-step; j<=step; j++)
        {
            sum += 1;
            float2 offset = float2(i, j) / step;
            float2 uvo = uv + offset * radius;
            //if(uvo.x<0 || uvo.x>1 || uvo.y<0 || uvo.y>1) continue;

            float depth_sample = SAMPLE_TEXTURE2D(_ShadowMap, sampler_ShadowMap, uvo).r;
            if(depth_sample > depth_shadingPoint) shadow += 1.0f;
        }
    }
    shadow /= sum;
    return 1.0 - shadow;
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
    
    // if (distance(_CSM_SphereCenter_0.xyz, worldPos.xyz) < _CSM_Radius_0)
    //     shadow *= ShadowMapPCF(worldPos, _ShadowMap_0, sampler_ShadowMap_0, Toy_ShadowMatrixVP_0);
    // else if (distance(_CSM_SphereCenter_1.xyz, worldPos.xyz) < _CSM_Radius_1)
    //     shadow *= ShadowMapPCF(worldPos, _ShadowMap_1, sampler_ShadowMap_1, Toy_ShadowMatrixVP_1);
    // else if (distance(_CSM_SphereCenter_2.xyz, worldPos.xyz) < _CSM_Radius_2)
    //     shadow *= ShadowMapPCF(worldPos, _ShadowMap_2, sampler_ShadowMap_2, Toy_ShadowMatrixVP_2);
    // else if (distance(_CSM_SphereCenter_3.xyz, worldPos.xyz) < _CSM_Radius_3)
    //     shadow *= ShadowMapPCF(worldPos, _ShadowMap_3, sampler_ShadowMap_3, Toy_ShadowMatrixVP_3);

    // if (distance(_CSM_SphereCenter_0.xyz, worldPos.xyz) < _CSM_Radius_0)
    //     shadow *= ShadowMapPCSS(worldPos, _ShadowMap_0, sampler_ShadowMap_0, Toy_ShadowMatrixVP_0, _CSM_Radius_0 * 2, 1, 1);
    // else if (distance(_CSM_SphereCenter_1.xyz, worldPos.xyz) < _CSM_Radius_1)
    //     shadow *= ShadowMapPCSS(worldPos, _ShadowMap_1, sampler_ShadowMap_1, Toy_ShadowMatrixVP_1, _CSM_Radius_1 * 2, 1, 1);
    // else if (distance(_CSM_SphereCenter_2.xyz, worldPos.xyz) < _CSM_Radius_2)
    //     shadow *= ShadowMapPCSS(worldPos, _ShadowMap_2, sampler_ShadowMap_2, Toy_ShadowMatrixVP_2, _CSM_Radius_2 * 2, 1, 1);
    // else if (distance(_CSM_SphereCenter_3.xyz, worldPos.xyz) < _CSM_Radius_3)
    //     shadow *= ShadowMapPCSS(worldPos, _ShadowMap_3, sampler_ShadowMap_3, Toy_ShadowMatrixVP_3, _CSM_Radius_3 * 2, 1, 1);
    return shadow;
}


#endif