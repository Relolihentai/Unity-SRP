Shader "ToyShader/Base"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _Metallic ("Metallic", Range(0, 1)) = 0
        _Roughness ("Roughness", Range(0, 1)) = 0
        
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend",Float) = 0
        [Enum(Off,0,On,1)] _ZWrite("Z Write",Float) = 1
        [Toggle(_ALPHACLIP)] _AlphaClip("Alpha Clip", Float) = 0
        _AlphaClipOff("Alpha Clip Off", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags 
        { 
            "LightMode" = "GBuffer"
        }
        
        ZWrite [_ZWrite]
        ZTest LEqual
        Blend [_SrcBlend][_DstBlend]
        
        HLSLINCLUDE
        //不生成OpenGL ES 2.0等图形API的着色器变体，其不支持可变次数的循环与线性颜色空间
        #pragma target 3.5
        
        ENDHLSL

        Pass
        {
            Name "DeferredWrite"
            HLSLPROGRAM
            #pragma vertex LitDeferredWritePassVertex
            #pragma fragment LitDeferredWriteFragment
            #pragma multi_compile_instancing
            #pragma shader_feature _ALPHACLIP
            #include "Assets/ToyRenderPipeline/ShaderLibrary/LitDeferredWritePass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags{ "LightMode" = "DepthOnly" }
            HLSLPROGRAM
            #pragma vertex DepthOnlyPassVertex
            #pragma fragment DepthOnlyPassFragment
            #pragma multi_compile_instancing
            #include "Assets/ToyRenderPipeline/ShaderLibrary/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }
}
