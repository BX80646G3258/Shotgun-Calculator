Shader "Calculator/SpreadPatternFilterShader"
{
    Properties
    {
        // _Count ("bullet count", Integer) = 6
    }
    SubShader
    {

        Pass
        {
            ZWrite Off

            CGPROGRAM
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
// #pragma exclude_renderers d3d11 gles
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

            int _Count;
            half2 _SpreadPatternA[10];
            half2 _SpreadPatternB[10];
            half _SpreadBlend;
            sampler2D _ModelPartTexture;
            float frag (v2f i) : SV_Target
            {
                int damageA = 0;
                int damageB = 0;
                half2 invTan = half2(unity_CameraProjection._m00, unity_CameraProjection._m11) / 2;
                for (int j = 0; j < _Count; j++)
                {
                    half2 offset = tan(_SpreadPatternA[j]) * invTan;
                    damageA += tex2D(_ModelPartTexture, i.uv + offset) * 1024;
                }
                for (int j = 0; j < _Count; j++)
                {
                    half2 offset = tan(_SpreadPatternB[j]) * invTan;
                    damageB += tex2D(_ModelPartTexture, i.uv + offset) * 1024;
                }
             
                return ((float) lerp(damageA, damageB, _SpreadBlend)) / 1024;
            }
            ENDCG
        }
    }
}
