Shader "Unlit/ModelPartShader"
{
    Properties
    {
        _ModelPart ("Model Part", Integer) = 0
    }
    SubShader
    {
        Pass
        {
            Tags { "LightMode" = "ModelPartTag" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            int _ModelPart;
            float frag (v2f i) : SV_Target
            {
                return _ModelPart / 1024.f;
                // return _ModelPart;
                // return 1;
            }
            ENDCG
        }
        
        Pass
        {
            Tags { "LightMode" = "Normals" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata 
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            half3 frag (v2f i) : SV_TARGET
            {
                half3 normal = normalize(i.normal);
                return (normal + 1) / 2;
            }

            ENDCG
        }

        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        // UsePass "Universal Render Pipeline/Lit/DepthNormals"
    }
}
