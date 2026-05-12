Shader "Custom/UIRadialColorChange"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _TargetColor ("Target Color", Color) = (1,0,0,1)

        _EffectCenter ("Effect Center", Vector) = (0,0,0,0)
        _EffectRadius ("Effect Radius", Float) = 0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

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
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TargetColor;
            float4 _EffectCenter;
            float _EffectRadius;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                // 에러가 없는 월드 좌표계 추출
                OUT.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 원본 이미지
                fixed4 originalCol = tex2D(_MainTex, IN.texcoord) * IN.color;

                // 1. 색상 반전: 검은색(0,0,0)은 흰색(1,1,1)이 되며, 기존 무늬는 보색으로 뒤집힘
                fixed3 invertedRGB = 1.0 - originalCol.rgb;

                // 2. 타겟 색상 곱하기: 원본의 검은 배경은 _TargetColor가 되고, 무늬는 타겟 색상에 묻힌 반전 색상이 됨
                fixed4 targetCol = fixed4(invertedRGB * _TargetColor.rgb, originalCol.a);

                // 월드 좌표 기준 거리 계산
                float dist = distance(IN.worldPos.xy, _EffectCenter.xy);

                // 원형 마스킹
                float edge = smoothstep(_EffectRadius - 1.0, _EffectRadius + 1.0, dist);

                return lerp(targetCol, originalCol, edge);
            }
            ENDCG
        }
    }
}