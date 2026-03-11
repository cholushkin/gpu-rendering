Shader "Tutorial/SDF/SDFPreview"
{
    Properties
    {
        _ShapeType ("Shape Type (0=Circle 1=Box 2=Line 3=Cross)", Int) = 0

        _ShapeParamsA ("Shape Params A", Vector) = (0.35,0.3,0.05,0)
        _ShapeParamsB ("Shape Params B", Vector) = (0,0,0,0)

        _Edge ("Edge Width", Float) = 0.01

        _Color ("Shape Color", Color) = (1,1,1,1)
        _Background ("Background Color", Color) = (0,0,0,1)
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

            /*
            ============================================================
            STEP 01 — SDF PREVIEW
            ============================================================

            Features introduced:

            • Signed Distance Field (SDF) rendering
            • Procedural shapes evaluated in fragment shader
            • Minimal SDF shape library
            • Shape selection via _ShapeType
            • Generic parameter system for shapes
            • Smooth anti-aliased edges using smoothstep
            • UV to local coordinate conversion

            Generic Shape Parameters

            _ShapeParamsA
                x = radius / width
                y = height
                z = thickness
                w = reserved

            _ShapeParamsB
                x = offsetX
                y = offsetY
                z = rotation
                w = reserved

            Shapes

                0 → Circle
                1 → Box
                2 → Horizontal Line
                3 → Cross

            This parameter design allows future extensions like:

                • SDF command interpreter
                • GPU generated tile libraries
                • procedural vector graphics
            ============================================================
            */

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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

            int _ShapeType;

            float4 _ShapeParamsA;
            float4 _ShapeParamsB;

            float _Edge;

            float4 _Color;
            float4 _Background;

            //----------------------------------------------------------
            // Minimal SDF Shape Library
            //----------------------------------------------------------

            float sdCircle(float2 p, float radius)
            {
                return length(p) - radius;
            }

            float sdBox(float2 p, float2 size)
            {
                float2 d = abs(p) - size;
                return length(max(d,0)) + min(max(d.x,d.y),0);
            }

            float sdHorizontalLine(float2 p, float thickness)
            {
                return abs(p.y) - thickness;
            }

            float sdCross(float2 p, float thickness)
            {
                float d1 = abs(p.y) - thickness;
                float d2 = abs(p.x) - thickness;
                return min(d1, d2);
            }

            //----------------------------------------------------------
            // Shape dispatcher
            //----------------------------------------------------------

            float EvaluateShape(int type, float2 p)
            {
                float radius = _ShapeParamsA.x;
                float width  = _ShapeParamsA.x;
                float height = _ShapeParamsA.y;
                float thick  = _ShapeParamsA.z;

                if(type == 0)
                    return sdCircle(p, radius);

                if(type == 1)
                    return sdBox(p, float2(width, height));

                if(type == 2)
                    return sdHorizontalLine(p, thick);

                if(type == 3)
                    return sdCross(p, thick);

                return 1e5;
            }

            //----------------------------------------------------------

            Varyings vert(Attributes input)
            {
                Varyings o;

                VertexPositionInputs pos =
                    GetVertexPositionInputs(input.positionOS);

                o.positionCS = pos.positionCS;
                o.uv = input.uv;

                return o;
            }

            //----------------------------------------------------------

            half4 frag(Varyings input) : SV_Target
            {
                float2 p = input.uv - 0.5;

                // Apply optional offset
                p -= _ShapeParamsB.xy;

                float d = EvaluateShape(_ShapeType, p);

                float alpha = smoothstep(_Edge, -_Edge, d);

                return lerp(_Background, _Color, alpha);
            }

            ENDHLSL
        }
    }
}