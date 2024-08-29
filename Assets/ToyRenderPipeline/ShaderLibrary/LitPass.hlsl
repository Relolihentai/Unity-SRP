#ifndef TOY_LIT_PASS_INCLUDE
#define TOY_LIT_PASS_INCLUDE

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
    float4 vertex : SV_POSITION;
    float2 uv : TEXCOORD0;
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
    float3 worldNormal = normalize(SAMPLE_TEXTURE2D(_GT1, sampler_GT1, i.uv).xyz * 2 - 1);
    float4 gt2 = SAMPLE_TEXTURE2D(_GT2, sampler_GT2, i.uv);
    float roughness = gt2.z;
    float metallic = gt2.w;
    float rawDepth = SAMPLE_DEPTH_TEXTURE(_GDepth, sampler_GDepth, i.uv);
    depthout = rawDepth;
    float depthLinear = Linear01Depth(rawDepth, _ZBufferParams);

    //这里就是NDC坐标
    float4 ndcPos = float4(i.uv * 2 - 1, rawDepth, 1);
    float far = _ProjectionParams.z;
    float3 clipVec = float3(ndcPos.x, ndcPos.y, 1.0) * far;
    float3 viewVec = mul(Toy_MatrixInvP, clipVec.xyzz).xyz;
    float3 viewPos = viewVec * depthLinear;
    float3 worldPos = mul(Toy_MatrixInvV, float4(viewPos, 1.0)).xyz;
    
    Surface surface;
    surface.color = color;
    surface.worldNormal = worldNormal;
    surface.worldPos = worldPos;
    surface.depthLinear = depthLinear;
    surface.metallic = metallic;
    surface.roughness = roughness;

    float3 finalColor = GetLitLighting(surface);
    return float4(finalColor.xyz, 1);
}

#endif