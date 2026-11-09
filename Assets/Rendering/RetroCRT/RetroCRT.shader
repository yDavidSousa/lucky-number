Shader "Custom/RetroCRT"
{
    Properties
    {
        _Resolution ("Virtual Resolution", Vector) = (426,240,0,0)
        _Blur ("Blur", Range(0,1)) = 0.35
        _Scanlines ("Scanlines", Range(0,1)) = 0.15
        _Chromatic ("Chromatic Aberration", Range(0,3)) = 0.5
        _Noise ("Noise", Range(0,1)) = 0.04
        _Curvature ("Curvature", Range(0,0.2)) = 0.03
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
        }
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "RetroCRT"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float4 _Resolution;
            float _Blur;
            float _Scanlines;
            float _Chromatic;
            float _Noise;
            float _Curvature;

            float Hash(float2 p)
            {
                p = frac(p * float2(0.1031, 0.1030));
                p += dot(p, p.yx + 33.33);
                return frac((p.x + p.y) * p.x);
            }

            float3 SamplePixel(float2 uv)
            {
                float2 res = max(_Resolution.xy, 1.0);
                float2 pixel = uv * res;
                float2 center = (floor(pixel) + 0.5) / res;

                float2 offset = _BlitTexture_TexelSize.xy
                    * _Blur * 3.0;

                float3 color = SAMPLE_TEXTURE2D_X(
                    _BlitTexture, sampler_LinearClamp, center
                ).rgb * 0.5;

                color += SAMPLE_TEXTURE2D_X(
                    _BlitTexture, sampler_LinearClamp,
                    center + float2(offset.x,0)
                ).rgb * 0.125;

                color += SAMPLE_TEXTURE2D_X(
                    _BlitTexture, sampler_LinearClamp,
                    center - float2(offset.x,0)
                ).rgb * 0.125;

                color += SAMPLE_TEXTURE2D_X(
                    _BlitTexture, sampler_LinearClamp,
                    center + float2(0,offset.y)
                ).rgb * 0.125;

                color += SAMPLE_TEXTURE2D_X(
                    _BlitTexture, sampler_LinearClamp,
                    center - float2(0,offset.y)
                ).rgb * 0.125;

                return color;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                float2 centered = uv * 2.0 - 1.0;

                float2 curved = centered *
                    (1.0 + dot(centered, centered) * _Curvature);

                uv = curved * 0.5 + 0.5;

                if (any(uv < 0.0) || any(uv > 1.0))
                    return half4(0, 0, 0, 1);

                float2 shift = float2(
                    _Chromatic / max(_Resolution.x, 1.0), 0
                );

                float3 color;
                color.r = SamplePixel(saturate(uv + shift)).r;
                color.g = SamplePixel(uv).g;
                color.b = SamplePixel(saturate(uv - shift)).b;

                float scan = sin(uv.y * _Resolution.y * 3.14159265);
                color *= 1.0 - _Scanlines * (0.5 - 0.5 * scan);

                float2 pixel = floor(uv * _ScreenParams.xy);

                float frame = floor(_Time.y * 60.0);

                float noise = Hash(pixel + float2(
                    frame * 17.13,
                    frame * 43.71
                ));

                color += (noise - 0.5) * _Noise;

                float vignette = 1.0 -
                    0.2 * dot(centered, centered);

                color *= saturate(vignette);

                return half4(saturate(color), 1);
            }
            ENDHLSL
        }
    }
}