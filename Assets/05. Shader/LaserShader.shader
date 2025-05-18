Shader "Unlit/LaserShader"
{
     Properties
    {
        _BaseColor("Base Color", Color) = (0, 1, 1, 1)
        _EmissionColor("Emission Color", Color) = (0, 1, 1, 1)
        _MainTex("Texture", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100
        Blend One One // Additive Blend
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _BaseColor;
            fixed4 _EmissionColor;
            sampler2D _MainTex;

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);
                return texColor * _BaseColor + _EmissionColor;
            }
            ENDCG
        }
    }
    FallBack Off
}
