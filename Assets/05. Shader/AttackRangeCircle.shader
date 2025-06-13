Shader "Unlit/AttackRangeCircle"
{
   Properties
    {
        _Color("Color", Color) = (1, 0, 0, 0.3) // 반투명 빨강
        _Radius("Radius", Range(0, 1)) = 1.0
        _Softness("Edge Softness", Range(0.001, 1)) = 0.1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _Radius;
            float _Softness;

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
                o.uv = v.uv * 2.0 - 1.0; // -1~1로 변환 (중앙기준)
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dist = length(i.uv);
                if (dist > _Radius) discard;

                // 가장자리 부드럽게
                float edgeFade = smoothstep(_Radius, _Radius - _Softness, dist);
                fixed4 col = _Color;
                col.a *= (1.0 - edgeFade);

                if (col.a < 0.01) discard;

                return col;
            }
            ENDCG
        }
    }
}
