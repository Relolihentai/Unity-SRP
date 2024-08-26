#ifndef TOY_LITPASS_INCLUDE
#define TOY_LITPASS_INCLUDE

#include "Lighting.hlsl"

TEXTURE2D(_GDepth);
SAMPLER(sampler_GDepth);
TEXTURE2D(_GT0);
SAMPLER(sampler_GT0);
TEXTURE2D(_GT1);
SAMPLER(sampler_GT1);
TEXTURE2D(_GT2);
SAMPLER(sampler_GT2);
TEXTURE2D(_GT3);
SAMPLER(sampler_GT3);

struct LitPassAttributes
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct LitPassVaryings
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
};

LitPassVaryings LitPassVertex (LitPassAttributes v)
{
    LitPassVaryings o;
    o.vertex = TransformObjectToHClip(v.vertex.xyz);
    o.uv = v.uv;
    return o;
}

float4 LitPassFragment (LitPassVaryings i, out float depthout : SV_Depth) : SV_Target
{
    float4 color = SAMPLE_TEXTURE2D(_GT0, sampler_GT0, i.uv);
    float3 worldNormal = SAMPLE_TEXTURE2D(_GT1, sampler_GT1, i.uv).xyz * 2 - 1;
    float rawDepth = SAMPLE_TEXTURE2D(_GDepth, sampler_GDepth, i.uv).x;
    depthout = rawDepth;
    float depth = Linear01Depth(rawDepth, _ZBufferParams);
    
    Surface surface;
    surface.alpha = color.a;
    surface.color = color.rgb;
    surface.normal = worldNormal;

    float3 finalColor = GetLighting(surface);
    return float4(finalColor, 1);
}

#endif