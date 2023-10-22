Shader "Custom/EdgeDetection"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _EdgeColor("Edge Color", Color) = (0,0,0,1)
        _EdgeWidth("Edge Width", Range(0.001, 0.03)) = 0.01
    }

    SubShader
    {
        Pass
        {
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _EdgeColor;
            float _EdgeWidth;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float DepthEdgeDetect(float2 uv)
            {
                float depth = tex2D(_MainTex, uv).r;
                float2 offsets = _MainTex_TexelSize.xy * _EdgeWidth;
                float edge = 0;

                // Sample surrounding depths
                for(int y = -1; y <= 1; y++)
                {
                    for(int x = -1; x <= 1; x++)
                    {
                        float sample = tex2D(_MainTex, uv + float2(x,y) * offsets).r;
                        edge += abs(sample - depth);
                    }
                }
                return saturate(edge * 5);  // Multiply to increase the contrast.
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv);
                // float edge = DepthEdgeDetect(i.uv);
                // return lerp(col, _EdgeColor, edge);
                return col;
            }
            ENDCG
        }
    }
}
