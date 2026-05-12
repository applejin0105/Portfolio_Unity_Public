Shader "Custom/UI/GridGlitch01"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _NoiseTex ("Gray Noise", 2D) = "white" {}
        _RainbowTex ("Rainbow Gradient", 2D) = "white" {}

        _GridSize ("Grid Size (X, Y)", Vector) = (3, 3, 0, 0)
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 1
        _Seed ("Random Seed", Float) = 0

        _MaskMode ("Mask Mode", Float) = 0

        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "RenderType"="Transparent"
        }
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            sampler2D _RainbowTex;

            float4 _GridSize;
            float _GlitchIntensity;
            float _Seed;
            float _MaskMode;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color;
                return OUT;
            }

            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 trueUV = IN.texcoord;
                fixed4 cleanColor = tex2D(_MainTex, trueUV);

                float2 cellID = floor(trueUV * _GridSize.xy);
                float2 localUV = frac(trueUV * _GridSize.xy);

                float randVal = random(cellID + _Seed);
                float2 shuffleOffset = float2(randVal, frac(randVal * 2.5)) * _GlitchIntensity;

                float noiseVal = tex2D(_NoiseTex, float2(trueUV.y, _Time.y * 5.0)).r;
                float jitter = (noiseVal - 0.5) * 0.2 * _GlitchIntensity;

                float2 fakeUV = (cellID + shuffleOffset + localUV) / _GridSize.xy;
                fakeUV.x += jitter;
                fakeUV = frac(fakeUV);

                fixed4 glitchColor = tex2D(_MainTex, fakeUV);
                fixed4 rainbowColor = tex2D(_RainbowTex, float2(fakeUV.x + _Time.y, 0.5));

                glitchColor.rgb *= rainbowColor.rgb * 1.5;

                fixed4 finalColor = lerp(cleanColor, glitchColor, _GlitchIntensity);

                if (_MaskMode > 0.5)
                {
                    float originalAlpha = tex2D(_MainTex, IN.texcoord).a;
                    finalColor.a *= originalAlpha;
                }

                return finalColor * IN.color;
            }
            ENDCG
        }
    }
}