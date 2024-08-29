#ifndef TOY_LIGHTING_INCLUDE
#define TOY_LIGHTING_INCLUDE

#include "BRDF.hlsl"
#include "Shadow.hlsl"

float3 GetLitLighting(Surface surface)
{
    float3 color = 0;
    for (int i = 0; i < GetDirectionalLightCount(); i++)
    {
        color += LitDirectLight(surface, GetDirectionalLight(i));
        color *= GetShadow(float4(surface.worldPos + surface.worldNormal * 0.01, 1), surface.depthLinear);
    }
    return color;
}

#endif