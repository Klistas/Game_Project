// 은박 레이어 표시 셰이더. _MaskTex(긁힘 마스크)의 r값이 클수록 투명해진다.
// 절차적 노이즈로 은박 질감 표현 (텍스처 에셋 불필요 — 프로토 단계).
Shader "LuckyScratch/ScratchFoil"
{
    Properties
    {
        _Color ("Foil Color", Color) = (0.76, 0.76, 0.80, 1)
        _Color2 ("Foil Color 2", Color) = (0.55, 0.56, 0.62, 1)
        _MaskTex ("Scratch Mask", 2D) = "black" {}
        _NoiseScale ("Noise Scale", Float) = 90
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            fixed4 _Color2;
            sampler2D _MaskTex;
            float _NoiseScale;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float mask = tex2D(_MaskTex, i.uv).r;

                // 은박 질감: 거친 노이즈 + 대각 브러시드 메탈 하이라이트
                float n = hash21(floor(i.uv * _NoiseScale));
                float streak = 0.5 + 0.5 * sin((i.uv.x + i.uv.y) * 60.0 + n * 4.0);
                fixed3 foil = lerp(_Color2.rgb, _Color.rgb, n * 0.6 + streak * 0.4);

                return fixed4(foil, 1.0 - mask);
            }
            ENDCG
        }
    }
    Fallback Off
}
