Shader "Tutorial/GPUProceduralTiles"
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

            TEXTURE2D_ARRAY(_Tiles);
            SAMPLER(sampler_Tiles);

            int _GridSize;
            float _TileSpacing;
            int _TileCount;

            //--------------------------------------------

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

            //--------------------------------------------

            Varyings vert(Attributes input, uint instanceID : SV_InstanceID)
            {
                //-----------------------------------------
                // Grid coordinates
                //-----------------------------------------

                uint x = instanceID % _GridSize;
                uint y = instanceID / _GridSize;

                float2 grid = float2(x,y);

                //-----------------------------------------
                // Base tile position
                //-----------------------------------------

                float2 pos;

                pos.x = (x - _GridSize * 0.5) * _TileSpacing;
                pos.y = (y - _GridSize * 0.5) * _TileSpacing;

                //-----------------------------------------
                // Center-relative math
                //-----------------------------------------

                float2 center = float2(_GridSize * 0.5, _GridSize * 0.5);

                float2 delta = grid - center;

                float dist = length(delta);

                float2 dir = normalize(delta + 0.0001);

                //-----------------------------------------
                // Radial rotation
                //-----------------------------------------

                float angle = atan2(dir.y, dir.x);

                float s = sin(angle);
                float c = cos(angle);

                float2 rotated;

                rotated.x = input.positionOS.x * c - input.positionOS.y * s;
                rotated.y = input.positionOS.x * s + input.positionOS.y * c;

                //-----------------------------------------
                // Improved ripple animation
                //-----------------------------------------

                float frequency = 0.8;
                float speed = 2.0;

                float phase =
                    dist * frequency -
                    _Time.y * speed;

                float ripple =
                    sin(phase);

                // fade waves outward
                float attenuation =
                    exp(-dist * 0.05);

                float height =
                    ripple * attenuation * 0.8;

                //-----------------------------------------
                // Final world position
                //-----------------------------------------

                float3 world;

                world.xy = pos + rotated;
                world.z  = height;

                //-----------------------------------------

                VertexPositionInputs posInputs =
                    GetVertexPositionInputs(world);

                Varyings o;

                o.positionCS = posInputs.positionCS;
                o.uv = input.uv;

                //-----------------------------------------
                // Distance ring tile selection
                //-----------------------------------------

                o.tileIndex =
                    floor(dist) % _TileCount;

                return o;
            }

            //--------------------------------------------

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