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

            // �Ȱ� ��
            float4 _FogColor;
            // ���� Ȱ��ȭ�� �ͷ�(����) ����
            int _RevealCount;

            // StructuredBuffer ����: C# ComputeBuffer���� SetBuffer �� �� ���� ���⼭ �޴´�.
            StructuredBuffer<float2> _SB_RevealCenters; // (u,v) ��
            StructuredBuffer<float>  _SB_RevealRadii;   // �ݰ�(uv ����)

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

                // ��RevealCount�� ��ŭ�� StructuredBuffer�� �о �ݺ�
                // (StructuredBuffer�� �ε����� ����)
                for (int idx = 0; idx < _RevealCount; idx++)
                {
                    float2 centerUV = _SB_RevealCenters.Load(idx);
                    float  radius   = _SB_RevealRadii.Load(idx);
                    float2 d        = uv - centerUV;
                    if (dot(d, d) <= radius * radius)
                    {
                        col.a = 0; // ���� ���� �� ����
                        return col;
                    }
                }

                // ������ �ƴ� ���� �� �Ȱ� ��
                return col;
            }
            ENDCG
        }
    }
    FallBack Off
}
