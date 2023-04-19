Shader "Calculator/SpreadFilterShader"
{
    Properties
    {
        _Radius ("radius", Float) = .0062
        _Quality ("quality", Range(0, 1)) = 1
        [KeywordEnum(NoSpread, ShowSpread, Average, Minimum, Maximum)] _Mode ("Mode", Float) = 0
    }
    SubShader
    {

        Pass
        {
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _MODE_NOSPREAD _MODE_SHOWSPREAD _MODE_AVERAGE _MODE_MINIMUM _MODE_MAXIMUM

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
                _Radius /= 2;
                #if defined(_MODE_NOSPREAD)
                return tex2D(_ModelPartTexture, i.uv);
                #endif

                float2 invTan = float2(unity_CameraProjection._m00, unity_CameraProjection._m11) * -_ProjectionParams.x;
                float r2 = _Radius * _Radius;
                int damage = tex2D(_ModelPartTexture, i.uv) * 1024;
                int samples = 1;

                #if defined(_MODE_SHOWSPREAD)
                return length((i.uv - .5) / invTan) < _Radius ? 100.f / 1024 : 0;
                #endif

                // return damage / 1024;
                // float aspect = _ScreenParams.y / _ScreenParams.x;
                float yLim = _Radius * invTan.y;
                float yStep = (1 / _ScreenParams.y) / _Quality;
                float xStep = (1 / _ScreenParams.x) / _Quality;
                for (float y = yStep; y < yLim; y += yStep)
                {
                    float xLim2 = (r2 - pow(y / invTan.y, 2)) * pow(invTan.x, 2);
                    for (float x = 0; x * x < xLim2; x += xStep)
                    {
                        #if defined(_MODE_AVERAGE)
                        damage += tex2D(_ModelPartTexture, i.uv + float2(x, y)) * 1024;
                        damage += tex2D(_ModelPartTexture, i.uv + float2(x, -y)) * 1024;
                        damage += tex2D(_ModelPartTexture, i.uv + float2(-x, y)) * 1024;
                        damage += tex2D(_ModelPartTexture, i.uv + float2(-x, -y)) * 1024;
                        samples += 4;
                        #elif defined(_MODE_MINIMUM)
                        damage = min(tex2D(_ModelPartTexture, i.uv + float2(x, y)) * 1024, damage);
                        damage = min(tex2D(_ModelPartTexture, i.uv + float2(x, -y)) * 1024, damage);
                        damage = min(tex2D(_ModelPartTexture, i.uv + float2(-x, y)) * 1024, damage);
                        damage = min(tex2D(_ModelPartTexture, i.uv + float2(-x, -y)) * 1024, damage);
                        #elif defined(_MODE_MAXIMUM)
                        damage = max(tex2D(_ModelPartTexture, i.uv + float2(x, y)) * 1024, damage);
                        damage = max(tex2D(_ModelPartTexture, i.uv + float2(x, -y)) * 1024, damage);
                        damage = max(tex2D(_ModelPartTexture, i.uv + float2(-x, y)) * 1024, damage);
                        damage = max(tex2D(_ModelPartTexture, i.uv + float2(-x, -y)) * 1024, damage);
                        #endif
                    }
                }
                return (((float) damage) / samples) / 1024;
            }
            ENDCG
        }
    }
}
