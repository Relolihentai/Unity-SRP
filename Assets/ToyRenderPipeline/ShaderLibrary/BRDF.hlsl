#ifndef TOY_BRDF_INCLUDE
#define TOY_BRDF_INCLUDE

#include "Surface.hlsl"
#include "Light.hlsl"

float D_Function(float NdotH, float roughness)
{
    float a2 = roughness * roughness;
    float NdotH2 = NdotH * NdotH;
    float d = (NdotH2 * (a2 - 1) + 1);
    d = d * d * PI;
    return a2 / d;
}
inline float G_subSection(float dot, float k)
{
    return dot / lerp(dot, 1, k);
}
float G_Function(float NdotL, float NdotV, float roughness)
{
    // method1-k:
    // // float k = pow(1 + roughness, 2) / 8.0;

    // // method2-k:
    // const float d = 1.0 / 8.0;
    // float k = pow(1 + roughness, 2) * d;

    // method3-k
    float k = pow(1 + roughness, 2) * 0.5;
    return G_subSection(NdotL, k) * G_subSection(NdotV, k);
}
inline float3 F0_Function(float3 albedo, float metallic)
{
    // jave.lin :
    // 非金属一般都是纯灰度：0.02 ~ 0.04
    // 金属一般都是纯颜色：0.7 ~ 1.0 之间的，这里我们简单的使用一个 lerp 来模拟
    // 后续其他版本的 pbr 可以调整为更加效果的
    return lerp(0.04, albedo, metallic);
}

float3 F_Function(float HdotL, float3 F)
{
    // jave.lin : (1-x)^5 转为性能更高的来模拟：2^((-5.55473 * x- 6.98316) * x)
    float Fre = exp2((-5.55473 * HdotL - 6.98316) * HdotL);
    return lerp(Fre, 1, F);
}

float3 Indirect_F_Function(float NdotV, float3 F0, float roughness)
{
    float fre = exp2((-5.55473 * NdotV - 6.98316) * NdotV);
    return F0 + fre * saturate(1 - roughness - F0);
}

float3 LitDirectLight(Surface surface, Light light)
{
    float3 viewDir = normalize(Toy_WorldSpaceCameraPos.xyz - surface.worldPos);
    float3 halfDir = normalize(viewDir + light.direction);
    float nol = max(saturate(dot(surface.worldNormal, light.direction)), 1e-5);
    float noh = max(saturate(dot(surface.worldNormal, halfDir)), 1e-5);
    float nov = max(saturate(dot(surface.worldNormal, viewDir)), 1e-5);
    float hol = max(saturate(dot(halfDir, light.direction)), 1e-5);
                
    float3 F0 = F0_Function(surface.color, surface.metallic);
    float Direct_D = D_Function(noh, surface.roughness);
    float Direct_G = G_Function(nol, nov, surface.roughness);
    float3 Direct_F = F_Function(hol, F0);
    float3 BRDFSpecSection = Direct_D * Direct_G * Direct_F / (4 * nol * nov);
    float3 DirectSpecColor = BRDFSpecSection * light.color * nol * PI;
    float3 Ks = Direct_F;
    float3 Kd = (1 - Ks) * (1 - surface.metallic);
    float3 DirectDiffColor = Kd * surface.color * light.color * nol;
    float3 DirectColor = DirectSpecColor + DirectDiffColor;
    return DirectColor;
}

#endif