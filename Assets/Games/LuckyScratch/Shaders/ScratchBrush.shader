// 마스크 RT에 브러시 스트로크(선분 SDF)를 누적하는 블릿 셰이더.
// _SegA→_SegB 선분에서 _Radius 이내를 긁힘(1)으로 마킹. max 누적.
Shader "LuckyScratch/ScratchBrush"
{
    Properties
    {
        _MainTex ("Prev Mask", 2D) = "black" {}
    }
    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _SegA;      // xy = uv
            float4 _SegB;      // xy = uv
            float _Radius;     // uv(높이) 기준 반경
            float _Hardness;   // 0~1, 경계 소프트니스
            float _Aspect;     // 티켓 width/height (uv 왜곡 보정)

            float distToSegment(float2 p, float2 a, float2 b)
            {
                float2 pa = p - a;
                float2 ba = b - a;
                float h = saturate(dot(pa, ba) / max(dot(ba, ba), 1e-6));
                return length(pa - ba * h);
            }

            fixed4 frag(v2f_img i) : SV_Target
            {
                float prev = tex2D(_MainTex, i.uv).r;

                // 가로 왜곡 보정: x축을 aspect로 스케일해 원형 브러시 유지
                float2 scale = float2(_Aspect, 1.0);
                float d = distToSegment(i.uv * scale, _SegA.xy * scale, _SegB.xy * scale);

                float inner = _Radius * saturate(_Hardness);
                float s = 1.0 - smoothstep(inner, _Radius, d);
                return max(prev, s);
            }
            ENDCG
        }
    }
    Fallback Off
}
