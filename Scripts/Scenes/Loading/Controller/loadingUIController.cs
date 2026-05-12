using System.Collections;
using System.Collections.Generic;
using Components.Effects.UI;
using Components.Effects.UI.Core;
using Components.Effects.UI.Types;
using Core.Attributes;
using UnityEngine;

namespace Scenes.Loading.Controller
{
    public class LoadingUIController : MonoBehaviour
    {
        [Header("Effect Sequence")]
        [SerializeField] private EffectSequence loadingOpenEffectSequence;
        [SerializeField] private EffectSequence loadingCloseEffectSequence;
        [SerializeField] private EffectSequence mainLogoGlitchEffectSequence;

        [Header("Random Effect Sequence")]
        [Tooltip("사용할 이펙트를 Step 하나당 하나씩 할당해 둘 것.")]
        [SerializeField] private List<EffectSequence> decoSparkEffectSequence = new();

        [SerializeField] private UIFadeCanvasGroupEffect fadeCanvasGroupEffect;

        [Header("Detail")]
        [SerializeField] private float fadeInDuration = 1.5f;
        [SerializeField] private float fadeOutDuration = 1.5f;

        [SerializeField] private int repeatCount;
        [SerializeField] [MinMaxSlider(0f, 10f)]
        private Vector2 randomOffset = new(0f, 2.5f);
        [SerializeField] [MinMaxSlider(0f, 10f)]
        private Vector2 wait = new(2.0f, 3.0f);

        private readonly List<Coroutine> _sparkCoroutines = new();

        private FadeCanvasGroupConfig _newFadeCanvasGroupConfig;

        public IEnumerator LoadingOpenEffect()
        {
            foreach (var sequence in decoSparkEffectSequence)
            {
                if (sequence == null) continue;

                var randomStartOffset = Random.Range(randomOffset.x, randomOffset.y);

                var c = StartCoroutine(sequence.PlayRandomLoopRoutine(
                    repeatCount,
                    wait.x,
                    wait.y,
                    randomStartOffset
                ));

                _sparkCoroutines.Add(c);
            }

            StartCoroutine(mainLogoGlitchEffectSequence.PlaySequenceRoutine());

            fadeCanvasGroupEffect.GetComponent<CanvasGroup>().alpha = 0.0f;
            _newFadeCanvasGroupConfig.endAlpha = 1.0f;
            fadeCanvasGroupEffect.SetProperty(_newFadeCanvasGroupConfig, fadeInDuration);

            // 페이드 인 실행 명령 누락 추가
            fadeCanvasGroupEffect.PlayOverrideEffect();

            yield return StartCoroutine(loadingOpenEffectSequence.PlaySequenceRoutine());
        }

        public IEnumerator LoadingCloseEffect()
        {
            foreach (var c in _sparkCoroutines)
                if (c != null)
                    StopCoroutine(c);

            _sparkCoroutines.Clear();

            foreach (var sequence in decoSparkEffectSequence)
                if (sequence != null)
                    sequence.StopAll();

            fadeCanvasGroupEffect.GetComponent<CanvasGroup>().alpha = 1.0f;
            _newFadeCanvasGroupConfig.endAlpha = 0.0f;
            fadeCanvasGroupEffect.SetProperty(_newFadeCanvasGroupConfig, fadeOutDuration);

            // 페이드 아웃 실행 명령 누락 추가
            fadeCanvasGroupEffect.PlayOverrideEffect();

            var seqCoroutine = StartCoroutine(loadingCloseEffectSequence.PlaySequenceRoutine());

            //페이드 아웃 시간만큼 물리적으로 대기하여 로딩 잔재 현상 차단
            yield return new WaitForSeconds(fadeOutDuration);
            yield return seqCoroutine;
        }
    }
}