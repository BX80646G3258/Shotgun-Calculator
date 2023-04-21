Shader "Calculator/SpreadFilterShader"
{
    Properties
    {
        // _Radius ("radius", Float) = .0062
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

            #define FIXED_SAMPLES 0
            #define FIXED_SAMPLE_COUNT 8

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

                float2 invTan = float2(unity_CameraProjection._m00, unity_CameraProjection._m11);
                float r2 = _Radius * _Radius;
                int damage = tex2D(_ModelPartTexture, i.uv) * 1024;
                int samples = 1;

                #if defined(_MODE_SHOWSPREAD)
                return length((i.uv - .5) / invTan) < _Radius ? 20.f / 1024 : 0;
                #endif

                float yLim = _Radius * invTan.y;
                #if FIXED_SAMPLES
                float yStep = yLim / FIXED_SAMPLE_COUNT;
                float xStep = yStep * (_ScreenParams.y / _ScreenParams.x);
                #else
                float yStep = (1 / _ScreenParams.y) / _Quality;
                float xStep = (1 / _ScreenParams.x) / _Quality;
                #endif

                #if FIXED_SAMPLES
                [unroll]
                for (int sy = 1; sy < FIXED_SAMPLE_COUNT; sy++)
                #else
                for (float y = yStep; y < yLim; y += yStep)
                #endif
                {                    
                    #if FIXED_SAMPLES
                    float y = yStep * sy;
                    int xsLim2 = pow(FIXED_SAMPLE_COUNT, 2) - pow(sy, 2);

                    [unroll]
                    for (int sx = 0; sx * sx < xsLim2; sx++)

                    #else

                    float xLim2 = (r2 - pow(y / invTan.y, 2)) * pow(invTan.x, 2);

                    for (float x = 0; x * x < xLim2; x += xStep)
                    #endif
                    {
                        #if FIXED_SAMPLES
                        float x = xStep * sx;
                        #endif

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
