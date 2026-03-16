Shader "Tutorial/ComputeGeneratedTiles"
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
                float2 position;
                float rotation;
                float scale;
                uint tileIndex;
            };

            StructuredBuffer<InstanceData> _InstanceData;

            TEXTURE2D_ARRAY(_Tiles);
            SAMPLER(sampler_Tiles);

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

            float2 Rotate(float2 p,float angle)
            {
                float s = sin(angle);
                float c = cos(angle);

                return float2(
                    c*p.x - s*p.y,
                    s*p.x + c*p.y
                );
            }

            Varyings vert(Attributes input,uint instanceID : SV_InstanceID)
            {
                InstanceData data = _InstanceData[instanceID];

                float2 pos = input.positionOS.xy;

                pos *= data.scale;

                pos = Rotate(pos,data.rotation);

                pos += data.position;

                float3 world = float3(pos,0);

                VertexPositionInputs v =
                    GetVertexPositionInputs(world);

                Varyings o;

                o.positionCS = v.positionCS;
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