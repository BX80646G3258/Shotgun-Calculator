Shader "Calculator/SpreadPatternFilterShader"
{
    Properties
    {
        _Count ("bullet count", Integer) = 6
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
            float2 _SpreadPattern[10];
            // float4 _ScreenParams;
            sampler2D _ModelPartTexture;
            float frag (v2f i) : SV_Target
            {
                int damage = 0;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                for (int j = 0; j < _Count; j++)
                {
                    float2 offset = _SpreadPattern[j];
                    damage += tex2D(_ModelPartTexture, i.uv + float2(offset.x, offset.y * aspect)) * 1024;
                }
             
                return ((float) damage) / 1024;
            }
            ENDCG
        }
    }
}
