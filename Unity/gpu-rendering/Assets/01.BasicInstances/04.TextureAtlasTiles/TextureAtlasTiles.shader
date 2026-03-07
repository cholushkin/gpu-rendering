Shader "Tutorial/TextureAtlasTiles"
{
    Properties
    {
        _Atlas ("Atlas Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_Atlas);
            SAMPLER(sampler_Atlas);

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4,_AtlasUV)
            UNITY_INSTANCING_BUFFER_END(Props)


            Varyings vert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);

                Varyings output;

                VertexPositionInputs pos =
                    GetVertexPositionInputs(input.positionOS);

                output.positionCS = pos.positionCS;

                float4 atlas =
                    UNITY_ACCESS_INSTANCED_PROP(Props,_AtlasUV);

                output.uv =
                    input.uv * atlas.xy + atlas.zw;

                return output;
            }


            half4 frag(Varyings input) : SV_Target
            {
                half4 color =
                    SAMPLE_TEXTURE2D(
                        _Atlas,
                        sampler_Atlas,
                        input.uv
                    );

                return color;
            }

            ENDHLSL
        }
    }
}