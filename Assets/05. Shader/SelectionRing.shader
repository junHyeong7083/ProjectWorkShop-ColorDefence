Shader "Unlit/SelectionRing"
{
    Properties
    {
        _Color ("Color", Color) = (0,1,0,1)
        _RingThickness ("Ring Thickness", Range(0.01, 1)) = 0.2
        _Alpha ("Alpha", Range(0,1)) = 0.8
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _Color;
            float _RingThickness;
            float _Alpha;

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
                o.uv = v.uv * 2 - 1; // -1~1로 변환해서 중심 기준 거리 측정
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float dist = length(i.uv);
                if(dist > 1.0 ) discard;
                float ring = smoothstep(1.0, 1.0 - _RingThickness, dist);
                float alpha = (1 - ring) * _Alpha;

                if(alpha<0.01) discard;


                return float4(_Color.rgb, alpha);
            }
            ENDCG
        }
    }
}
