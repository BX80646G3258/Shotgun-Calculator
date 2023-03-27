Shader "Calculator/ModelPartColorShader"
{
    Properties
    {
    }
    SubShader
    {
        Pass
        {
            ZWrite Off

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
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _ModelPartTexture;
            fixed4 frag (v2f i) : SV_Target
            {
                int part = tex2D(_ModelPartTexture, i.uv) * 1024;
                if (part > 0)
                {
                    fixed4 color = fixed4((part & 1) > 0, (part & 2) > 0, (part & 4) > 0, 1);
                    return color;
                }
                else
                {
                    return 0;
                }
            }
            ENDCG
        }
    }
}
