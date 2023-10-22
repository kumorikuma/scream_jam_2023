// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL; // added normal
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1; // added normal
                UNITY_FOG_COORDS(2)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Lighting calculations
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz); // main light direction
                float3 norm = normalize(i.normal);
                float diff = max(0, dot(norm, lightDir)); // diffuse term (note the negative because light direction points from surface to light)
                
                // Since we're not using UnityGI, let's assume the main directional light color is white for simplicity. 
                // In a more complex scenario, you might pass this as a property.
                float3 lightColor = float3(1, 1, 1); 

                float3 litColor = diff * lightColor;

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * fixed4(litColor, 1.0);
                fixed4 tint = fixed4(1, 0, 0, 1);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col * tint;
            }
            ENDCG
        }
    }
}
