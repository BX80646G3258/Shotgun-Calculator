Shader "Calculator/SpreadFilterShader"
{
    Properties
    {
        _Radius ("radius", Float) = .0062
        _Quality ("quality", Range(0, 1)) = 1
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

            float _Radius;
            float _Quality;
            // float4 _ScreenParams;
            sampler2D _ModelPartTexture;
            float frag (v2f i) : SV_Target
            {
                float r2 = _Radius * _Radius;
                int damage = tex2D(_ModelPartTexture, i.uv) * 1024;
                int samples = 1;

                // return damage / 1024;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float yStep = (1 / _ScreenParams.y) / _Quality;
                float xStep = (1 / _ScreenParams.x) / _Quality;
                for (float y = yStep; y < _Radius * aspect; y += yStep)
                {
                    float yScl = y / aspect;
                    float x2Lim = r2 - yScl * yScl;
                    for (float x = 0; x * x < x2Lim; x += xStep)
                    {
                        damage += tex2D(_ModelPartTexture, i.uv + float2(x, y)) * 1024;
                        damage += tex2D(_ModelPartTexture, i.uv + float2(x, -y)) * 1024;
                        damage += tex2D(_ModelPartTexture, i.uv + float2(-x, y)) * 1024;
                        damage += tex2D(_ModelPartTexture, i.uv + float2(-x, -y)) * 1024;
                        samples += 4;
                    }
                }
                return (((float) damage) / samples) / 1024;
            }
            ENDCG
        }
    }
}
