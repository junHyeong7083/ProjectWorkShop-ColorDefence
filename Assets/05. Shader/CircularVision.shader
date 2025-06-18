
Shader "Unlit/CircularVision"
{
    Properties
    {
        _Color("Color", Color) = (0.0, 0.5, 1.0, 1.0)
        _Radius("Radius", Range(0, 1)) = 0.5
        _Softness("Edge Softness", Range(0.01, 1)) = 0.2
        _MinAlpha("Minimum Alpha", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _Radius;
            float _Softness;
            float _MinAlpha;

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
                o.uv = v.uv * 2.0 - 1.0;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dist = length(i.uv);
                if (dist > 1.0) discard;

                   float fade = smoothstep(_Radius - _Softness, _Radius, dist);
                float finalAlpha = lerp(_Color.a, _MinAlpha, fade);

                return fixed4(_Color.rgb, finalAlpha);
            }
            ENDCG
        }
    }
}
