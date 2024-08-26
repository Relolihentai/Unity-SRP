Shader "ToyRenderPipeline/lightPass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite On ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #include "Assets/ToyRenderPipeline/ShaderLibrary/LitPass.hlsl"
            ENDHLSL
        }
    }
}