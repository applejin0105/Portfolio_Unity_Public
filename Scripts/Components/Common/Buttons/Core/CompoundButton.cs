using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Components.Common.Buttons.Core
{
    public class CompoundButton : UIButtonConfigurableEffect<ButtonConfig>
    {
        private static readonly int GlowPower = Shader.PropertyToID("_GlowPower");
        private static readonly int GlowColor = Shader.PropertyToID("_GlowColor");

        [Header("Config")]
        [SerializeField] protected ButtonConfig defaultConfig;
        private readonly List<Material> _instantiatedGraphicMats = new();
        private Tween _compoundButtonTween;

        private Coroutine _effectCoroutine;

        private ButtonConfig? _overrideConfig;

        public ButtonConfig DefaultConfig => defaultConfig;

        protected override void Awake()
        {
            base.Awake();
            var config = defaultConfig;

            if (config.graphicSettings != null)
                for (var i = 0; i < config.graphicSettings.Count; i++)
                {
                    var gConfig = config.graphicSettings[i];

                    // 원본(Base)을 저장할 공간 초기화
                    gConfig.baseState = new UIElementStateData();

                    // 세팅된 고유 속성 캡처
                    var rect = gConfig.targetRect != null
                        ? gConfig.targetRect
                        : gConfig.target != null
                            ? gConfig.target.rectTransform
                            : null;
                    if (rect != null)
                    {
                        gConfig.baseState.scale = rect.localScale;
                        gConfig.baseState.useScale = true;
                    }

                    if (gConfig.target != null)
                    {
                        gConfig.baseState.color = gConfig.target.color;
                        gConfig.baseState.useColor = true;

                        if (gConfig.target is Image img)
                        {
                            gConfig.baseState.sprite = img.sprite;
                            gConfig.baseState.useSprite = true;
                        }

                        if (gConfig.target is TextMeshProUGUI tmp)
                        {
                            gConfig.baseState.text = tmp.text;
                            gConfig.baseState.useText = true;
                        }
                    }

                    // Bloom 캡처
                    if (gConfig.bloomImage != null)
                    {
                        gConfig.baseState.bloomScaleX = gConfig.bloomImage.rectTransform.localScale.x;
                        gConfig.baseState.bloomScaleY = gConfig.bloomImage.rectTransform.localScale.y;
                        gConfig.baseState.bloomScaleZ = gConfig.bloomImage.rectTransform.localScale.z;
                        gConfig.baseState.bloomColor = gConfig.bloomImage.color;
                        gConfig.baseState.bloomAlpha = gConfig.bloomImage.color.a;
                        gConfig.baseState.useBloom = true;
                    }

                    // Material Glow 캡처
                    if (gConfig.targetMaterial != null && gConfig.target != null)
                    {
                        var newMat = new Material(gConfig.targetMaterial);
                        _instantiatedGraphicMats.Add(newMat);
                        gConfig.targetMaterial = newMat;

                        if (gConfig.target is TextMeshProUGUI tmp) tmp.fontMaterial = newMat;
                        else if (gConfig.target is Image img) img.material = newMat;

                        var isGlowKeywordOn = newMat.IsKeywordEnabled("GLOW_ON");

                        gConfig.baseState.glowPower = isGlowKeywordOn && newMat.HasProperty(GlowPower)
                            ? newMat.GetFloat(GlowPower)
                            : 0f;

                        gConfig.baseState.glowColor =
                            newMat.HasProperty(GlowColor) ? newMat.GetColor(GlowColor) : Color.clear;
                    }

                    config.graphicSettings[i] = gConfig;
                }

            defaultConfig = config;
        }

        private void Start()
        {
            Stop();
            UpdateButtonState();
        }

        private void OnDestroy()
        {
            if (_compoundButtonTween != null && _compoundButtonTween.IsActive()) _compoundButtonTween.Kill();
            foreach (var mat in _instantiatedGraphicMats)
                if (mat != null)
                    Destroy(mat);
            _instantiatedGraphicMats.Clear();
        }

        public override void SetProperty(ButtonConfig configData)
        {
            _overrideConfig = configData;
        }

        public override void ClearProperty()
        {
            _overrideConfig = null;
        }

        public override void PlayOverrideEffect()
        {
            if (_effectCoroutine != null) StopCoroutine(_effectCoroutine);
            _compoundButtonTween?.Kill();
            _effectCoroutine = StartCoroutine(ExecuteEffect());
        }

        public override IEnumerator PlayWaitableEffect()
        {
            if (_effectCoroutine != null) StopCoroutine(_effectCoroutine);
            _effectCoroutine = StartCoroutine(ExecuteEffect());
            yield return _effectCoroutine;
        }

        public void SetText(string newText, int index = 0)
        {
            if (defaultConfig.graphicSettings == null) return;
            var textCount = 0;
            for (var i = 0; i < defaultConfig.graphicSettings.Count; i++)
            {
                var gConfig = defaultConfig.graphicSettings[i];
                if (gConfig.target is TextMeshProUGUI tmp)
                {
                    if (textCount == index)
                    {
                        // 코드에서 텍스트 변경 시 원본(Base) 데이터도 갱신
                        gConfig.baseState.text = newText;
                        if (gConfig.useAlways) gConfig.alwaysState.text = newText;
                        tmp.text = newText;
                        defaultConfig.graphicSettings[i] = gConfig;
                        return;
                    }

                    textCount++;
                }
            }
        }

        public void SetImage(Sprite newSprite, int index = 0)
        {
            if (defaultConfig.graphicSettings == null) return;
            var imageCount = 0;
            for (var i = 0; i < defaultConfig.graphicSettings.Count; i++)
            {
                var gConfig = defaultConfig.graphicSettings[i];
                if (gConfig.target is Image img)
                {
                    if (imageCount == index)
                    {
                        gConfig.baseState.sprite = newSprite;
                        if (gConfig.useAlways) gConfig.alwaysState.sprite = newSprite;
                        img.sprite = newSprite;
                        defaultConfig.graphicSettings[i] = gConfig;
                        return;
                    }

                    imageCount++;
                }
            }
        }

        private UIElementStateData GetResolvedStateData(GraphicConfig config, ButtonState state)
        {
            var resolved = new UIElementStateData
            {
                scale = config.baseState.scale,
                useScale = config.baseState.useScale,
                color = config.baseState.color,
                useColor = config.baseState.useColor,
                sprite = config.baseState.sprite,
                useSprite = config.baseState.useSprite,
                text = config.baseState.text,
                useText = config.baseState.useText,
                glowPower = config.baseState.glowPower,
                glowColor = config.baseState.glowColor,
                useGlow = config.baseState.useGlow,
                bloomScaleX = config.baseState.bloomScaleX,
                bloomScaleY = config.baseState.bloomScaleY,
                bloomScaleZ = config.baseState.bloomScaleZ,
                bloomAlpha = config.baseState.bloomAlpha,
                bloomColor = config.baseState.bloomColor,
                useBloom = config.baseState.useBloom
            };

            UIElementStateData activeState;
            if (config.useAlways)
                activeState = config.alwaysState;
            else
                activeState = state switch
                {
                    ButtonState.Hover => config.hoverState,
                    ButtonState.Pressed => config.pressedState,
                    ButtonState.Disabled => config.disabledState,
                    _ => config.baseState
                };

            if (activeState.useScale)
            {
                resolved.scale = activeState.scale;
                resolved.useScale = true;
            }

            if (activeState.useColor)
            {
                resolved.color = activeState.color;
                resolved.useColor = true;
            }

            if (activeState.useSprite)
            {
                resolved.sprite = activeState.sprite;
                resolved.useSprite = true;
            }

            if (activeState.useText)
            {
                resolved.text = activeState.text;
                resolved.useText = true;
            }

            if (activeState.useGlow)
            {
                resolved.glowPower = activeState.glowPower;
                resolved.glowColor = activeState.glowColor;
                resolved.useGlow = true;
            }

            if (activeState.useBloom)
            {
                resolved.bloomScaleX = activeState.bloomScaleX;
                resolved.bloomScaleY = activeState.bloomScaleY;
                resolved.bloomScaleZ = activeState.bloomScaleZ;
                resolved.bloomAlpha = activeState.bloomAlpha;
                resolved.bloomColor = activeState.bloomColor;
                resolved.useBloom = true;
            }

            return resolved;
        }

        protected override IEnumerator ExecuteEffect()
        {
            var configToUse = _overrideConfig ?? defaultConfig;
            var duration = configToUse.animDuration;

            yield return animType switch
            {
                AnimType.Normal => CompoundButtonRoutine(configToUse, duration),
                AnimType.DoTween => CompoundButtonDoTweenRoutine(configToUse, duration),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private IEnumerator CompoundButtonRoutine(ButtonConfig configToUse, float globalDuration)
        {
            var elapsed = 0f;
            var maxDuration = 0f;
            var gCount = configToUse.graphicSettings?.Count ?? 0;
            var gStates = new ElementState[gCount];

            for (var i = 0; i < gCount; i++)
            {
                var gConfig = configToUse.graphicSettings[i];
                var rState = GetResolvedStateData(gConfig, currentState);

                gStates[i] = new ElementState();

                var elemDuration = gConfig.useSeparateDuration ? gConfig.customDuration : globalDuration;
                gStates[i].Duration = elemDuration;

                if (elemDuration > maxDuration) maxDuration = elemDuration;

                // 1. RectTransform Scale
                gStates[i].TargetRect = gConfig.targetRect != null
                    ? gConfig.targetRect
                    : gConfig.target != null
                        ? gConfig.target.rectTransform
                        : null;
                if (gStates[i].TargetRect != null)
                {
                    gStates[i].StartScale = gStates[i].TargetRect.localScale;
                    gStates[i].TargetScale = rState.scale;
                }

                // 2. Graphic Color
                if (gConfig.target != null)
                {
                    gStates[i].GraphicTarget = gConfig.target;
                    gStates[i].StartColor = gConfig.target.color;
                    gStates[i].TargetColor = rState.color;
                }

                // 3. Material Glow
                if (gConfig.targetMaterial != null)
                {
                    gStates[i].Mat = gConfig.targetMaterial;
                    gStates[i].Mat.EnableKeyword("GLOW_ON");
                    gStates[i].StartGlow =
                        gStates[i].Mat.HasProperty(GlowPower) ? gStates[i].Mat.GetFloat(GlowPower) : 0f;
                    gStates[i].StartGlowColor = gStates[i].Mat.HasProperty(GlowColor)
                        ? gStates[i].Mat.GetColor(GlowColor)
                        : rState.glowColor;

                    gStates[i].TargetGlow = rState.glowPower;
                    gStates[i].TargetGlowColor = rState.glowColor;
                }

                // 4. Fake Bloom
                if (gConfig.bloomImage != null)
                {
                    gStates[i].BloomImage = gConfig.bloomImage;
                    gStates[i].StartBloomScale = gConfig.bloomImage.rectTransform.localScale;
                    gStates[i].StartBloomColor = gConfig.bloomImage.color;

                    gStates[i].TargetBloomScale =
                        new Vector3(rState.bloomScaleX, rState.bloomScaleY, rState.bloomScaleZ);

                    var tBloomColor = rState.bloomColor;
                    tBloomColor.a = rState.bloomAlpha;
                    gStates[i].TargetBloomColor = tBloomColor;
                }
            }

            while (elapsed < maxDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                for (var i = 0; i < gCount; i++)
                {
                    var st = gStates[i];

                    var progress = st.Duration > 0f ? Mathf.Clamp01(elapsed / st.Duration) : 1f;
                    var t = 1f - (1f - progress) * (1f - progress);

                    if (st.TargetRect != null)
                        st.TargetRect.localScale = Vector3.Lerp(st.StartScale, st.TargetScale, t);
                    if (st.GraphicTarget != null)
                        st.GraphicTarget.color = Color.Lerp(st.StartColor, st.TargetColor, t);

                    if (st.Mat != null)
                    {
                        st.Mat.SetFloat(GlowPower, Mathf.Lerp(st.StartGlow, st.TargetGlow, t));
                        st.Mat.SetColor(GlowColor, Color.Lerp(st.StartGlowColor, st.TargetGlowColor, t));
                    }

                    if (st.BloomImage != null)
                    {
                        st.BloomImage.rectTransform.localScale =
                            Vector3.Lerp(st.StartBloomScale, st.TargetBloomScale, t);
                        st.BloomImage.color = Color.Lerp(st.StartBloomColor, st.TargetBloomColor, t);
                    }
                }

                yield return null;
            }

            for (var i = 0; i < gCount; i++)
            {
                var st = gStates[i];

                if (st.TargetRect != null) st.TargetRect.localScale = st.TargetScale;
                if (st.GraphicTarget != null) st.GraphicTarget.color = st.TargetColor;

                if (st.Mat != null)
                {
                    st.Mat.SetFloat(GlowPower, st.TargetGlow);
                    st.Mat.SetColor(GlowColor, st.TargetGlowColor);
                    if (st.TargetGlow <= 0f) st.Mat.DisableKeyword("GLOW_ON");
                }

                if (st.BloomImage != null)
                {
                    st.BloomImage.rectTransform.localScale = st.TargetBloomScale;
                    st.BloomImage.color = st.TargetBloomColor;
                }

                var gConfig = configToUse.graphicSettings[i];
                var rState = GetResolvedStateData(gConfig, currentState);
                if (gConfig.target is TextMeshProUGUI tmp && rState.useText) tmp.text = rState.text ?? "";
                if (gConfig.target is Image img && rState.useSprite) img.sprite = rState.sprite;
            }
        }

        private IEnumerator CompoundButtonDoTweenRoutine(ButtonConfig configToUse, float globalDuration)
        {
            _compoundButtonTween?.Kill();
            var seq = DOTween.Sequence().SetUpdate(true);

            if (configToUse.graphicSettings != null)
                foreach (var gConfig in configToUse.graphicSettings)
                {
                    var rState = GetResolvedStateData(gConfig, currentState);

                    var durationToUse = gConfig.useSeparateDuration ? gConfig.customDuration : globalDuration;

                    var rect = gConfig.targetRect != null
                        ? gConfig.targetRect
                        : gConfig.target != null
                            ? gConfig.target.rectTransform
                            : null;
                    if (rect != null) seq.Join(rect.DOScale(rState.scale, durationToUse).SetEase(Ease.OutQuad));

                    if (gConfig.target != null)
                        seq.Join(gConfig.target.DOColor(rState.color, durationToUse).SetEase(Ease.OutQuad));

                    if (gConfig.targetMaterial != null)
                    {
                        if (rState.glowPower > 0)
                        {
                            gConfig.targetMaterial.EnableKeyword("GLOW_ON");
                            seq.Join(gConfig.targetMaterial.DOFloat(rState.glowPower, "_GlowPower", durationToUse));
                            seq.Join(gConfig.targetMaterial.DOColor(rState.glowColor, "_GlowColor", durationToUse));
                        }
                        else
                        {
                            seq.Join(gConfig.targetMaterial.DOFloat(0f, "_GlowPower", durationToUse)
                                .OnComplete(() => gConfig.targetMaterial.DisableKeyword("GLOW_ON")));
                            seq.Join(gConfig.targetMaterial.DOColor(rState.glowColor, "_GlowColor", durationToUse));
                        }
                    }

                    if (gConfig.bloomImage != null)
                    {
                        var bColor = rState.bloomColor;
                        bColor.a = rState.bloomAlpha;
                        var bScale = new Vector3(rState.bloomScaleX, rState.bloomScaleY, rState.bloomScaleZ);

                        seq.Join(gConfig.bloomImage.rectTransform.DOScale(bScale, durationToUse).SetEase(Ease.OutQuad));
                        seq.Join(gConfig.bloomImage.DOColor(bColor, durationToUse).SetEase(Ease.OutQuad));
                    }

                    seq.InsertCallback(durationToUse, () =>
                    {
                        if (gConfig.target is TextMeshProUGUI tmp && rState.useText) tmp.text = rState.text ?? "";
                        if (gConfig.target is Image img && rState.useSprite) img.sprite = rState.sprite;
                    });
                }

            _compoundButtonTween = seq;
            yield return _compoundButtonTween.WaitForCompletion();
        }

        public override void Stop(bool snapToEnd = true)
        {
            base.Stop(snapToEnd);

            if (_effectCoroutine != null) StopCoroutine(_effectCoroutine);
            if (_compoundButtonTween != null && _compoundButtonTween.IsActive()) _compoundButtonTween.Kill();

            if (snapToEnd && defaultConfig.graphicSettings != null)
                foreach (var gConfig in defaultConfig.graphicSettings)
                {
                    var rState = GetResolvedStateData(gConfig, currentState);

                    var rect = gConfig.targetRect != null
                        ? gConfig.targetRect
                        : gConfig.target != null
                            ? gConfig.target.rectTransform
                            : null;
                    if (rect != null) rect.localScale = rState.scale;

                    if (gConfig.target != null)
                    {
                        gConfig.target.color = rState.color;
                        if (gConfig.target is TextMeshProUGUI tmp && rState.useText) tmp.text = rState.text ?? "";
                        if (gConfig.target is Image img && rState.useSprite) img.sprite = rState.sprite;
                    }

                    if (gConfig.targetMaterial != null)
                    {
                        gConfig.targetMaterial.SetFloat(GlowPower, rState.glowPower);
                        gConfig.targetMaterial.SetColor(GlowColor, rState.glowColor);
                        if (rState.glowPower > 0) gConfig.targetMaterial.EnableKeyword("GLOW_ON");
                        else gConfig.targetMaterial.DisableKeyword("GLOW_ON");
                    }

                    if (gConfig.bloomImage != null)
                    {
                        gConfig.bloomImage.rectTransform.localScale = new Vector3(rState.bloomScaleX,
                            rState.bloomScaleY, rState.bloomScaleZ);
                        var c = rState.bloomColor;
                        c.a = rState.bloomAlpha;
                        gConfig.bloomImage.color = c;
                    }
                }
        }

        private struct ElementState
        {
            public Graphic GraphicTarget;
            public RectTransform TargetRect;
            public Color StartColor, TargetColor;
            public Vector3 StartScale, TargetScale;

            public Material Mat;
            public float StartGlow, TargetGlow;
            public Color StartGlowColor, TargetGlowColor;

            public Image BloomImage;
            public Color StartBloomColor, TargetBloomColor;
            public Vector3 StartBloomScale, TargetBloomScale;

            public float Duration;
        }
    }
}