#ifndef TOY_COMMON_INCLUDE
#define TOY_COMMON_INCLUDE

#include "UnityInput.hlsl"

#define UNITY_MATRIX_M unity_ObjectToWorld
#define UNITY_MATRIX_I_M unity_WorldToObject
#define UNITY_MATRIX_V unity_MatrixV
#define UNITY_MATRIX_VP unity_MatrixVP
#define UNITY_MATRIX_P glstate_matrix_projection

#define UNITY_PREV_MATRIX_M unity_ObjectToWorld
#define UNITY_PREV_MATRIX_I_M unity_WorldToObject
#define UNITY_MATRIX_I_V Toy_MatrixInvV

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

inline float4 EncodeFloatRGBA( float v ) {
    float4 enc = float4(1.0, 255.0, 65025.0, 16581375.0) * v;
    enc = frac(enc);
    enc -= enc.yzww * float4(1.0/255.0,1.0/255.0,1.0/255.0,0.0);
    return enc;
}

inline float DecodeFloatRGBA( float4 rgba ) {
    return dot( rgba, float4(1.0, 1/255.0, 1/65025.0, 1/16581375.0) );
}

#endif