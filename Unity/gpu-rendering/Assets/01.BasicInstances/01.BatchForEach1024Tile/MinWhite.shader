Shader "Minimal/WhiteInstanced"
{
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

            struct Attributes
            {
                float3 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);

                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS);

                Varyings o;
                o.positionCS = pos.positionCS;
                return o;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return half4(1,1,1,1);
            }

            ENDHLSL
        }
    }
}