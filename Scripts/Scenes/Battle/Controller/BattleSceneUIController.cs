using System;
using System.Collections;
using Components.Common.Buttons.Core;
using Components.Effects.Cameras.Types;
using Components.Effects.UI.Core;
using Components.Effects.UI.Types;
using Core.Data.Enums;
using Core.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scenes.Battle.Controller
{
    [Serializable]
    public struct CameraPosConfig
    {
        public Vector3 position;
        public Vector3 rotation;
        public float fieldOfView;
    }

    public class BattleSceneUIController : MonoBehaviour
    {
        [Header("UI Panels & Elements")]
        [SerializeField] private GameObject mainFadePanel;
        [SerializeField] private GameObject gameUIElements;
        [SerializeField] private GameObject gameOverElements;
        [SerializeField] private GameObject gameOverFadePanel;
        [SerializeField] private GameObject gameEndLayout;

        [SerializeField] private CompoundButton backBtn;

        [SerializeField] private SpriteRenderer frontSprite01;
        [SerializeField] private SpriteRenderer frontSprite02;

        [Header("Sequence Settings")]
        [Tooltip("싱글로직 씬 진입 시 재생될 연출")]
        [SerializeField] private EffectSequence fadeInSequence;
        [SerializeField] private float fadeInDuration = 1.0f;
        [Tooltip("싱글로직 씬 퇴장 시 재생될 연출")]
        [SerializeField] private EffectSequence fadeOutSequence;
        [SerializeField] private float fadeOutDuration = 1.0f;

        [Header("UI Effects")]
        [SerializeField] private UIFadeEffect mainFadePanelFadeEffect;
        [SerializeField] private UIFadeCanvasGroupEffect gameOverFadePanelFadeCanvasGroupEffect;

        [Header("Camera Effects")]
        [SerializeField] private CameraMoveEffect cameraMoveEffect;

        [Header("Configs")]
        [Tooltip("Start/End, Init")]
        [SerializeField] private CameraPosConfig[] cameraSettings = new CameraPosConfig[2];
        private FadeConfig _uiFadeConfig;
        private CameraMoveConfig _cameraMoveConfig;
        private FadeCanvasGroupConfig _fadeCanvasGroupConfig;

        private bool _isGameOverTriggered = false;

        private void OnEnable()
        {
            backBtn.onClickEvent.AddListener(BackButtonOnClick);
        }

        private void OnDisable()
        {
            backBtn.onClickEvent.RemoveListener(BackButtonOnClick);
        }

        public void ResetUILoad()
        {
            _isGameOverTriggered = false;

            fadeInSequence?.StopAll();
            fadeOutSequence?.StopAll();

            _uiFadeConfig = mainFadePanelFadeEffect.DefaultConfig;
            _uiFadeConfig.endAlpha = 0.0f;
            mainFadePanelFadeEffect.SetProperty(_uiFadeConfig);

            _cameraMoveConfig = cameraMoveEffect.DefaultConfig;
            _cameraMoveConfig.position.start = cameraSettings[0].position;
            _cameraMoveConfig.position.end = cameraSettings[1].position;
            cameraMoveEffect.SetProperty(_cameraMoveConfig, 1.0f);

            gameUIElements.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }

        public void ResetUIExit()
        {
            fadeInSequence?.StopAll();
            fadeOutSequence?.StopAll();

            _uiFadeConfig = mainFadePanelFadeEffect.DefaultConfig;
            _uiFadeConfig.endAlpha = 1.0f;
            mainFadePanelFadeEffect.SetProperty(_uiFadeConfig);

            _cameraMoveConfig = cameraMoveEffect.DefaultConfig;
            _cameraMoveConfig.position.start = cameraSettings[1].position;
            _cameraMoveConfig.position.end = cameraSettings[0].position;
            cameraMoveEffect.SetProperty(_cameraMoveConfig, 1.0f);

            gameUIElements.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }

        public void GameOverResetUI()
        {
            CanvasGroup gameOverCanvasGroup = gameOverElements.GetComponent<CanvasGroup>();
            gameOverCanvasGroup.alpha = 0.0f;

            FadeCanvasGroupConfig fader = gameOverFadePanelFadeCanvasGroupEffect.DefaultConfig;
            fader.blockRaycasts = true;
            fader.endAlpha = 1.0f;
            gameOverFadePanelFadeCanvasGroupEffect.SetProperty(fader);
        }

        public IEnumerator PlayEnterUILoad()
        {
            ResetUILoad();
            if (fadeInSequence != null && fadeInSequence.steps.Count > 0)
            {
                yield return StartCoroutine(fadeInSequence.PlaySequenceRoutine());
                cameraMoveEffect.PlayOverrideEffect();
                StartCoroutine(OnFadeRoutine(frontSprite01, 0.0f, 1.0f));
                StartCoroutine(OnFadeRoutine(frontSprite02, 0.0f, 1.0f));
            }
        }

        public IEnumerator PlayExitUILoad()
        {
            ResetUIExit();

            if (fadeOutSequence != null && fadeOutSequence.steps.Count > 0)
            {
                cameraMoveEffect.PlayOverrideEffect();
                StartCoroutine(OnFadeRoutine(frontSprite01, 1.0f, 1.0f));
                StartCoroutine(OnFadeRoutine(frontSprite02, 1.0f, 1.0f));
                yield return StartCoroutine(fadeOutSequence.PlaySequenceRoutine());
            }
        }

        public void PlayGameOverRoutine()
        {
            if (_isGameOverTriggered) return;
            _isGameOverTriggered = true;

            GameOverResetUI();
            SoundManager.Instance.StopQueue();
            SoundManager.Instance.PlayBgm(BGMSoundType.Victory);
            gameOverFadePanelFadeCanvasGroupEffect.PlayOverrideEffect();
        }

        private void BackButtonOnClick()
        {
            StartCoroutine(BackToMainMenuRoutine());
        }

        private IEnumerator BackToMainMenuRoutine()
        {
            Time.timeScale = 1f;
            yield return StartCoroutine(PlayExitUILoad());
            SceneManager.LoadScene("03_Main");
        }

        public IEnumerator OnFadeRoutine(SpriteRenderer spriteRenderer, float targetAlpha, float duration)
        {
            if (spriteRenderer == null) yield break;

            var currentColor = spriteRenderer.color;
            var startAlpha = currentColor.a;
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                currentColor.a = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
                spriteRenderer.color = currentColor;
                yield return null;
            }

            currentColor.a = targetAlpha;
            spriteRenderer.color = currentColor;
        }
    }
}