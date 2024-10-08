#ifndef TOY_LIGHT_INCLUDE
#define TOY_LIGHT_INCLUDE

#include "Common.hlsl"

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

CBUFFER_START(_CustomLight)
    int _DirectionalLightCount;
    vector _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
    vector _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

struct Light
{
    float3 color;
    float3 direction;
};
int GetDirectionalLightCount()
{
    return _DirectionalLightCount;
}
Light GetDirectionalLight(int index)
{
    Light light;
    light.color = _DirectionalLightColors[index].rgb;
    light.direction = normalize(_DirectionalLightDirections[index].xyz);
    return light;
}

#endif