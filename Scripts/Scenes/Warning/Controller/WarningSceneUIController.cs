using System;
using System.Collections;
using System.Collections.Generic;
using Components.Common.Buttons.Core;
using Components.Effects.UI;
using Components.Effects.UI.Core;
using Components.Effects.UI.Types;
using Core.Data.Enums;
using Scenes.Warning.Manager;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scenes.Warning.Controller
{
    public class WarningSceneUIController : MonoBehaviour
    {
        [Header("Effect Sequence")]
        [SerializeField] private EffectSequence warningOpenEffectSequence;
        [SerializeField] private EffectSequence warningCloseEffectSequence;
        [SerializeField] private EffectSequence typewriterEffectSequence;

        [SerializeField] private UIFadeCanvasGroupEffect fadeCanvasGroupEffect;

        [SerializeField] private List<SfxSoundType> startSounds;

        [Header("Detail")]
        [SerializeField] private float fadeInDuration = 1.5f;
        [SerializeField] private float fadeOutDuration = 1.5f;

        [SerializeField] private CompoundButton startButton;

        private FadeCanvasGroupConfig _fadeCanvasGroupConfig;

        private Coroutine _openEffectCoroutine;

        public void PlayWarningSceneAppear()
        {
            _openEffectCoroutine = StartCoroutine(WarningOpenEffect());
        }

        public void HandleTouchToStart()
        {
            if (_openEffectCoroutine != null) StopCoroutine(_openEffectCoroutine);

            warningOpenEffectSequence.StopAll(); // 진행 중인 시퀀스 강제 종료
            typewriterEffectSequence.StopAll();

            startButton.SetInteractable(false);
            StartCoroutine(TouchToStartEffect(() => { WarningSceneManager.Instance.LoadNextScene(); }));
        }

        private IEnumerator WarningOpenEffect()
        {
            fadeCanvasGroupEffect.GetComponent<CanvasGroup>().alpha = 0.0f;
            _fadeCanvasGroupConfig.useSound = true;
            _fadeCanvasGroupConfig.soundType = startSounds[Random.Range(0, startSounds.Count)];
            _fadeCanvasGroupConfig.endAlpha = 1.0f;
            _fadeCanvasGroupConfig.blockRaycasts = true;
            fadeCanvasGroupEffect.SetProperty(_fadeCanvasGroupConfig, fadeInDuration);
            yield return StartCoroutine(warningOpenEffectSequence.PlaySequenceRoutine());
            StartCoroutine(typewriterEffectSequence.PlaySequenceRoutine());
        }

        private IEnumerator WarningCloseEffect()
        {
            fadeCanvasGroupEffect.GetComponent<CanvasGroup>().alpha = 1.0f;
            _fadeCanvasGroupConfig.useSound = true;
            _fadeCanvasGroupConfig.soundType = startSounds[Random.Range(0, startSounds.Count)];
            _fadeCanvasGroupConfig.endAlpha = 0.0f;
            _fadeCanvasGroupConfig.blockRaycasts = false;
            fadeCanvasGroupEffect.SetProperty(_fadeCanvasGroupConfig, fadeOutDuration);
            yield return StartCoroutine(warningCloseEffectSequence.PlaySequenceRoutine());
            yield return new WaitForSeconds(fadeCanvasGroupEffect.Duration);
        }

        private IEnumerator TouchToStartEffect(Action onComplete)
        {
            yield return StartCoroutine(WarningCloseEffect());
            onComplete?.Invoke();
        }
    }
}