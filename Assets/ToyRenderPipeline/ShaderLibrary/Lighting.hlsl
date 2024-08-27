#ifndef TOY_LIGHTING_INCLUDE
#define TOY_LIGHTING_INCLUDE


#include "BRDF.hlsl"

float3 GetLitLighting(Surface surface)
{
    float3 color = 0;
    for (int i = 0; i < GetDirectionalLightCount(); i++)
    {
        color += LitDirectLight(surface, GetDirectionalLight(i));
    }
    return color;
}

#endif