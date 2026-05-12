Shader "Custom/UI/GridGlitch02"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _NoiseTex ("Gray Noise", 2D) = "white" {}
        _RainbowTex ("Rainbow Gradient", 2D) = "white" {}

        _GridSize ("Grid Size (X, Y)", Vector) = (3, 3, 0, 0)
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0
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

            float rand(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            float2 rand2(float2 uv)
            {
                float2 st = float2(dot(uv, float2(127.1, 311.7)), dot(uv, float2(269.5, 183.3)));
                return frac(sin(st) * 43758.5453);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float2 gridSize = max(_GridSize.xy, float2(1, 1));

                float2 cellID = floor(uv * gridSize);
                float2 localUV = frac(uv * gridSize);

                float breakProb = rand(cellID + floor(_Seed));
                float isBroken = step(breakProb, _GlitchIntensity);

                float2 targetCellID = cellID;

                if (isBroken > 0.5)
                {
                    targetCellID = floor(rand2(cellID + _Seed) * gridSize);
                }

                float2 finalUV = (targetCellID + localUV) / gridSize;
                fixed4 color = tex2D(_MainTex, finalUV);

                if (isBroken > 0.5)
                {
                    float noise = tex2D(_NoiseTex, finalUV + _Time.y).r;
                    finalUV.x += (noise - 0.5) * 0.1;

                    color = tex2D(_MainTex, finalUV);
                    fixed4 rainbow = tex2D(_RainbowTex, float2(noise, 0.5));
                    color.rgb = lerp(color.rgb, color.rgb * rainbow.rgb * 2.5, 0.9);
                }

                if (_MaskMode > 0.5)
                {
                    float originalAlpha = tex2D(_MainTex, IN.texcoord).a;
                    color.a *= originalAlpha;
                }

                return color * IN.color;
            }
            ENDCG
        }
    }
}