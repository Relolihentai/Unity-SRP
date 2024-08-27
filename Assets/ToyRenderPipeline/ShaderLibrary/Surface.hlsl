#ifndef TOY_SURFACE_INCLUDE
#define TOY_SURFACE_INCLUDE

#define PI 3.14159265358

struct Surface
{
    float4 color;
    float3 worldNormal;
    float3 worldPos;
    float metallic;
    float roughness;
};

#endif