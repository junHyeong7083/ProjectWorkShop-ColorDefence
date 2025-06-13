Shader "Unlit/DashedLine"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Texture", 2D) = "white" {}
        _DashSize("Dash Size (World Unit)", Float) = 1.0
        _WorldLength("Line World Length", Float) = 10.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _DashSize;
            float _WorldLength;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dashCount = max(_WorldLength / _DashSize, 1.0); // √÷º“ 1∞≥
                float dash = fmod(i.uv.x * dashCount, 2.0);
                if (dash < 1.0)
                    discard;

                return _Color;
            }
            ENDCG
        }
    }
}
