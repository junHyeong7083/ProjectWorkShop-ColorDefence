Shader "Unlit/MinimapCursor"
{Properties
    {
        _Color ("Color", Color) = (0.2, 0.5, 1, 1)
        _RectMin ("Hole Rect Min UV", Vector) = (0.3, 0.3, 0, 0)
        _RectMax ("Hole Rect Max UV", Vector) = (0.7, 0.7, 0, 0)
        _EdgeWidth ("Edge Width", Float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _Color;
            float4 _RectMin;
            float4 _RectMax;
            float _EdgeWidth;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 rectMin = _RectMin.xy;
                float2 rectMax = _RectMax.xy;

                // 사각형 내부는 완전 투명
                if (uv.x >= rectMin.x && uv.x <= rectMax.x &&
                    uv.y >= rectMin.y && uv.y <= rectMax.y)
                {
                    return float4(0, 0, 0, 0);
                }

                // 거리: 사각형 테두리에서 얼마나 떨어져 있는가
                float2 edgeDist = min(abs(uv - rectMin), abs(uv - rectMax));
                float minEdge = min(edgeDist.x, edgeDist.y);

                // 그라데이션 알파: 외곽부터 안쪽으로 alpha 1 → 0
                float alpha = smoothstep(0.0, _EdgeWidth, minEdge);

                return float4(_Color.rgb, _Color.a * alpha);
            }
            ENDCG
        }
    }
}
