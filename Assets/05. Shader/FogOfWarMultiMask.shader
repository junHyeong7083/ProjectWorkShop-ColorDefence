Shader "Custom/FogOfWarMultiMask_Buffer"
{
   Properties
    {
        _FogColor    ("Fog Color", Color) = (0, 0, 0, 1)
        _RevealCount ("Reveal Count", Int) = 0
        _FadeRange   ("Fade Range (UV)", Float) = 0.05
         _Aspect      ("Aspect Ratio", Float) = 1.0 
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100

        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off
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

            float4 _FogColor;
            int _RevealCount;
            float _FadeRange;
            float _Aspect;

            StructuredBuffer<float2> _SB_RevealCenters;
            StructuredBuffer<float>  _SB_RevealRadii;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float alpha = 1.0; // 기본 완전 불투명

                for (int idx = 0; idx < _RevealCount; idx++)
                {
                    float2 center = _SB_RevealCenters.Load(idx);
                    float radius  = _SB_RevealRadii.Load(idx);
                    float dist    = distance(uv, center);

                    if (dist < radius)
                    {
                        alpha = 0.0; // 중심은 완전 투명
                        break;
                    }
                    else if (dist < radius + _FadeRange)
                    {
                        float t = (dist - radius) / _FadeRange;
                        alpha = min(alpha, t); // 그라데이션 처리 (겹치면 더 투명한 쪽 우선)
                    }
                }

                return float4(_FogColor.rgb, alpha);
            }
            ENDCG
        }
    }

    FallBack Off
}
