Shader "ToyShader/LitDeferredWrite"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        
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
        Blend [_SrcBlend][_DstBlend]
        
        HLSLINCLUDE
        #include "Assets/ToyRenderPipeline/ShaderLibrary/LitPass.hlsl"
        ENDHLSL

        Pass
        {
            HLSLPROGRAM
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #pragma multi_compile_instancing
            #pragma shader_feature _ALPHACLIP
            ENDHLSL
        }
    }
}
