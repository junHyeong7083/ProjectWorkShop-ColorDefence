Shader "Unlit/ExpandingRing"
{
    Properties
    {
        _Color("Color", Color) = (0,1,0,1)
        _Radius("Radius", Float) = 0.2
        _LineWidth("Line Width", Float) = 0.02
        _FadeOut("Fade Out Start Time", Float) = 0.8
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _Radius;
            float _LineWidth;
            float _FadeOut;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * 2 - 1; // [-1,1] 좌표계
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dist = length(i.uv);
                
                // 외곽선 영역 계산
                float edge = smoothstep(_Radius, _Radius - _LineWidth, dist) *
                             smoothstep(_Radius + _LineWidth, _Radius, dist);

                // 알파 Fade
                float fade = saturate((_FadeOut - _Radius) / _FadeOut);
                return fixed4(_Color.rgb, edge * fade * _Color.a);
            }
            ENDCG
        }
    }
}
