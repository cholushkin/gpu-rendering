Shader "Tutorial/InstanceColor"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
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

            struct Attributes
            {
                float3 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
            };

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _InstanceColor)
            UNITY_INSTANCING_BUFFER_END(Props)

            Varyings vert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);

                Varyings output;

                VertexPositionInputs pos =
                    GetVertexPositionInputs(input.positionOS);

                output.positionCS = pos.positionCS;

                output.color =
                    UNITY_ACCESS_INSTANCED_PROP(Props, _InstanceColor);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return input.color;
            }

            ENDHLSL
        }
    }
}