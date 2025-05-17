Shader "Custom/CircleRange"
{
   Properties
    {
        _Color("Color", Color) = (1,1,1,0.5)
        _Radius("Radius", Range(0,1)) = 0.5
        _Smooth("Smooth Edge", Range(0.001, 0.1)) = 0.02
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _Radius;
            float _Smooth;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv; // 0~1 범위 유지
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 centeredUV = i.uv - 0.5; // 중심으로 이동 (0.5,0.5 기준)
                float dist = length(centeredUV); // 중심에서 거리
                float alpha = smoothstep(_Radius, _Radius - _Smooth, dist);
                return fixed4(_Color.rgb, _Color.a * (1 - alpha));
            }
            ENDCG
        }
    }
}