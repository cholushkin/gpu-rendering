Shader "Tutorial/TextureArrayTiles"
{
    Properties
    {
        _Tiles ("Tile Array", 2DArray) = "" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_ARRAY(_Tiles);
            SAMPLER(sampler_Tiles);

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
                float tileIndex : TEXCOORD1;
            };

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _TileIndex)
            UNITY_INSTANCING_BUFFER_END(Props)

            Varyings vert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);

                Varyings output;

                VertexPositionInputs pos =
                    GetVertexPositionInputs(input.positionOS);

                output.positionCS = pos.positionCS;
                output.uv = input.uv;

                output.tileIndex =
                    UNITY_ACCESS_INSTANCED_PROP(Props,_TileIndex);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return SAMPLE_TEXTURE2D_ARRAY(
                    _Tiles,
                    sampler_Tiles,
                    input.uv,
                    input.tileIndex
                );
            }

            ENDHLSL
        }
    }
}