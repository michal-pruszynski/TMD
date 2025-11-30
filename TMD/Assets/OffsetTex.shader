Shader "CustomRenderTexture/OffsetTex"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _BlankColor ("Blank Top Color", Color) = (0.5,0.5,0.5,1)
        _Floors ("Floors (from script)", Float) = 1
        _TopBlankFraction ("Top Blank Fraction", Range(0,0.9)) = 0.1
     }

     SubShader
     {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _BlankColor;
            float _Floors;
            float _TopBlankFraction;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // uv.y ranges ~[0 .. floors]
                float t = 0.0;
                if (_Floors > 0.0001)
                    t = saturate(i.uv.y / _Floors);   // 0 at bottom, 1 at top

                // if t greater than (1 - topBlankFraction) => we're in the top blank region
                float threshold = 1.0 - _TopBlankFraction;
                float isBlank = step(threshold, t);   // 0 below, 1 above threshold

                fixed4 texCol = tex2D(_MainTex, i.uv) * _Color;
                fixed4 col = lerp(texCol, _BlankColor, isBlank);

                return col;
            }
            ENDCG
        }
    }
}
