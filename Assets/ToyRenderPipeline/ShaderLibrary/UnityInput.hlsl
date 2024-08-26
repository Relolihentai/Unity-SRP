#ifndef TOY_UNITY_INPUT_INCLUDE
#define TOY_UNITY_INPUT_INCLUDE

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

CBUFFER_START(UnityPerDraw)
float4x4 unity_ObjectToWorld;
float4x4 unity_WorldToObject;
real4 unity_WorldTransformParams;

float4x4 unity_MatrixV;
float4x4 glstate_matrix_projection;
float4x4 unity_MatrixInvV;
CBUFFER_END

float4x4 unity_MatrixVP;
float4 _ZBufferParams;
float4 _ScreenParams;

#endif