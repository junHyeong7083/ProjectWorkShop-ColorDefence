Shader "Custom/FogOfWarMultiMask_Buffer"
{
    Properties
    {
        _FogColor    ("Fog Color", Color) = (0, 0, 0, 1)
        _RevealCount ("Reveal Count", Int)  = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            ZTest Always
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            // 안개 색
            float4 _FogColor;
            // 실제 활성화된 터렛(구멍) 개수
            int _RevealCount;

            // StructuredBuffer 선언: C# ComputeBuffer에서 SetBuffer 해 준 값을 여기서 받는다.
            StructuredBuffer<float2> _SB_RevealCenters; // (u,v) 쌍
            StructuredBuffer<float>  _SB_RevealRadii;   // 반경(uv 단위)

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                fixed4 col = _FogColor;

                // “RevealCount” 만큼만 StructuredBuffer를 읽어서 반복
                // (StructuredBuffer를 인덱스로 접근)
                for (int idx = 0; idx < _RevealCount; idx++)
                {
                    float2 centerUV = _SB_RevealCenters.Load(idx);
                    float  radius   = _SB_RevealRadii.Load(idx);
                    float2 d        = uv - centerUV;
                    if (dot(d, d) <= radius * radius)
                    {
                        col.a = 0; // 구멍 안쪽 → 투명
                        return col;
                    }
                }

                // 구멍이 아닌 영역 → 안개 색
                return col;
            }
            ENDCG
        }
    }
    FallBack Off
}
