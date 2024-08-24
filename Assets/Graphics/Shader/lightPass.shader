Shader "ToyRenderPipeline/lightPass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/ToyRenderPipeline/ShaderLibrary/Common.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _gdepth;
            sampler2D _GT0;
            sampler2D _GT1;
            sampler2D _GT2;
            sampler2D _GT3;

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_GT0, i.uv);
                return col;
            }
            ENDHLSL
        }
    }
}