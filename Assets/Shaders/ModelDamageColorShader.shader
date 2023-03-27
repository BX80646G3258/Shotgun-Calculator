Shader "Calculator/ModelDamageColorShader"
{
    Properties
    {
        _Radius ("radius", Float) = .001
        _Quality ("quality", Range(0, 1)) = 1
        _Threshold ("outline threshold", Range(.6, 1)) = .9
        _Brightness ("outline brightness", Range(-1, 1)) = .5
        _MaxHits("max hits", Integer) = 8
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

            sampler2D _ModelPartTexture;
            sampler2D _ModelNormalsTexture;
            float _Radius;
            float _Quality;
            float _Threshold;
            float _Brightness;

            int _MaxHits;
            float _ColorRange;

            half3 getNormal(float2 uv)
            {
                half3 output = tex2D(_ModelNormalsTexture, uv);
                if(output.x + output.y + output.z == 0)
                    return half3(0, 0, 0);
                else return output * 2 - 1;
            }

            bool checkNormal(float2 uv, half3 normal)
            {
                return dot(getNormal(uv), normal) < _Threshold;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                bool outline = false;

                float r2 = _Radius * _Radius;
                half3 normal = getNormal(i.uv);

                float aspect = _ScreenParams.x / _ScreenParams.y;
                float yStep = (1 / _ScreenParams.y) / _Quality;
                float xStep = (1 / _ScreenParams.x) / _Quality;

                for (float y = yStep; y < _Radius * aspect; y += yStep)
                {
                    float yScl = y / aspect;
                    float x2Lim = r2 - yScl * yScl;
                    for (float x = 0; x * x < x2Lim; x += xStep)
                    {
                        if( checkNormal(i.uv + float2(x, y), normal) || 
                            checkNormal(i.uv + float2(x, -y), normal) || 
                            checkNormal(i.uv + float2(-x, y), normal) || 
                            checkNormal(i.uv + float2(-x, -y), normal))
                        {
                            outline = true;
                        }
                    }
                }

                if(dot(normal, normal) == 0)
                {
                    outline = false;
                }

                float damage = tex2D(_ModelPartTexture, i.uv) * 1024;
                if(damage == 0)
                    return fixed4(0, 0, 0, 0);
                int hits = (100 / damage);
                float theta = -UNITY_TWO_PI * _ColorRange * saturate(((float) hits) / _MaxHits);
                half3 color = half3(
                    (cos(theta) + 1) / 2,
                    (cos(theta + UNITY_TWO_PI / 3) + 1) / 2,
                    (cos(theta + UNITY_FOUR_PI / 3) + 1) / 2
                );

                if(hits > _MaxHits)
                    color = 0;

                if(outline)
                    color += _Brightness;

                return fixed4(color, 1);
                
            }
            ENDCG
        }
    }
}
