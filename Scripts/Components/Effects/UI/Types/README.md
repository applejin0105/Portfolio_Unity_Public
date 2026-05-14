[참고용! DOTWeen 함수 시각화 보기](https://easings.net/ko#)

## UIBounceEffect.cs
> 띠요오옹 하는 이펙트입니다.
<img width="800" height="682" alt="UIBounceEffect" src="https://github.com/user-attachments/assets/15505d67-c1c3-4344-9570-7e1e84368d90" />

```csharp
    public struct BounceConfig
    {
        public Vector3 startScale;
        public Vector3 endScale;
        public UIBounceEffectType bounceEffectType;
        [ShowIf("bounceEffectType == Jelly")]
        public float frequency;

        [Header("Effect Sound")]
        public bool useSound;

        [ShowIf("useSound")]
        public SfxSoundType soundType;

        [ShowIf("useSound")]
        public float volume;
    }
```
- `StartScale`: 시작하는 스케일
- `EndScale`: 끝나는 스케일
- Overshoot과 Jelly 두 가지 타입이 존재합니다.
  - Overshoot: 목표치보다 살짝 초과했다가 다시 제자리로 돌아오는 효과
    - Normal: 3차 함수 곡선을 사용, 값이 1.0을 초과하여 최대 약 1.1~1.2배까지 커졌다가 1.0으로 돌아오도록 계산됩니다.
    - DOTWeen: Ease.OutBack 곡선을 사용합니다.
  - Jelly: 용수철처럼 여러 번 출렁이거나 떨리면서 서서히 멈추는 효과
    - Normal: 코사인 함수(Mathf.Cos)를 사용하여 frequency 값에 비례해 물결처럼 오르락내리락하는 진동을 만들어냅니다.
    - DOTWeen: Ease.OutElastic 곡선을 사용합니다.

## UIFadeCanvasGroupEffect.cs
> CanvasGroup의 Alpha값을 조절하는 페이드 이펙트입니다.
<img width="680" height="502" alt="UIFadeEffect" src="https://github.com/user-attachments/assets/9beee320-e123-400c-927d-036cde66f800" />

```csharp
    public struct FadeCanvasGroupConfig
    {
        public float endAlpha;

        [Header("Raycast Settings")]
        public bool blockRaycasts;

        [Tooltip("최종 알파값이 이 수치 미만이면 강제로 blockRaycasts를 해제(false).")]
        public float disableRaycastAlphaThreshold;

        [Header("Effect Sound")]
        public bool useSound;

        [ShowIf("useSound")]
        public SfxSoundType soundType;

        [ShowIf("useSound")]
        public float volume;
    }
```
- `endAlpha`: 최종 알파값
- `disableRaycastAlphaThreshold`: 최종 알파값이 해당 수치 미만이면 자동으로 blockRaycast를 해제(false)하여 접근을 막습니다.

## UIFadeEffect.cs
> 이미지 혹은 텍스트의 개별 Alpha값을 조절하는 페이드 이펙트입니다.
<img width="680" height="502" alt="UIFadeEffect" src="https://github.com/user-attachments/assets/9beee320-e123-400c-927d-036cde66f800" />

```csharp
    public struct FadeConfig
    {
        public float endAlpha;

        [Header("Effect Sound")]
        public bool useSound;

        [ShowIf("useSound")]
        public SfxSoundType soundType;

        [ShowIf("useSound")]
        public float volume;
    }
```
- `endAlpha`: 최종 알파값
- CanvasGroup알파를 만들기 전에, 실험용으로 만든 이펙트입니다. 아무래도, 안정성이나 여러 측면에서 CanvasGroup보다 떨어집니다.

## UIFillEffect.cs
> Unity Image의 Fill을 자연스럽게 진행시켜주는 Fill 이펙트입니다.
<img width="612" height="478" alt="UIFillEffect" src="https://github.com/user-attachments/assets/fff17f80-030f-45df-b704-43b9bde7fef9" />

```csharp
    public struct FillConfig
    {
        public Image.FillMethod fillMethod;
        [DynamicFillOrigin("fillMethod")]
        public int fillOrigin;
        public float startFillAmount;
        public float endFillAmount;

        [Header("Effect Sound")]
        public bool useSound;

        [ShowIf("useSound")]
        public SfxSoundType soundType;

        [ShowIf("useSound")]
        public float volume;
    }
```
- [DynamicFillOrigin](https://github.com/applejin0105/Portfolio_Unity_Public/blob/main/Scripts/Core/Attributes/DynamicFillOriginAttribute.cs) 함수를 여기서 사용합니다.
- 인스펙터에서 Fill Origin 설정 하기도 귀찮고, 매번 할때마다 하나하나 신경써주기 싫어서 귀차니즘 해소용으로 만들었습니다.

- `fillMethod`: 유니티에서 제공하는 Fill Method를 선택합니다.
- `fillOrigin`: 각 Fill Method의 Fill Origin을 선택합니다.
- `startFillAmount`: Fill 시작 값
- `endFillAmount`: Fill 종료 값

## UIFlickerEffect.cs
> 깜빡깜빡 거리는 이펙트입니다. 추가적인 DimColor도 적용 가능합니다. 이 프로젝트에서는 Loading에서 번개가 번쩍번쩍 거리는 효과를 구현했습니다.
<img width="560" height="468" alt="UIFlickerEffect" src="https://github.com/user-attachments/assets/4f8cf84c-2c18-4dd5-94c3-b2f0834d9f9d" />

```csharp
    public struct FlickerConfig
    {
        public bool isLoop;

        public Color baseColor;

        public Color dimColor;

        public float flickerDuration;

        [Header("Effect Sound")]
        public bool useSound;

        [ShowIf("useSound")]
        public SfxSoundType soundType;

        [ShowIf("useSound")]
        public float volume;
    }
```
- 깜빡깜빡 거리는 효과입니다. Base Color와 Dim Color를 사용하여, 각 색상을 교차로 깜빡깜빡하게 만들거나 Dim Color를 알파 0으로 만들면 완전히 껏다 켯다도 가능합니다.

- `baseColor`: 기본 색상
- `dimColor`: 깜빡거릴때 전환될 색상
- `flickerDuration`: 깜빡거리는 간격(시간)

## UIGlitchFadeEffect.cs
>무지개 색으로 지지직 거리는 이펙트입니다. Shader 그리고 Material과 함께 사용해야 합니다.
<img width="574" height="450" alt="UIGlitchFadeEffect" src="https://github.com/user-attachments/assets/b861030c-44cf-4052-a41f-b184bb0489f2" />

```csharp
    public enum GlitchMode
    {
        SingleFadeIn,
        RandomLoop
    }

    [Serializable]
    public struct GlitchFadeConfig
    {
        public GlitchMode mode;
        public GlitchMaskMode maskMode;
        public Vector2 gridSize;
        public Material targetMaterial;

        [Header("Loop Settings (RandomLoop 모드 전용)")]
        public float loopIntervalMin;
        public float loopIntervalMax;
        public float loopGlitchDuration;

        [Header("Effect Sound")]
        public bool useSound;

        [ShowIf("useSound")]
        public SfxSoundType soundType;

        [ShowIf("useSound")]
        public float volume;
    }
```

```shaderlab
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
```
- 화면 전체의 격자들을 난수에 따라 제각각 밀어내고(Offset) 무지개색을 덮어씌워 '완전히 망가진 이미지'를 먼저 생성합니다. 그 후, 원본 이미지와 망가진 이미지를 _GlitchIntensity에 따라 부드럽게 섞는(Lerp) 방식을 사용합니다.
1. 정상 이미지 추출
  - 가장 먼저 원래의 올바른 UV(trueUV)를 이용해 왜곡이 없는 깨끗한 원본 색상(cleanColor)을 샘플링하여 저장해 둡니다.
2. UV 오프셋 및 지터(Jitter) 연산
  - 화면을 _GridSize로 나눈 뒤, 각 격자의 고유 ID(cellID)와 _Seed를 조합해 난수(randVal)를 생성합니다.
  - 이 난수를 바탕으로 shuffleOffset을 계산하여 격자 내부의 이미지가 대각선이나 엉뚱한 방향으로 밀려나게 만듭니다.
  - 노이즈 텍스처(_NoiseTex)와 시간(_Time.y)을 사용해 jitter 값을 구하고, 이를 가로축(X)에만 더해 브라운관 TV가 고장 난 것처럼 화면이 가로로 떨리게 만듭니다.
3. 색상 변조 및 텍스처 래핑
  - 밀려나고 떨리는 최종 임시 UV(fakeUV)를 계산한 뒤, frac() 함수를 씌워 UV 값이 0과 1 사이를 반복하도록(타일링) 만듭니다. 이로 인해 이미지가 밖으로 벗어나지 않고 잘려서 반대쪽으로 나타납니다.
  - 이 왜곡된 UV로 텍스처를 읽어와 글리치된 색상(glitchColor)을 만들고, 무지개 텍스처(_RainbowTex)를 시간에 따라 스크롤하며 곱해 색상을 화려하고 강렬하게(1.5배) 증폭시킵니다.
4. 최종 합성 (Lerp) 및 마스킹
  - lerp(cleanColor, glitchColor, _GlitchIntensity): 강도(Intensity) 값이 0이면 깨끗한 원본만 보이고, 1이면 완전히 왜곡된 이미지만 보이며, 0.5면 두 이미지가 반투명하게 겹쳐 보입니다.
  - 마지막으로 _MaskMode가 켜져 있다면, 원본 텍스처의 알파값을 곱해 UI 밖으로 사각형의 글리치 찌꺼기가 튀어나가지 않게 깔끔하게 잘라냅니다.

```shaderlab
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
```
- 화면을 격자(Grid) 모양으로 나누고, 특정 확률로 격자의 위치를 섞거나 무지개 색상의 노이즈를 덮어씌워 화면이 깨진 듯한 효과를 주는 셰이더입니다.
  - `_GridSize`: 화면을 나눌 X, Y 격자 개수입니다.
  - `_GlitchIntensity`: 격자가 깨질(위치가 섞일) 확률을 결정합니다.
  - `_Seed`: 난수 생성에 사용되는 시드 값으로, 이 값을 변경해 매번 다른 패턴의 글리치를 만듭니다.
  - `_MaskMode`: 활성화(0.5 이상) 시 원본 텍스처의 알파(투명도) 값을 유지하여, UI 영역 밖으로 글리치가 튀어나가지 않게 마스킹합니다.
- 작동 원리
  - 현재 픽셀의 UV 좌표를 _GridSize에 곱하여 자신이 속한 격자의 고유 ID(cellID)와 격자 내부의 로컬 UV(localUV)를 계산합니다.
  - cellID와 _Seed를 조합해 생성한 난수(breakProb)가 _GlitchIntensity보다 작으면 해당 격자를 '망가진(isBroken)' 상태로 판별합니다.
  - 격자가 망가졌다면(isBroken > 0.5), 화면 내의 무작위 다른 격자 좌표(targetCellID)의 픽셀을 가져와 화면을 섞습니다.
  - 추가로 노이즈 텍스처(_NoiseTex)와 시간에 따라 X축 위치를 미세하게 흔들고, 무지개 텍스처(_RainbowTex)를 곱해 색상이 깨지는 연출을 더합니다.

- 01 버전은 강도 조절에 따라 화면 전체가 스르륵 망가지거나 겹쳐 보이는 연출을 만드는 구조입니다.
- 02 버전은 노이즈 텍스처를 덜 샘플링하고 확률적으로만 작동해서 연출의 느낌이 더 날카롭게 끊어지게 만들어줍니다.

## UIGlitchFadePuzzleEffect.cs
> UIGlitchFadeEffect의 확장 버전으로, 사용자가 정한 Grid로 이미지가 잘리고, 뒤섞이며 무지개 색 이펙트와 함께 지지직거리는 이펙트입니다. 마찬가지로, Shader 그리고 Material과 함께 사용해야 합니다.
<img width="578" height="476" alt="UIGlitchFadePuzzleEffect" src="https://github.com/user-attachments/assets/111e5c5b-9fa0-44c3-b491-fbd1aee91fb9" />

```csharp
    public enum PuzzleGlitchMode
    {
        SingleError,
        RandomLoop
    }

    public enum GlitchMaskMode
    {
        RectArea,
        VisibleArea
    }

    [Serializable]
    public struct GlitchPuzzleConfig
    {
        public PuzzleGlitchMode mode;
        public GlitchMaskMode maskMode;
        public Vector2 gridSize;

        [Range(0f, 1f)]
        public float filterChance;
        public Material targetMaterial;

        [Header("Loop Settings")]
        public float brokenTimeMin;
        public float brokenTimeMax;
        public float transitionDuration;
        public float normalTimeMin;
        public float normalTimeMax;

        [Header("Impact Settings")]
        [Tooltip("퍼즐이 변할 때 강하게 파르르 떨리는 시간")]
        public float jitterDuration;

        [Header("Effect Sound")]
        public bool useSound;

        [ShowIf("useSound")]
        public SfxSoundType soundType;

        [ShowIf("useSound")]
        public float volume;
    }
```
- GridGlitch02 셰이더와 유사하게 화면을 격자로 쪼개지만, 슬라이딩 퍼즐처럼 조각이 섞이는 느낌에 더해 강한 가로 떨림(Jitter)과 색상 수차(Chromatic Aberration) 효과를 제공합니다.
  - `_GlitchIntensity`: 퍼즐 조각이 섞일 확률을 결정합니다.
  - `_FilterAmount`: 섞인 조각에 무지개색 필터가 씌워질 확률을 개별적으로 제어합니다.
  - `_JitterIntensity`: 가로로 심하게 흔들리는 떨림 효과의 강도를 조절합니다.
- 작동 원리
  - _GlitchIntensity에 따라 특정 격자의 위치를 무작위로 섞습니다.
  - 지터(Jitter) 효과: `_JitterIntensity`가 0보다 클 경우, Y축 좌표와 시간(_Time.y)을 기반으로 난수를 생성하여 가로(X축) UV 값을 빠르게 떨리게 만듭니다.
  - RGB 채널 분리: 지터가 발생할 때, Red 채널은 양수 방향으로, Blue 채널은 음수 방향으로 UV를 엇갈리게 샘플링하여 레트로 브라운관 TV가 고장 난 것 같은 색상 분리 효과(RGB Split)를 구현합니다.
  - 이후 특정 난수 조건`(filterChance < _FilterAmount)`을 만족하는 퍼즐 조각에만 노이즈와 무지개색을 덮어씌웁니다.

## UIHoverSwingEffect.cs
> 타겟을 살짝 띄우고, 원하는 방향과 각도로 그네처럼 흔들흔들 흔들리는 이펙트입니다. 로보토미 코퍼레이션의 환상체 선택 연출을 참고하여 제작하였습니다.
<img width="532" height="462" alt="UIHoverSwingEffect" src="https://github.com/user-attachments/assets/fa0b8954-8318-4f90-b9b2-daafd096643f" />

```csharp
    public struct HoverSwingConfig
    {
        public Vector3 swingRotInit;

        public Vector3 swingRotMin;
        public Vector3 swingRotMax;

        public float returnDuration;

        public bool isLoop;

        [Header("Effect Sound")]
        public bool useSound;

        [ShowIf("useSound")]
        public SfxSoundType soundType;

        [ShowIf("useSound")]
        public float volume;
    }
```
- `swingRotInit`: 최종적으로 돌아올 Rotation
- `swingRotMin`: 이동할 Rotation1
- `swingRotMax`: 이동할 Rotation2
- `returnDuration`: 돌아올 시간

## UIHoverTiltEffect.cs
> 마우스를 올리면 해당 방향으로 대상이 들어가는, 마치 3D 공간에 카드를 놓은 것을 보여주는 이펙트입니다.
<img width="800" height="374" alt="Hover" src="https://github.com/user-attachments/assets/8894a0fe-399f-4917-a291-7ed0f0985b5b" />

```csharp
    public struct HoverTiltConfig
    {
        [Header("X Axis Rotation")]
        public float maxAngleX;
        public float minAngleX;

        [Header("Y Axis Rotation")]
        public float maxAngleY;
        public float minAngleY;

        [Header("Settings")]
        public float dampingSpeed;
    }
```
- `maxAngleX`: X축 최대 각도
- `minAngleX`: X축 최소 각도
- `maxAngleY`: Y축 최대 각도
- `minAngleY`: Y축 최소 각도
- `dampingSpeed`: 꺾일 때 얼마나 '빠르게' 꺾이는지 조절하는 값

## UIMoveEffect.cs
> 타겟의 Position, Scale, Rotation을 자유롭게 조절하는 이펙트입니다.
<img width="438" height="366" alt="UIMoveEffect" src="https://github.com/user-attachments/assets/d8251f17-2973-4e73-b9d6-dd3dc462e569" />

```csharp
    [Serializable]
    public struct Range
    {
        public Vector3 start;
        public Vector3 end;
    }

    [Serializable]
    public struct MoveConfig
    {
        [Header("Position")]
        public Range position;
        [Header("Rotation")]
        public Range rotation;
        [Header("Scale")]
        public Range scale;

        [Header("Effect Sound")]
        public bool useSound;

        [ShowIf("useSound")]
        public SfxSoundType soundType;

        [ShowIf("useSound")]
        public float volume;
    }
```
- `Range`: 각 position, rotation, scale의 시작값과 끝값을 결정.

## UIPulseEffect.cs
> 심장이 두근두근 거리는 것과 같은 연출을 하는 이펙트입니다.
<img width="318" height="262" alt="UIPulseEffect" src="https://github.com/user-attachments/assets/68f035de-4a15-4f77-9a43-f326e40103a3" />

```csharp
    public struct PulseConfig
    {
        public bool isLoop;

        public float scaleMin;
        public float scaleMax;

        public float pulseDuration;

        [Header("First Effect Sound (Scale Up)")]
        public bool useFirstSound;

        [ShowIf("useFirstSound")]
        public SfxSoundType firstSoundType;

        [ShowIf("useFirstSound")]
        public float firstVolume;

        [Header("Second Effect Sound (Scale Down)")]
        public bool useSecondSound;

        [ShowIf("useSecondSound")]
        public SfxSoundType secondSoundType;

        [ShowIf("useSecondSound")]
        public float secondVolume;
    }
```
- 여긴 좀 더 공들여서 구현했습니다. 심장이 두근두근 거리는 것 처럼 진행하고 싶었는데, 제가 원했던 디테일은 두/근 하는 소리가 각기 다르길 원했습니다.
- 그래서, 기존 Sound도 사용이 가능은 하지만, 별도의 firstSoundType과 secondSoundType을 두고, 각각 사용 여부를 결정하여 소리를 출력하게 만들었습니다.
- `scaleMin`: 스케일 최소값
- `scaleMax`: 스케일 최대값
- `pulseDuration`: 두근 거리는 속도

```csharp
            while (configToUse.isLoop || elapsed < exactTotalTime)
            {
                // halfCycleTime(pulseDuration) 기준으로 현재 주기를 계산
                var currentHalfCycle = Mathf.FloorToInt(elapsed / configToUse.pulseDuration);

                if (currentHalfCycle > lastPlayedHalfCycle)
                {
                    var isScalingUp = currentHalfCycle % 2 == 0;

                    if (isScalingUp && configToUse.useFirstSound && SoundManager.Instance != null)
                    {
                        var finalVolume = configToUse.firstVolume <= 0f ? 1.0f : configToUse.firstVolume;
                        PlaySound(configToUse.firstSoundType, finalVolume);
                    }
                    else if (!isScalingUp && configToUse.useSecondSound && SoundManager.Instance != null)
                    {
                        var finalVolume = configToUse.secondVolume <= 0f ? 1.0f : configToUse.secondVolume;
                        PlaySound(configToUse.secondSoundType, finalVolume);
                    }

                    lastPlayedHalfCycle = currentHalfCycle;
                }

                var t = Mathf.PingPong(elapsed, configToUse.pulseDuration) / configToUse.pulseDuration;
                targetRectTransform.localScale = _originalScale * Mathf.Lerp(1f, targetScaleMultiplier, t);

                elapsed += Time.deltaTime;
                yield return null;
            }
```
- 내부를 보면, `halfCycleTime(pulseDuration)` 기준으로 현재 재생되는 주기를 계산합니다. 누적된 시간 `elapsed`을 반주기 시간 `pulseDuration`으로 나눈 뒤, 내림 처리를 합니다.
- 이를 거치면, `currentHalfCycle` 값은 `0 -> 1 -> 2 -> 3 ...` 형태로 정수로 딱딱 끊어져 증가합니다.
  - 0: 첫 번째 스케일 업 구간
  - 1: 첫 번째 스케일 다운 구간
  - 2: 두 번째 스케일 업 구간
- 그러면 `currentHalfCycle`의 숫자가 올라갈 때 마다 `if (currentHalfCycle > lastPlayedHalfCycle)`가 딱 한번씩 실행됩니다.
- 그러면 현재 주기 정수값을 2로 나눈 나머지 (`%2`)가 0인지 확인해서 짝수면 스케일 업, 홀수면 스케일 다운이므로 각 사운드를 알맞게 출력합니다.

## UIRadialEffect.cs
> 원하는 인터렉션에 할당한 뒤, 누르면 해당 인터렉션으로 원이 좁혀지고, 다시 다른 색으로 밝아지는 연출을 위한 이펙트입니다. Shader 적용이 필요합니다.
<img width="800" height="443" alt="Radial" src="https://github.com/user-attachments/assets/1dea4f0a-4b37-4a4d-8ba7-9c0d43ae9944" />

```csharp
    public struct RadialConfig
    {
        public RectTransform centerRect;
        public bool isExpand;
        public bool useSound;

        [ShowIf("useSound")]
        public SfxSoundType soundType;

        [ShowIf("useSound")]
        public float volume;
    }
```

- 솔직히 완성하고 가장 뿌듯한 코드입니다. 페르소나 느낌으로 제작하고 싶었습니다. 특히 셰이더는 AI의 도움을 많이 받았습니다.
- 처음 기획은 색 반전을 통해 숨겨진 요소를 깔끔하게 드러내고 싶었습니다. 그런데 자꾸만 원하는 위치에 생기지도 않고, 화면을 넘어서까지 과도하게 계산을 하고 작동하는 바람에 화면에 위치에 따라 줄어들고 생기는 속도가 너무나 달랐습니다.

- 기존에는 셰이더에서 UV 좌표를 사용하고, C#에서는 마우스 클릭 위치나 UI의 `anchoredPosition`을 그대로 전달해서 사용했습니다. 그러다보니 셰이더는 가령 위치를 0.5로 보내면, C#에서는 중심점(픽셀, 가령 500)라고 전달하면 셰이더 입장에서는 중심점이 자신의 범위를 한참 벗어난 곳에 있다고 판단해서 이게 원이 화면 밖 어딘가에 생성되어 보이지 않거나 엉뚱한 곳에서 나타나버렸습니다.
- 그래서, 셰이더에서 `unity_ObjectToWorld`를 통해 월드 좌표를 추출하고, C#에서도 `targetRect.position`이라는 월드 좌표를 넘겨주고 양쪽의 단위와 기준점을 월드 공간으로 통일해서 해결했습니다.
- 이러다보니, 효과가 화면 전부 덮으려면, 원이 계속계속 커져야해서 시간이 오래걸려버렸습니다. 그래서 `CalculateExactWorldMaxRadius` 메서드를 통해, 화면의 네 귀퉁이 `GetWorldCorners`까지의 거리 중 가장 먼 거리를 계산하여 딱 필요한 만큼만 반지름을 키웠습니다. 낭비되는 반경이 이러면 사라지므로, duration 동안 꽉차는 연출이 균일하게 일어납니다.

- 문제는, 제가 셰이더에 관한 지식이 거의 없다는 것입니다. 이걸 지금 배우고 적용하기에는 한계가 존재하고, 그래서 AI의 도움을 받아 제작했습니다. 단, 셰이더에 모든 연산을 맡기지는 않고 원이 화면을 덮기 위한 최대 반경 계산과 애니메이션 타이밍은 C#의 컨트롤러가 담당하게 하고, 셰이더는 단순히, 최대한 단순히 넘겨받은 반경 값을 기반으로 픽셀 단위의 색상 반전과 마스킹만 처리하도록 역할을 분리했습니다. 다 만들고 나서 결과적으로, 렌더링 부하도 의도치않게 줄일 수 있었습니다.
- 만일 화면 비율이 변경되는 경우도 고민해보았습니다. 원이 타원으로 찌그러지는 문제를 방지하기 위해, `unity_ObjectToWorld` 매트릭스를 사용하여 절대적인 월드 좌표를 추출하고, 어떤 해상도나 UI 크기에서도 완벽하게 원을 그리도록 구현했습니다.

## `unity_ObjectToWorld`와 Shader
- `unity_ObjectToWorld`는 로컬 좌표를 월드 좌표로 변환해 주는 4x4 변환 행렬입니다. 셰이더에서 mul(unity_ObjectToWorld, v.vertex)를 실행하면, UI 컴포넌트의 position, rotation, scale 정보가 곱해져 절대적인 월드 좌표를 계산할 수 있습니다.
- 이게 몇몇 셰이더에서 핵심인 이유는, 일반적으로 2D UI 셰이더는 UV 좌표계(0.0 ~ 1.0)를 사용합니다. 하지만 UV는 이미지 비율(Aspect Ratio)에 종속됩니다.
  - 가령, 1000px * 200px짜리 길쭉한 버튼이 있다고 하면, UV 좌표에서 가로로 0.1 이동하는 것과 세로로 0.1 이동하는건 수학적으로는 값이 같지만 실제 화면 상의 픽셀 거리로는 5배 차이가 나게 됩니다. 이러다보니 UV 좌표계에서 `distance()` 함수로 원을 그리면, 원이 아니라 버튼의 비율을 따라 쭉 늘어난 타원이 그려집니다.
- 그래서 월드 좌표는 해상도나 UI의 찌그러짐과 무관한 절대적인 거리 단위를 가집니다. 따라서 중심점과 현재 픽셀 사이를 계산하려면 반드시 `unity_ObjectToWorld`로 구한 월드 좌표를 사용해서 이미지가 아무리 길쭉하고 찌그러져 있어도 항상 화면상에서 제대로된 원을 유지하며 퍼져나갈 수 있게 해야합니다.

## 이미지 사용 없이 수학적으로 시각 효과 계산하기
- 제가 하고 싶던건 이미지로 덮어씌우는 방식이 아니었습니다. **최적화**에 집중하고 싶었고, 수학적인 계산을 해보고 싶었습니다.
- 원했던 효과는 색을 특정 색으로 반전 -> 그러면 배경 색과 동일하게 존재하던 요소들이 마치 마법처럼 뾰로로로롱 나타나는 효과가 필요했습니다.
- 셰이더에서는 기본적으로 색상이 0~255가 아니라, 0.0~1.0 사이의 실수로 정규화되어 처리됩니다.
  - 가령 원본 픽셀이 `(1, 1, 1,)`로 흰색이라면 색상 반전 시, `1.0 - 1.0 = 0`로 완전히 검정색이 될겁니다.
  - 단순한 뺄셈 연산으로 이미지의 보색을 실시간으로 만들어내는 행위인데, 저는 이것도 좋지만 '특정 색'으로 반전시키고 싶었습니다.
- 그러니 여기에 목표 색상 `_TargetColor`을 곱해주면, 기존의 무늬와 명암은 그대로 유지하면서 색상 톤만 덮어 씌워주는, 제가 딱 원한 기능을 구현하게 되었습니다.
- 여기에서 처음에는 원 테두리가 픽셀 단위로 우글우글 깨져서 징그럽기도하고 못생겨서 좀 기분이 나빳습니다.
- 이를 해결하고자 `smoothstep(min, max, value)` 함수를 사용했습니다. 이 함수는 lerp와 같이 min과 max 사이일 때 0.0에서 1.0 사이의 값을 부드러운 곡선 형태로 보간해줍니다.
- 특히 코드에서는 그냥 적용한게 아니라 `smoothstep(_EffectRadius - 1.0, _EffectRadius + 1.0, dist)`를 해서 반지름 기준 앞뒤로 1픽셀씩, 총 2픽셀 영역으로 테두리 영역을 잡아 부드러운 그라데이션을 주었습니다.
- 결과적으로 테두리 픽셀이 100% 원본 혹은 100% 타겟 색상으로 딱 떨어지는게 아니라, 50% 30% 식으로 반투명하게 lerp되어 경계선을 아주 매끄럽게 전문 용어(와!)로 Anti-aliasing이 됩니다.

## Stencil 마스크
- 이제 이렇게 만들었는데 문제가, 스크롤뷰 내부 요소들을 추가했더니, 이놈들이 표시가 되었습니다. 꼴보기 싫게. 이게 내부 들어가는 글들은 양이 많아서 무조건 스크롤뷰로 넣어야했는데 그렇다고 도중에 생성 시킬수도 없었습니다. 글씨는 뭐 배경색에서 바뀌는거라 문제는 없었는데, 이미지들이 문제였습니다.
- 레디얼 효과 발동 -> 안보이는 영역에서 프리팹으로 각 요소에 맞게 요소를 교체 -> 스크롤 바 내용들이 교체되는 형식인데, 이미지들이 지-랄 맞게 자기주장을 해서 이걸 어쩌지 고민했습니다. 그렇다고 이미지도 전부 색을 까맣게 만들수도 없는 노릇인데. 점묘화를 찍을것도 아니고.
- 자료를 찾아보니, 유니티 UI는 스텐실 버퍼(Stencil Buffer)라는 기술로 마스킹을 처리한다고 합니다.
  - 컴퓨터 그래픽스에서 스텐실 버퍼(Stencil Buffer)는 화면의 각 픽셀마다 특정 값을 저장할 수 있는 메모리 공간입니다. 유니티 UI는 이 공간을 활용해 어떤 픽셀을 그리고 버릴지 결정합니다.
  - 그럼 이제 스크롤뷰의 기본적으로는 Viewport에 Mask가 있으면 유니티는 해당 영역만큼 화면에 그림을 그리는 대신, 스텐실 버퍼에 특정 숫자(1이나 뭐 그런거)를 기록합니다. 보이는게 아닌 데이터로만 기록합니다.
  - 스크롤 뷰 안에 이미지나 텍스트를 그릴 차례가 되면, 자식 요소들은 그리기를 시작하기 전에 GPU에게 스텐실 번호를 확인합니다.
  - 번호가 맞는 경우, 마스크 영역 안인것이니깐 그리고, 안맞으면 출력하지 않고 버립니다.
- 그런데 문제가, 커스텀 셰이더는 기본적으로 이런 명령은 무시합니다. 생각해보면 당연한게 뭐, 시키지도 않은 일을 하면 그게 사람이지 컴퓨터는 아니지 않겠습니까...
- 그래서 스텐실 처리를 안 했더니, 스크롤 뷰 UI 밖으로 튀어 나와가지고 보이면 안되는 이미지들이 튀어나와 보이는겁니다. 그렇다고 이미지가 없는 부분을 하나씩 계산해서 처음에 스크롤바에서 그 부분을 표시하고, 로딩 끝나면 가장 위로 쑥 이동하는 걸 넣자니 이건 노가다도 이런 개노가다가 없어서 이 스텐실 마스크를 사용했습니다.
- 사실, 따지고보면 눈속임입니다. 제가 원한건 **배경 자체의 색상이 퍼져나가면서 바뀌는 연출**을 원했기 때문에, 이 방식을 고수하면서, 그냥 Radial 효과 적용 중에, 그 뒷부분은 완전히 보이지 않게 해버리자 가 되어버렸습니다.
- 그래서 스텐실을 적용해서, 뒤에는 완전히 안보이게 하면서 그림도 가리고, 시각적인건 프론트엔드에서 마치 가려져있는데 짜잔 하고 나타났네? 로 타협봤습니다. 물론 조금 디테일이 사라진건 아쉽지만, 솔직히 누가 그런걸 신경쓰겠습니까. 그냥 와! 가려져 있던게 생겼구나! 로 먼저 인식하지 않겠습니까????

- 지금 당장에 셰이더를 구현하는 것은 불가능했습니다. AI에게 도움을 받아 셰이더를 구성하고, 고치고를 반복했습니다. 그래서 `테크니컬 아티스트를 위한 유니티 쉐이더 스타트업` 이라는 책을 구입하여, 공부중에 있습니다. 언젠가 이런 셰이더들도 모두 직접 구현해보고 싶습니다!

```shaderlab
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
```

- 특정 중심점을 기준으로 원형(Radial)으로 퍼져나가며 UI의 색상을 반전시키고 지정된 타겟 색상으로 물들이는 트랜지션 셰이더입니다. UI 스텐실 마스킹을 완벽히 지원합니다.
  - `_TargetColor`: 전환될 목표 색상입니다.
  - `_EffectCenter`: 색상 전환 효과가 시작되는 중심점의 위치(월드 좌표계)입니다.
  - `_EffectRadius`: 중심점으로부터 색상이 전환된 영역의 반지름입니다.
- 작동 원리
  - Vertex Shader: UI 요소가 회전하거나 크기가 변하더라도 효과가 일정하게 유지되도록 unity_ObjectToWorld 행렬을 사용해 에러 없는 절대적인 월드 좌표(worldPos)를 추출하여 프래그먼트 셰이더로 넘깁니다.
  - Fragment Shader (색상 연산): 원본 픽셀의 RGB 값을 1에서 빼서 색상을 완전히 반전시킵니다(1.0 - originalCol.rgb). 그 후 반전된 색상에 _TargetColor를 곱해 새로운 질감을 만듭니다.
  - Fragment Shader (원형 마스킹): 현재 픽셀의 월드 좌표와 _EffectCenter 사이의 거리를 계산합니다. 이 거리가 _EffectRadius보다 작으면 타겟 색상을, 크면 원본 색상을 출력하도록 smoothstep과 lerp를 사용해 부드러운 원형 경계선을 그립니다.

## UIShakeEffect.cs
> 타겟이 떨리는 이펙트입니다.
<img width="318" height="240" alt="UIShakeEffect" src="https://github.com/user-attachments/assets/1284a6a9-b82f-44a9-b59f-fd8f4badfa37" />

```csharp
    public struct ShakeConfig
    {
        public float power;
        public int frequency;

        [Header("Effect Sound")]
        public bool useSound;

        [ShowIf("useSound")]
        public SfxSoundType soundType;

        [ShowIf("useSound")]
        public float volume;
    }
```
- `power`: 얼마나 세게 흔들릴지 결정합니다.
- `frequency`: 얼마나 자주 흔들릴지 결정합니다.

## UISweepEffect.cs
> 타겟에 일정 속도와 간격으로 이동하여 마치 거울이 닦이는 연출을 하는 이펙트입니다.
<img width="800" height="466" alt="Sweaping" src="https://github.com/user-attachments/assets/fed9ffe6-3b79-4aec-9210-2b0984b250d0" />

```csharp
    public struct SweepConfig
    {
        public bool isLoop;

        [Range(0, 360)]
        public float angle; // 닦이는 진행 방향 (각도)
        public float sweepDuration; // 지나가는 속도 (시간)
        public float loopInterval; // 루프 모드일 경우 대기 시간

        public float lineWidth; // 선의 두께
        public Color lineColor; // 선의 색상 및 투명도

        public Ease easeType;
    }
```
- `angle`: 닦이는 진행 방향 (각도)
- `sweepDuration`: 지나가는 속도 (시간)
- `loopInterval`: 루프 모드일 경우 대기 시간
- `lineWidth`: 선의 두께
- `lineColor`: 선의 색상 및 투명도

## UITypewriterEffect.cs
> 타자기를 치는 효과 이펙트입니다.
<img width="800" height="84" alt="Typing" src="https://github.com/user-attachments/assets/849dc9de-c582-4ab2-b360-bc1d207de602" />

```csharp
    [Serializable]
    public struct TypewriterConfig
    {
        public string customText;
    }

    public enum TypewriterMode
    {
        Normal,
        KoreanAssemble
    }

    public enum DurationMode
    {
        TotalDuration,
        PerCharacter
    }

    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class UITypewriterEffect : UIConfigurableEffect<TypewriterConfig>
    {
        private const int AudioPoolSize = 5;

        private static readonly Regex RichTextRegex = new(@"<.*?>");
        [Header("Target Components")]
        [SerializeField] private TextMeshProUGUI targetText;

        [Header("Config")]
        [SerializeField] private TypewriterConfig defaultConfig;

        [Header("Init Settings")]
        [Tooltip("체크하면 시작 시(Awake) 텍스트를 미리 숨겨둡니다. 순차 타이핑 연출에 필수.")]
        [SerializeField] private bool hideOnAwake = true;

        [Header("Mode & Timing")]
        [SerializeField] private TypewriterMode typewriterMode;
        [SerializeField] private DurationMode durationMode = DurationMode.TotalDuration;

        [Tooltip("줄바꿈 시 잠시 대기할 시간 (초)")]
        [SerializeField] private float newlineDelay = 0.3f;

        [Header("Audio Settings")]
        [SerializeField] private SfxSoundType[] typingSounds;
        [SerializeField] private SfxSoundType returnSound;

        [Tooltip("줄바꿈 사운드가 출력될 때 볼륨을 몇 배로 키울지 설정합니다.")]
        [SerializeField] [Range(1f, 3f)] private float returnVolumeMultiplier = 1.5f;

        [SerializeField] [Range(0f, 2f)] private float minPitch = 0.9f;
        [SerializeField] [Range(0f, 2f)] private float maxPitch = 1.1f;
        [SerializeField] [Range(0f, 1f)] private float audioVolume = 1.0f;
        private readonly List<string> _koreanFrames = new();
        private AudioSource[] _audioPool;
        private int _audioPoolIndex;

        private string _cachedEndText;

        private bool _isPaused; // 코루틴 딜레이용 플래그

        private int _lastTypedIndex = -1;
        private float? _originalBaseDuration;
        private TypewriterConfig? _overrideConfig;
        private List<string> _pureFrames = new();
        private int _totalCharacterCount;
        private Tween _typewriterTween;
        public TypewriterConfig DefaultConfig => defaultConfig;

        protected override void Awake()
        {
            base.Awake();
            if (targetText == null) targetText = GetComponent<TextMeshProUGUI>();
            InitializeAudioPool();

            if (hideOnAwake && targetText != null) targetText.maxVisibleCharacters = 0;
        }

        private void OnDisable()
        {
            _typewriterTween?.Kill();
            StopAllCoroutines();
            _isPaused = false;
        }

        private void InitializeAudioPool()
        {
            _audioPool = new AudioSource[AudioPoolSize];
            for (var i = 0; i < AudioPoolSize; i++)
            {
                var audioObj = new GameObject($"TypewriterAudio_{i}");
                audioObj.transform.SetParent(transform);

                var source = audioObj.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.spatialBlend = 0f;

                _audioPool[i] = source;
            }
        }

        public override void ClearProperty()
        {
            _overrideConfig = null;
        }

        public override void SetProperty(TypewriterConfig configData, float? customDuration = null)
        {
            _overrideConfig = configData;

            if (customDuration.HasValue)
            {
                OverrideDuration = customDuration.Value;
                _originalBaseDuration = customDuration.Value;
            }
        }

        public override void PlayOverrideEffect()
        {
            StopCoroutine(nameof(ExecuteEffect));
            StartCoroutine(ExecuteEffect());
        }

        public override IEnumerator PlayWaitableEffect()
        {
            StopCoroutine(nameof(ExecuteEffect));
            yield return StartCoroutine(ExecuteEffect());
        }

        protected override IEnumerator ExecuteEffect()
        {
            _originalBaseDuration ??= ActualDuration;

            var baseDuration = _originalBaseDuration.Value;

            var configToUse = _overrideConfig ?? defaultConfig;
            var rawText = string.IsNullOrEmpty(configToUse.customText) ? targetText.text : configToUse.customText;

            _cachedEndText = rawText;
            _lastTypedIndex = -1;
            _pureFrames.Clear();
            _isPaused = false;

            var newlineCount = 0;
            foreach (var c in rawText)
                if (c == '\n')
                    newlineCount++;

            var totalAddedDelay = newlineCount * newlineDelay;

            if (typewriterMode == TypewriterMode.KoreanAssemble)
            {
                var pureText = RichTextRegex.Replace(rawText, "");
                _pureFrames = KoreanTypingHelper.GetTypingFrames(pureText);

                _koreanFrames.Clear();
                foreach (var frame in _pureFrames) _koreanFrames.Add(ApplyRichTextAndAlpha(rawText, frame));

                if (_koreanFrames.Count == 0) yield break;

                targetText.text = _koreanFrames[0];
                targetText.maxVisibleCharacters = 99999;
                targetText.ForceMeshUpdate();
            }
            else
            {
                targetText.text = rawText;
                targetText.maxVisibleCharacters = 0;
                targetText.ForceMeshUpdate();
                _totalCharacterCount = targetText.textInfo.characterCount;
            }

            var typingDuration = baseDuration;
            if (durationMode == DurationMode.PerCharacter)
            {
                var stepCount = typewriterMode == TypewriterMode.KoreanAssemble
                    ? _koreanFrames.Count
                    : _totalCharacterCount;
                typingDuration = baseDuration * stepCount;
            }

            OverrideDuration = typingDuration + totalAddedDelay;

            yield return typewriterMode switch
            {
                TypewriterMode.Normal => animType switch
                {
                    AnimType.Normal => TypewriterNormal(typingDuration),
                    AnimType.DoTween => TypewriterNormalDoTween(typingDuration),
                    _ => throw new ArgumentOutOfRangeException()
                },
                TypewriterMode.KoreanAssemble => animType switch
                {
                    AnimType.Normal => TypewriterKoreanAssemble(typingDuration),
                    AnimType.DoTween => TypewriterKoreanAssembleDoTween(typingDuration),
                    _ => throw new ArgumentOutOfRangeException()
                },
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private string ApplyRichTextAndAlpha(string originalText, string frame)
        {
            var sb = new StringBuilder(originalText.Length + 50);
            var pureCharIndex = 0;
            var i = 0;
            var isHidden = false;

            while (i < originalText.Length)
            {
                if (originalText[i] == '<')
                {
                    var endIndex = originalText.IndexOf('>', i);
                    if (endIndex != -1)
                    {
                        sb.Append(originalText.Substring(i, endIndex - i + 1));
                        i = endIndex + 1;

                        if (isHidden) sb.Append("<alpha=#00>");

                        continue;
                    }
                }

                if (pureCharIndex < frame.Length)
                {
                    sb.Append(frame[pureCharIndex]);
                    pureCharIndex++;
                }
                else
                {
                    if (!isHidden)
                    {
                        sb.Append("<alpha=#00>");
                        isHidden = true;
                    }

                    sb.Append(originalText[i]);
                }

                i++;
            }

            return sb.ToString();
        }

        private void CheckAndPlayTypingSound(int currentIndex, bool isNewline = false)
        {
            if (currentIndex > _lastTypedIndex)
            {
                PlayRandomTypingSound(isNewline);
                _lastTypedIndex = currentIndex;
            }
        }

        private void PlayRandomTypingSound(bool isNewline)
        {
            AudioClip clipToPlay = null;

            if (isNewline && returnSound != SfxSoundType.None)
            {
                SoundManager.Instance.GetSfx(returnSound, out clipToPlay);
            }
            else if (typingSounds is { Length: > 0 })
            {
                var randomTypeSound = typingSounds[Random.Range(0, typingSounds.Length)];
                SoundManager.Instance.GetSfx(randomTypeSound, out clipToPlay);
            }

            if (clipToPlay == null) return;

            var source = _audioPool[_audioPoolIndex];
            source.clip = clipToPlay;

            if (isNewline)
            {
                source.pitch = 1.0f;
                source.volume = Mathf.Clamp01(audioVolume * returnVolumeMultiplier);
            }
            else
            {
                source.pitch = Random.Range(minPitch, maxPitch);
                source.volume = audioVolume;
            }

            source.Play();

            _audioPoolIndex = (_audioPoolIndex + 1) % AudioPoolSize;
        }

        private IEnumerator HandlePauseRoutine()
        {
            _isPaused = true;
            yield return new WaitForSeconds(newlineDelay);
            _isPaused = false;
        }

        private IEnumerator TypewriterNormal(float typingDuration)
        {
            var elapsed = 0f;
            while (elapsed < typingDuration)
            {
                if (_isPaused)
                {
                    yield return null;
                    continue;
                }

                elapsed += Time.deltaTime;
                var currentVisible = Mathf.FloorToInt(Mathf.Clamp01(elapsed / typingDuration) * _totalCharacterCount);

                if (currentVisible > _lastTypedIndex)
                {
                    var isNewline = false;
                    for (var i = _lastTypedIndex + 1; i <= currentVisible; i++)
                        if (i > 0 && i <= _totalCharacterCount)
                        {
                            var c = targetText.textInfo.characterInfo[i - 1].character;
                            if (c == '\n' || c == '\r') isNewline = true;
                        }

                    CheckAndPlayTypingSound(currentVisible, isNewline);

                    if (isNewline && newlineDelay > 0f) StartCoroutine(HandlePauseRoutine());
                }

                targetText.maxVisibleCharacters = currentVisible;
                yield return null;
            }

            targetText.maxVisibleCharacters = _totalCharacterCount;
        }

        private IEnumerator TypewriterNormalDoTween(float typingDuration)
        {
            _typewriterTween?.Kill();
            _typewriterTween = DOTween.To(
                () => targetText.maxVisibleCharacters,
                x =>
                {
                    if (x > _lastTypedIndex)
                    {
                        var isNewline = false;
                        for (var i = _lastTypedIndex + 1; i <= x; i++)
                            if (i > 0 && i <= _totalCharacterCount)
                            {
                                var c = targetText.textInfo.characterInfo[i - 1].character;
                                if (c == '\n' || c == '\r') isNewline = true;
                            }

                        targetText.maxVisibleCharacters = x;
                        CheckAndPlayTypingSound(x, isNewline);

                        if (isNewline && newlineDelay > 0f)
                        {
                            _typewriterTween.Pause();
                            DOVirtual.DelayedCall(newlineDelay, () =>
                            {
                                if (_typewriterTween != null && _typewriterTween.IsActive())
                                    _typewriterTween.Play();
                            });
                        }
                    }
                },
                _totalCharacterCount, typingDuration
            ).SetEase(Ease.Linear);

            yield return _typewriterTween.WaitForCompletion();
        }

        private IEnumerator TypewriterKoreanAssemble(float typingDuration)
        {
            var elapsed = 0f;
            var totalFrames = _koreanFrames.Count;

            while (elapsed < typingDuration)
            {
                if (_isPaused)
                {
                    yield return null;
                    continue;
                }

                elapsed += Time.deltaTime;
                var currentFrameIndex =
                    Mathf.Clamp(Mathf.FloorToInt(Mathf.Clamp01(elapsed / typingDuration) * totalFrames), 0,
                        totalFrames - 1);

                if (currentFrameIndex > _lastTypedIndex)
                {
                    var isNewline = false;
                    for (var i = _lastTypedIndex + 1; i <= currentFrameIndex; i++)
                        if (i > 0 && i < _pureFrames.Count)
                            if (_pureFrames[i].EndsWith("\n") || _pureFrames[i].EndsWith("\r"))
                                isNewline = true;

                    CheckAndPlayTypingSound(currentFrameIndex, isNewline);

                    if (isNewline && newlineDelay > 0f) StartCoroutine(HandlePauseRoutine());
                }

                targetText.text = _koreanFrames[currentFrameIndex];
                yield return null;
            }

            targetText.text = _cachedEndText;
        }

        private IEnumerator TypewriterKoreanAssembleDoTween(float typingDuration)
        {
            _typewriterTween?.Kill();
            var totalFrames = _koreanFrames.Count;

            _typewriterTween = DOTween.To(
                () => 0,
                x =>
                {
                    if (x > _lastTypedIndex)
                    {
                        var isNewline = false;
                        for (var i = _lastTypedIndex + 1; i <= x; i++)
                            if (i > 0 && i < _pureFrames.Count)
                                if (_pureFrames[i].EndsWith("\n") || _pureFrames[i].EndsWith("\r"))
                                    isNewline = true;

                        targetText.text = _koreanFrames[x];
                        CheckAndPlayTypingSound(x, isNewline);

                        if (isNewline && newlineDelay > 0f)
                        {
                            _typewriterTween.Pause();
                            DOVirtual.DelayedCall(newlineDelay, () =>
                            {
                                if (_typewriterTween != null && _typewriterTween.IsActive())
                                    _typewriterTween.Play();
                            });
                        }
                    }
                },
                totalFrames - 1, typingDuration
            ).SetEase(Ease.Linear);

            yield return _typewriterTween.WaitForCompletion();
        }

        /// <summary>
        ///     타이핑 효과를 강제 중단하고 텍스트를 완전히 가려진 초기 상태로 리셋
        /// </summary>
        public void ResetToHiddenState()
        {
            // 실행 중인 애니메이션 및 딜레이 정지
            _typewriterTween?.Kill();
            StopAllCoroutines();
            _isPaused = false;
            _lastTypedIndex = -1;

            if (targetText == null) return;

            // 모드에 따른 초기 가림 처리
            if (typewriterMode == TypewriterMode.KoreanAssemble)
            {
                // 한글 조합 모드는 첫 프레임(보통 빈 문자열 또는 첫 초성)으로 텍스트 교체
                if (_koreanFrames.Count > 0)
                    targetText.text = _koreanFrames[0];
                else
                    // 프레임 캐싱이 안 된 상태라면 내용만 비움 (필요에 따라 Alpha 0 처리 등)
                    targetText.text = string.Empty;
            }
            else
            {
                // 일반 모드는 텍스트 원본은 유지하되, 보이는 글자 수를 0으로 강제
                targetText.maxVisibleCharacters = 0;
            }

            // UI 갱신 강제
            targetText.ForceMeshUpdate();
        }

        public override void Stop(bool snapToEnd = true)
        {
            base.Stop(snapToEnd);
            _typewriterTween?.Kill();
            _isPaused = false;

            if (snapToEnd && targetText != null)
            {
                if (string.IsNullOrEmpty(_cachedEndText))
                {
                    var configToUse = _overrideConfig ?? defaultConfig;
                    _cachedEndText = string.IsNullOrEmpty(configToUse.customText)
                        ? targetText.text
                        : configToUse.customText;
                }

                targetText.text = _cachedEndText;
                targetText.maxVisibleCharacters = 99999;
            }
        }
    }
```
- `customText`: 커스텀 텍스트입니다. 사용할 경우, 내부 텍스트로 변경하여 출력합니다. 프로젝트 실행중에 유동적으로 타이핑을 바꿔야 한다면 쓸 수 있게끔 구성했습니다.
- `TypewriterMode`: 한글 분해해서 출력하기 VS 그냥 한글자씩 출력하기. 당연하게도 전자느 한글만, 후자는 모두 지원합니다.
- `DurationMode`: Duration을 이 글자를 전부 치는데 걸리는 시간으로 할건지, 한 글자당 시간을 줄건지 정해서 인스펙터에서 직관적으로 사용 가능하게 분리했습니다.

- 인터넷과 블로그에 정리된 여러 요소들을 바탕으로 제가 구현한 Effect System에 녹아들게끔 설계하였습니다.
- 기본적으로 한글의 초성/중성/종성 조합 원리를 유니코드 연산으로 풀어내고, TMP의 Rich Text 태그가 깨지지 않도록 신경썼습니다. (유니티에세 제가 가장가장가장 싫어하는게 □□입니다. □□는 □ □□ □□□ □□□□ 에러 표시 문자입니다.)
- 한글 유니코드는 기본적으로 0XAC00(가) - 0xD743(힣) ~ㅋㅋㅋ~ 총 11172자가 규칙적으로 배열되어있습니다. 즉 `초성 19개 * 중성 21개 * 종성 28개`로 볼 수 있습니다.
- 분해연산:
  - `unicode = c - HangulBase`: 시작점으로부터의 오프셋을 구합니다. 이게 실제 시작인 `가`는 `0xAC00`에 존재하지만, 계산을 용이하게 하고 편하게 하려면 당연히 오프셋을 0으로 잡고 시작하는게 좋지 않겠습니까.
  - `choIndex = unicode / (21 * 28)`: 몫을 이용해 초성(ㄱ~ㅎ) 인덱스를 추출합니다.
  - `jungIndex = (unicode % (21 * 28)) / 28`: 나머지를 다시 28로 나눠 중성(ㅏ~ㅣ) 인덱스를 추출합니다.
  - `jongIndex = unicode % 28`: 마지막 나머지가 종성(받침) 인덱스가 됩니다. 종성이 없으면 0입니다.
- 여기까지 해서 가령 `힣` 라는 글자가 들어갔다면 다음과 같이 작동하게 됩니다.
  - `unicode = 0xD7A3 - 0xAC00 = 0x2ba3` (실제로는 const int 형식이지만 보기 편하게) 이 0x2ba3가 base 값이 됩니다. (이제 여기에 `0xAC00` 즉 `가`를 더하면 원하는 값이 뿅 나오게 되는거죠. 지금 나온 값을 십진수로 표현하면 `11171`가 됩니다.)
  - `choIndex = 0x2ba3 / (21 * 28)` 해서 중성과 종성을 없애버리면 초성으로 `18.99829931972789`가 나오고, int니깐 당연히 버림 처리를 해서 `18`을 얻습니다. 그럼 0부터 `ㄱ`이므로 18은 `ㅎ`이 됩니다.
  - 동일한 방식으로 중성과 종성을 구해주면 각각 `ㅣ`와 `ㅎ`이 나옵니다.
  - 이렇게 나온 것들을 바탕으로, 프레임을 더해줄겁니다. 리스트에는 그럼 `{'ㅎ', '히', '힣'}`이 들어가게 됩니다.
- 그럼 이제 이 귀여운 녀석을 어떻게 출력하느냐?
  -  UITypewriterEffect는 두가지 방식을 지원합니다. 글자가 그냥 나오게도 할 수 있고, 초성 중성 종성을 탁탁 분리해서 출력하는 방식도 있습니다. 이는 언젠가 이 구현한 놈이 영어도, 일어도, 중국어도 한국어도 모두모두 지원하는 아주 참한 녀석이 되기를 바람과 동시에, 한글은 분리해서 쳐지는게 더 멋지기 때문입니다. 즉, `Korean Assemble Mode`는 오직 Korean만 지원하는 아주 멋찐 녀석입니다. 으흐흐.
  - 한글 조합은 한글자씩 치려면 계속 글자가 변해야 하므로, maxVisibleCharacters를 쓸 수 없습니다. 그래서 앞서 계산을 담당한 `KoreanTypingHelper`를 통해서 `targetText.text = _koreanFrames[x]`를 통해 계속해서 텍스트 자체를 교체해줍니다. 물론 이러면... 성능상에서 부담이 좀 늘어나긴 합니다. (성능이 낮은 기기는 이 모드를 변경해주는 것도 좋은 방법일 수 있습니다!!!)
  - 거기에 만일, TMPRO에서 태그를 사용해서 글씨를 이쁘고 귀엽게 출력중이었거나 색이 변경되어야 한다면 UI가 쭈인님 주거요 하면서 고장이 나버립니다. 그래서 정규식을 하나 추가했습니다. 바로 `RichTextRegex`로 태그를 제외한 순수 문자열 `pureText`로만 타이핑 프레임을 계산시킵니다.
  - 그러면 원본 텍스트 구조를 순회함녀서 아직 타이핑 안된 부분은 `<alpha=#00>` 태그로 감싸서 안보이게 만들어줍니다. 이렇게 해버리면 글자가 타이핑 되는 동안 텍스트의 전체 레이아웃이나 정렬은 안건드리고 오직 타이핑 효과만 탁탁 나오게 해줍니다.
- 그럼, 사운드 처리는? 타건음이 찰져야 보는 맛이 사는 법이죠.
  - PlayOnShot을 남발하면 해보니깐 소리가 갑자기 겹쳐서 고막을 테러하거나 안들리는 경우가 있어서 이는 를 방지하기 위해 AudioPoolSize = 5 크기의 전용 AudioSource 배열을 Awake 시점에 미리 생성(`InitializeAudioPool`)해 두고 순환하며(`_audioPoolIndex`) 사용했습니다.
  - 아! 그런데 그냥 탁탁 치면 또 재미 없지 않습니까. 그래서 미묘하게 피치도 좀 바꾸고, 줄바꿈은 엔터 탁! 치고 잠깐 멈추는 그 손 맛을 위해 따로 설정해주었습니다. 프로그래머라면 그 손맛 참 좋아한다 이거에용.
- 여기까지만 할까 하다가, 이왕 모든 이펙트들을 Normal과 DOTween으로 만든김에, 둘 다 구현했습니다. DurationMode를 통해 전체 시간을 고정할지(TotalDuration), 글자 수에 비례해 시간을 늘릴지(`PerCharacter`) 결정하여 유연한 대응이 가능합니다.
- DOTween에서는 `DOVirtual.DelayedCall`과 `Tween.Pause() / Play()`를 활용해서 줄바꿈 문자(\n)를 만났을 때 newlineDelay만큼 연출을 잠시 멈추는(Pause) 로직으로 구현했습니다.
