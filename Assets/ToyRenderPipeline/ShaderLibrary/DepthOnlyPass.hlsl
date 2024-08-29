#ifndef TOY_DEPTH_ONLY_PASS_INCLUDE
#define TOY_DEPTH_ONLY_PASS_INCLUDE

#include "Common.hlsl"

struct DepthOnlyPassAttributes
{
    float4 vertex : POSITION;
};

struct DepthOnlyPassVaryings
{
    float4 vertex : SV_POSITION;
    float2 depth : TEXCOORD0;
};

DepthOnlyPassVaryings DepthOnlyPassVertex (DepthOnlyPassAttributes v)
{
    DepthOnlyPassVaryings o;
    o.vertex = TransformObjectToHClip(v.vertex.xyz);
    o.depth = o.vertex.zw;
    return o;
}

float4 DepthOnlyPassFragment (DepthOnlyPassVaryings i) : SV_Target
{
    float depth = i.depth.x / i.depth.y;
    #ifdef UNITY_REVERSED_Z
    depth = 1.0 - depth;
    #endif
    float4 color = EncodeFloatRGBA(depth);
    return color;
}

#endif