using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Components.Effects.UI.Core;
using Core.Data.Enums;
using Core.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Components.Effects.UI.Types
{
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
}