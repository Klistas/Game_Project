// PSX/CRT 풀스크린 포스트 필터. 씬 RenderTexture(_MainTex)를 받아
// 배럴 왜곡 + 색수차 + 스캔라인 + 비네트 + 그레인/플리커를 입힌다.
Shader "NightShift/CrtFilter"
{
    Properties
    {
        _MainTex ("Scene", 2D) = "black" {}
        _Curvature ("Barrel Curvature", Float) = 0.07
        _Aberration ("Chromatic Aberration", Float) = 0.0016
        _ScanIntensity ("Scanline Intensity", Range(0,1)) = 0.12
        _ScanCount ("Scanline Count", Float) = 400
        _Vignette ("Vignette", Range(0,2)) = 1.1
        _Grain ("Grain", Range(0,1)) = 0.03
        _Flicker ("Flicker", Range(0,1)) = 0.025
        _Fade ("Fade To Black", Range(0,1)) = 0
        _Glitch ("Glitch", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Curvature, _Aberration, _ScanIntensity, _ScanCount, _Vignette, _Grain, _Flicker, _Fade, _Glitch;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert(appdata v){ v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }

            float hash(float2 p){ p = frac(p*float2(123.34,456.21)); p += dot(p,p+45.32); return frac(p.x*p.y); }

            // 배럴(볼록) 왜곡
            float2 barrel(float2 uv, float k)
            {
                float2 c = uv*2.0-1.0;
                float r2 = dot(c,c);
                c *= 1.0 + k*r2;
                return c*0.5+0.5;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = barrel(i.uv, _Curvature);

                // 글리치: 스캔라인 블록 수평 찢김 + 세로 롤
                float aber = _Aberration;
                if (_Glitch > 0.001)
                {
                    float scanBlock = floor(uv.y * 70.0);
                    float n = hash(float2(scanBlock, floor(_Time.y * 24.0)));
                    if (n > 1.0 - _Glitch * 0.7)
                        uv.x += (hash(float2(scanBlock, 11.0)) - 0.5) * _Glitch * 0.18;
                    uv.y += _Glitch * 0.015 * sin(_Time.y * 60.0);
                    aber += _Glitch * 0.01;
                }

                // 화면 밖(왜곡으로 빠진 영역)은 검정
                if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
                    return fixed4(0,0,0,1);

                // 색수차: RGB를 좌우로 미세 분리 (글리치 시 증폭)
                float2 dir = uv - 0.5;
                float3 col;
                col.r = tex2D(_MainTex, uv + dir*aber).r;
                col.g = tex2D(_MainTex, uv).g;
                col.b = tex2D(_MainTex, uv - dir*aber).b;

                // 스캔라인
                float scan = sin(uv.y * _ScanCount * 3.14159) * 0.5 + 0.5;
                col *= 1.0 - _ScanIntensity * scan;

                // 비네트
                float vig = smoothstep(1.1, _Vignette*0.4, length(dir)*1.4);
                col *= vig;

                // 그레인 + 형광등 플리커
                float g = hash(uv * _ScreenParams.xy + frac(_Time.y)*97.0);
                col += (g - 0.5) * _Grain;
                float flick = 1.0 - _Flicker * (hash(float2(floor(_Time.y*30.0), 3.0)));
                col *= flick;

                // 사망 암전 (풀스크린)
                col *= (1.0 - _Fade);

                return fixed4(saturate(col), 1.0);
            }
            ENDCG
        }
    }
    Fallback Off
}
