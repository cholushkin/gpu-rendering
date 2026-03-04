Shader "Custom/GPUInstancedGPU"
{
    Properties
    {
        _MainTex("Texture",2D)="white"{}
    }

    SubShader
    {
        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            struct InstanceData
            {
                float4x4 transform;
            };

            StructuredBuffer<InstanceData> _Instances;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes IN, uint instanceID : SV_InstanceID)
            {
                Varyings OUT;

                InstanceData inst = _Instances[instanceID];

                float4 worldPos = mul(inst.transform,float4(IN.positionOS,1));

                OUT.positionCS = mul(UNITY_MATRIX_VP,worldPos);
                OUT.uv = IN.uv;

                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                return tex2D(_MainTex,IN.uv);
            }

            ENDHLSL
        }
    }
}