using System;
using System.Collections;
using Components.Effects.UI;
using Components.Effects.UI.Core;
using Components.Effects.UI.Types;
using Core.Data.Enums;
using Core.Managers;
using Scenes.Intro.Manager;
using UnityEngine;

namespace Scenes.Intro.Controller
{
    public class IntroSceneUIController : MonoBehaviour
    {
        [SerializeField] private PasswordManager passwordManager;

        [Header("Effect Sequence")]
        [SerializeField] private EffectSequence introOpenEffectSequence;
        [SerializeField] private EffectSequence introCloseEffectSequence;
        [SerializeField] private EffectSequence loginFailEffects;

        [Header("Effects")]
        [SerializeField] private UIFadeCanvasGroupEffect fadeCanvasGroupEffect;
        [SerializeField] private UIMoveEffect moveEffect;
        [SerializeField] private UIShakeEffect uiShakeEffect;

        [Header("Detail")]
        [SerializeField] private float fadeInDuration = 2.0f;
        [SerializeField] private float fadeOutDuration = 3.0f;

        [SerializeField] private Vector2 moveStartPos = new Vector3(0, 1190, 0);
        [SerializeField] private Vector2 moveEndPos = new Vector3(0, 250, 0);

        [SerializeField] private float moveInDuration = 1.5f;
        [SerializeField] private float moveOutDuration = 3.0f;

        [SerializeField] private float sucVolume = 0.6f;

        private FadeCanvasGroupConfig _newFadeCanvasGroupConfig;

        private MoveConfig _newMoveConfig;

        private ShakeConfig _newShakeConfig;

        private SfxSoundType Suc => SfxSoundType.Suc;

        private void Awake()
        {
            // Start에 선언할 경우, Start는 실행순서를 보장하지는 않으므로
            // 유니티의 실행 순서로 인한 초기화 지연 문제가 발생
            // 몇번 테스트 해보니 Scale값이 0 0 0으로, default config 값이 복사가 안됨.
            _newMoveConfig = moveEffect.DefaultConfig;
            _newFadeCanvasGroupConfig = fadeCanvasGroupEffect.DefaultConfig;
        }

        private void OnEnable()
        {
            passwordManager.OnPasswordSuccess += HandleLoginSuccess;
            passwordManager.OnPasswordFailure += HandleLoginFailure;
        }

        private void OnDisable()
        {
            passwordManager.OnPasswordSuccess -= HandleLoginSuccess;
            passwordManager.OnPasswordFailure -= HandleLoginFailure;
        }

        private void HandleLoginSuccess()
        {
            StartCoroutine(LoginSuccessEffects(() => { IntroSceneManager.Instance.LoadNextScene(); }));
        }

        private void HandleLoginFailure()
        {
            StartCoroutine(LoginFailureEffects());
        }

        public void PlayPasswordWindowAppear()
        {
            IntroOpenEffects();
        }

        private void IntroOpenEffects()
        {
            fadeCanvasGroupEffect.GetComponent<CanvasGroup>().alpha = 0.0f;

            _newFadeCanvasGroupConfig.endAlpha = 1.0f;

            _newMoveConfig.position.start = moveStartPos;
            _newMoveConfig.position.end = moveEndPos;

            fadeCanvasGroupEffect.SetProperty(_newFadeCanvasGroupConfig, fadeInDuration);
            moveEffect.SetProperty(_newMoveConfig, moveInDuration);
            StartCoroutine(introOpenEffectSequence.PlaySequenceRoutine());
        }

        private IEnumerator IntroCloseEffects()
        {
            fadeCanvasGroupEffect.GetComponent<CanvasGroup>().alpha = 1.0f;

            _newFadeCanvasGroupConfig.endAlpha = 0.0f;

            _newMoveConfig.position.start = moveEndPos;
            _newMoveConfig.position.end = moveStartPos;
            _newMoveConfig.useSound = true;
            _newMoveConfig.soundType = SfxSoundType.UiClose;

            fadeCanvasGroupEffect.SetProperty(_newFadeCanvasGroupConfig, fadeOutDuration);
            moveEffect.SetProperty(_newMoveConfig, moveOutDuration);
            yield return StartCoroutine(introCloseEffectSequence.PlaySequenceRoutine());
        }

        private IEnumerator LoginSuccessEffects(Action onComplete)
        {
            SoundManager.Instance.PlaySfx(Suc, sucVolume);
            yield return StartCoroutine(IntroCloseEffects());
            onComplete?.Invoke();
        }

        private IEnumerator LoginFailureEffects()
        {
            yield return StartCoroutine(loginFailEffects.PlaySequenceRoutine());
        }
    }
}