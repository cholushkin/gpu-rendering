Shader "Tutorial/GPUAnimatedTiles"
{
    Properties
    {
        _Tiles ("Tile Array", 2DArray) = "" {}
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct InstanceData
            {
                float4x4 transform;
                float2 gridPos;
                float tileIndex;
            };

            StructuredBuffer<InstanceData> _InstanceData;

            TEXTURE2D_ARRAY(_Tiles);
            SAMPLER(sampler_Tiles);

            float _TimeValue;
            float _WaveAmplitude;
            float _WaveFrequency;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float tileIndex : TEXCOORD1;
            };

            Varyings vert(Attributes input, uint instanceID : SV_InstanceID)
            {
                InstanceData data = _InstanceData[instanceID];

                float wave =
                    sin(_TimeValue * 2 +
                        data.gridPos.x * _WaveFrequency +
                        data.gridPos.y * _WaveFrequency);

                float4 world =
                    mul(data.transform, float4(input.positionOS,1));

                world.y += wave * _WaveAmplitude;

                VertexPositionInputs pos =
                    GetVertexPositionInputs(world.xyz);

                Varyings o;

                o.positionCS = pos.positionCS;
                o.uv = input.uv;
                o.tileIndex = data.tileIndex;

                return o;
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