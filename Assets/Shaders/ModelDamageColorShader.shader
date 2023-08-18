Shader "Calculator/ModelDamageColorShader"
{
    Properties
    {
        _Radius ("radius", Range(0, 10)) = 1
        _Quality ("quality", Range(0, 1)) = 1
        _NormalWeight ("normal outline weight", Range(0, 1)) = .9
        _DepthWeight ("depth outline weight", Range(0, 1)) = .9
        _Margin ("outline margin", Range(0, 1)) = .1
        _Threshold ("outline threshold", Range(0, 1)) = .5
        _Brightness ("outline brightness", Range(-1, 1)) = .5
        _MaxHits("max hits", Integer) = 8
        _Remainder("show remaining", Range(0, 1)) = 0
        _ColorRange("color range", Range(0, 1)) = 1
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

            SamplerState point_clamp_sampler;
            Texture2D _ModelPartTexture;
            Texture2D _ModelNormalsTexture;
            half _Radius;
            half _Quality;
            half _NormalWeight;
            half _DepthWeight;
            half _Margin;
            half _Threshold;
            half _Brightness;
            float _Remainder;
            sampler2D _CameraDepthTexture;

            int _MaxHits;
            float _ColorRange;

            half3 getNormal(float2 uv)
            {
                half3 output = _ModelNormalsTexture.Sample(point_clamp_sampler, uv);
                if(output.x + output.y + output.z == 0)
                return half3(0, 0, 0);
                else return output * 2 - 1;
            }

            float checkNormal(float2 uv, half3 normal)
            {
                // return dot(getNormal(uv), normal) < _Threshold;
                return saturate(1 - dot(getNormal(uv), normal));
                // return 1 - dot(getNormal(uv), normal);
            }

            float checkDepth(float2 uv, float depth)
            {
                return abs(depth - DECODE_EYEDEPTH(tex2D(_CameraDepthTexture, uv)));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half normalOutline = 0;
                half depthOutline = 0;

                float r2 = _Radius * _Radius;
                half3 normal = getNormal(i.uv);
                float depth = DECODE_EYEDEPTH(tex2D(_CameraDepthTexture, i.uv));

                float step = 1 / _Quality;
                float2 pixelToSC = 1 / _ScreenParams.xy;

                for (float y = 0; y <= _Radius; y += step)
                {
                    float x2Lim = (r2 - y * y);
                    for (float x = step; x * x <= x2Lim; x += step)
                    {
                        half2 pCoord = half2(x, y);
                        for (int q = 0; q < 4; q++)
                        {
                            float2 sampleCoord = i.uv + pCoord * pixelToSC;
                            normalOutline += checkNormal(sampleCoord, normal);
                            depthOutline += checkDepth(sampleCoord, depth);
                            pCoord = half2(-pCoord.y, pCoord.x);
                        }
                    }
                }

                if(dot(normal, normal) == 0)
                    normalOutline = 0;
                float outline = normalOutline * _NormalWeight + depthOutline * _DepthWeight;
                outline = (outline - _Threshold) / _Margin;

                int damage = round(_ModelPartTexture.Sample(point_clamp_sampler, i.uv) * 1024);
                // if(damage == 0)
                //     return fixed4(0, 0, 0, 0);
                float hits = (100.0 / damage);
                int iHits = ceil(hits);
                float theta = -UNITY_TWO_PI * _ColorRange * saturate(((float) iHits - 1) / _MaxHits);
                half3 color = half3(
                (cos(theta) + 1) / 2,
                (cos(theta + UNITY_TWO_PI / 3) + 1) / 2,
                (cos(theta + UNITY_FOUR_PI / 3) + 1) / 2
                );
                color = lerp(color, 0, _Remainder * (1 - saturate(hits - iHits)));

                if(hits > _MaxHits)
                color = 0;

                color += _Brightness * saturate(outline);

                return fixed4(color, 1);
                // return normalOutline;
                // return half4(getNormal(i.uv), 1);
                
            }
            ENDCG
        }
    }
}
