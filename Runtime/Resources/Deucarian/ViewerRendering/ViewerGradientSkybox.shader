Shader "Deucarian/Viewer/GradientSkybox"
{
    Properties
    {
        _TopColor ("Top", Color) = (0.216, 0.220, 0.216, 1)
        _HorizonColor ("Horizon", Color) = (0.251, 0.255, 0.251, 1)
        _BottomColor ("Bottom", Color) = (0.173, 0.173, 0.173, 1)
        _PrimaryTint ("Primary Tint", Color) = (0.251, 0.251, 0.251, 1)
        _PrimaryStrength ("Primary Strength", Range(0, 0.25)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Background"
            "RenderType" = "Background"
            "PreviewType" = "Skybox"
            "RenderPipeline" = "UniversalPipeline"
        }
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 direction : TEXCOORD0;
            };

            half4 _TopColor;
            half4 _HorizonColor;
            half4 _BottomColor;
            half4 _PrimaryTint;
            half _PrimaryStrength;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = UnityObjectToClipPos(input.positionOS);
                output.direction = input.positionOS.xyz;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half3 direction = normalize(input.direction);
                half horizonBlend = smoothstep(-0.34h, 0.26h, direction.y);
                half topBlend = smoothstep(0.08h, 0.88h, direction.y);
                half3 color = lerp(_BottomColor.rgb, _HorizonColor.rgb, horizonBlend);
                color = lerp(color, _TopColor.rgb, topBlend);

                half primaryBand = pow(saturate(1.0h - abs(direction.y - 0.18h)), 6.0h);
                color = lerp(color, _PrimaryTint.rgb, primaryBand * _PrimaryStrength);
                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
