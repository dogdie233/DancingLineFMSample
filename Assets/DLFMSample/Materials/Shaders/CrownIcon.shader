Shader "Unlit/CrownIcon"
{
    Properties
    {
        [NoScaleOffset] _OutTex ("OutTexture", 2D) = "white" {}
        [NoScaleOffset] _InTex ("InTexture", 2D) = "white" {}
        _Fade ("Fade", Range(0 , 1)) = 0
        _Color ("FadeColor", Color) = (1, 1, 1, 1)
        _OutColor ("OutColor", Color) = (0, 0, 0, 1)
    }
    SubShader
    {
        Tags { "QUEUE" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _OutTex;
            sampler2D _InTex;
            float4 _OutTex_ST;
            float4 _InTex_ST;
            float _Fade;
            fixed4 _Color;
            fixed4 _OutColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _OutTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 inCol = tex2D(_InTex, i.uv);
                fixed4 outCol = tex2D(_OutTex, i.uv);
                fixed4 col = sqrt(inCol * _Color * inCol.w) * _Fade + sqrt(outCol * _OutColor * outCol.w);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
