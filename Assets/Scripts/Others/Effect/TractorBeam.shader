Shader "Custom/TractorBeam"
{
    Properties
    {
        _Color ("Color", Color) = (0.6, 0.9, 1, 0.6)
        _Intensity ("Intensity", Float) = 1.5
        _Stripes ("Vertical Stripes (정수)", Float) = 8
        _StripeScroll ("Stripe Scroll", Float) = 0.15
        _BandScroll ("Rising Band Speed", Float) = 1.5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend (1=Additive, 10=Alpha)", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha [_DstBlend]
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _Intensity, _Stripes, _StripeScroll, _BandScroll;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // uv.y: 0 = 바닥, 1 = 하늘 쪽 끝
                float fade = saturate(i.uv.y * 10.0) * pow(saturate(1.0 - i.uv.y), 1.5);

                // 세로 결(빛줄기 느낌) - Stripes가 정수여야 이음매가 안 보임
                float stripe = 0.65 + 0.35 * sin((i.uv.x * _Stripes + _Time.y * _StripeScroll) * 6.2831);

                // 위로 올라가는 밝은 띠
                float band = 0.8 + 0.2 * sin(i.uv.y * 25.0 - _Time.y * _BandScroll);

                float a = _Color.a * fade * stripe * band;
                return fixed4(_Color.rgb * _Intensity, a);
            }
            ENDCG
        }
    }
}