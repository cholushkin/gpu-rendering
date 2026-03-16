Shader "Tutorial/MassiveTileWorlds"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
                float noise;
            };

            StructuredBuffer<InstanceData> _InstanceData;

            float3 _CameraOffset;
            float _HeightScale;

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float noise : TEXCOORD1;
            };

            float2 Rotate(float2 p,float a)
            {
                float s = sin(a);
                float c = cos(a);

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

                float3 world;

                world.x = pos.x;
                world.z = pos.y;
                world.y = data.noise * _HeightScale;

                world -= _CameraOffset;

                VertexPositionInputs v =
                    GetVertexPositionInputs(world);

                Varyings o;

                o.positionCS = v.positionCS;
                o.uv = input.uv;
                o.noise = data.noise;

                return o;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 color =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        input.uv
                    );

                float tint = lerp(0.6,1.4,input.noise);

                color.rgb *= tint;

                return color;
            }

            ENDHLSL
        }
    }
}