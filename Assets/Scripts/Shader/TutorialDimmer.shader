Shader "UI/TutorialDimmer"
{
    Properties
    {
        [MainColor] _BaseColor("Dim Color", Color) = (0, 0, 0, 0.7)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define MAX_HOLES 8

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 screenUV : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
            CBUFFER_END

            int _HoleCount;
            // 구멍의 x,y 좌표 저장용
            float4 _HoleRects[MAX_HOLES];

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // 그려야 하는 지점을 계산하게 함
                OUT.positionHCS =
                    TransformObjectToHClip(IN.positionOS.xyz);

                // TRANSFORM_TEX = IN.uv * _BaseMap_ST.xy + _BaseMap_ST.zw
                OUT.uv =
                    TRANSFORM_TEX(IN.uv, _BaseMap);

                // 정점의 화면상 위치
                float4 screenPos =
                    ComputeScreenPos(OUT.positionHCS);

                // w를 이용해서 동차 좌표를 실제 화면 좌표로 변환
                OUT.screenUV =
                    screenPos.xy / screenPos.w;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                for (int i = 0; i < _HoleCount; i++)
                {
                    float4 rect = _HoleRects[i];

                    if (IN.screenUV.x >= rect.x &&
                        IN.screenUV.x <= rect.z &&
                        IN.screenUV.y >= rect.y &&
                        IN.screenUV.y <= rect.w)
                    {
                        // 해당 지점의 픽셀들을 제거해서 뒤의 버튼을 보이게 하는 것
                        discard;
                    }
                }

                // 다른 픽셀은 보여주기
                half4 color =
                    SAMPLE_TEXTURE2D(
                        _BaseMap,
                        sampler_BaseMap,
                        IN.uv
                    ) * _BaseColor;

                return color;
            }

            ENDHLSL
        }
    }
}