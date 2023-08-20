Shader "Calculator/SpreadFilterShader"
{
    Properties
    {
        // _Radius ("radius", Float) = .0062
        _Quality ("quality", Range(0, 1)) = 1
        _MaxSamples ("max samples", Range(1, 16000)) = 1000
        [KeywordEnum(NoSpread, ShowSpread, Average, Minimum, Maximum)] _Mode ("Mode", Float) = 0
        [KeywordEnum(Dynamic, Fixed)] _Samples ("Sampling", float) = 0
    }
    SubShader
    {

        Pass
        {
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _MODE_NOSPREAD _MODE_SHOWSPREAD _MODE_AVERAGE _MODE_FLATAVERAGE _MODE_MINIMUM _MODE_MAXIMUM
            #pragma multi_compile _SAMPLES_DYNAMIC _SAMPLES_FIXED

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

            // #define FIXED_SAMPLES 0
            #define FIXED_SAMPLE_COUNT 4

            float _Radius;
            float _Quality;
            float _MaxSamples;
            sampler2D _ModelPartTexture;

            float2 random2(float2 uv)
            {
                float2x2 m = float2x2(16.04, 496.35, 262.85, 536.69);
                return frac(sin(mul(m, uv)) * 85351.22);
            }

            float frag (v2f i) : SV_Target
            {
                #if defined(_MODE_NOSPREAD)
                    return tex2D(_ModelPartTexture, i.uv);
                #endif

                float2 invTan = float2(unity_CameraProjection._m00, unity_CameraProjection._m11);
                float2 pixelToSC = 1 / _ScreenParams.xy;
                float pixelRadius = _Radius * invTan.x * _ScreenParams.x;
                int r2 = pixelRadius * pixelRadius;
                float damage = tex2D(_ModelPartTexture, i.uv) * 1024;
                float sampleCount = 1;
                
                #if defined(_SAMPLES_FIXED)
                    float step = pixelRadius / FIXED_SAMPLE_COUNT;
                    i.uv += ((random2(i.uv) - .5) * pixelRadius) * pixelToSC;
                    //i.uv += (random2(i.uv) - .5) * float2(xStep, yStep);
                #else
                    half step = 1 / _Quality;

                    half estSampleCount = UNITY_PI * pow(pixelRadius, 2);
                    half correction = estSampleCount / _MaxSamples;
                    if(correction > 1)
                    step *= correction;
                #endif

                #if defined(_MODE_SHOWSPREAD)
                    return length((i.uv - .5) * _ScreenParams.xy) < pixelRadius ? 20.f / 1024 : 0;
                #endif

                #if defined(_MODE_AVERAGE)
                    sampleCount = 7.0509887 / step;
                    damage *= sampleCount;
                    // sampleCount = 0;
                    // damage = 0;
                #endif

                for (half y = step; y < pixelRadius; y += step)
                {
                    for (half x = 0; x < pixelRadius; x += step)
                    {
                        half2 pCoord = half2(x, y);
                        half dist = length(pCoord);
                        half invDist = 1 / dist;
                        if(dist >= pixelRadius)
                        break;


                        [unroll]
                        for (int q = 0; q < 4; q++)
                        {
                            #if defined(_MODE_FLATAVERAGE)
                                damage += tex2D(_ModelPartTexture, i.uv + pCoord * pixelToSC) * 1024;
                                sampleCount++;
                            #elif defined(_MODE_AVERAGE)
                                damage += round(tex2D(_ModelPartTexture, i.uv + pCoord * pixelToSC) * 1024) * invDist;
                                sampleCount += invDist;
                            #elif defined(_MODE_MINIMUM)
                                damage = min(tex2D(_ModelPartTexture, i.uv + pCoord * pixelToSC) * 1024, damage);
                            #elif defined(_MODE_MAXIMUM)
                                damage = max(tex2D(_ModelPartTexture, i.uv + pCoord * pixelToSC) * 1024, damage);
                            #endif
                            pCoord = half2(-pCoord.y, pCoord.x);
                        }
                    }
                }

                return (floor((damage + .01) / sampleCount)) / 1024;
            }
            ENDCG
        }
    }
}
