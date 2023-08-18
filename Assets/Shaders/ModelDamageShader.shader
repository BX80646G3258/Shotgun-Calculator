Shader "Calculator/ModelDamageShader"
{
    Properties
    {
        // _HelmetRatio ("helmet armor ratio", Float) = 2
        // _KevlarRatio ("kevlar armor ratio", Float) = 2
        // _Damage ("base damage", Integer) = 36
        // _RangeMod ("range modifier", Float) = .98
        // _Range ("max range", Integer) = 8192
        // _HeadMul ("head multiplier", Float) = 4
        // _ChestMul ("chest multiplier", Float) = 1
        // _StomachMul ("stomach multiplier", Float) = 1.25
        // _LegMul ("leg multiplier", Float) = .75
        _HUPerMeter ("hammer units per meter", Float) = 39.37008
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

            float _HelmetRatio;
            float _KevlarRatio;
            float _Damage;
            float _RangeMod;
            float _Range;
            float _HeadMul;
            float _ChestMul;
            float _StomachMul;
            float _LegMul;
            float _HUPerMeter;
            sampler2D _ModelPartTexture;
            sampler2D _CameraDepthTexture;

            float depthToDist(float depth, float2 uv)
            {
                float2 invTan = float2(unity_CameraProjection._m00, unity_CameraProjection._m11) / 2;
                float2 xy = depth * ((uv - .5) / invTan);
                return length(float3(xy, depth)) * _HUPerMeter;
            }

            float frag (v2f i) : SV_Target
            {
                int part = tex2D(_ModelPartTexture, i.uv) * 1024;
                float dist = DECODE_EYEDEPTH(tex2D(_CameraDepthTexture, i.uv));
                dist = depthToDist(dist, i.uv);
                float damage = _Damage;

                if (part == 1)
                    damage *= (_HelmetRatio / 2) * _HeadMul;
                else if (part == 2 || part == 8 || part == 4 || part == 5)
                    damage *= (_KevlarRatio / 2) * _ChestMul;
                else if (part == 3)
                    damage *= (_KevlarRatio / 2) * _StomachMul;
                else if (part == 6 || part == 7)
                    damage *= _LegMul;
                else
                    damage *= 0;

                damage *= dist > _Range ? 0 : pow(_RangeMod, dist / 500);

                return min(floor(damage), 100) / 1024;
            }
            ENDCG
        }
    }
}
