Shader "ToyRenderPipeline/lightPass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off 
        ZWrite On 
        ZTest Always

        Pass
        {
            HLSLPROGRAM
            //不生成OpenGL ES 2.0等图形API的着色器变体，其不支持可变次数的循环与线性颜色空间
            #pragma target 3.5
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #include "Assets/ToyRenderPipeline/ShaderLibrary/LitPass.hlsl"
            ENDHLSL
        }
    }
}