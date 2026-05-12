using System.Collections;
using Components.Common.Buttons.Core;
using Components.Effects.UI;
using Components.Effects.UI.Core;
using Components.Effects.UI.Types;
using Core.Interfaces;
using Core.Managers;
using Cysharp.Threading.Tasks;
using Scenes.Projects.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scenes.Projects.Controller
{
    public class ProjectsSceneUIController : MonoBehaviour, ISceneUIController
    {
        [Header("UI Panels & Elements")]
        [SerializeField] private RectTransform mainPanel;
        [SerializeField] private CompoundButton backBtn;

        [Header("Sequence Settings")]
        [Tooltip("싱글로직 씬 진입 시 재생될 연출")]
        [SerializeField] private EffectSequence fadeInSequence;
        [SerializeField] private float fadeInDuration = 2.0f;
        [Tooltip("싱글로직 씬 퇴장 시 재생될 연출")]
        [SerializeField] private EffectSequence fadeOutSequence;
        [SerializeField] private float fadeOutDuration = 2.0f;

        [Tooltip("씬 진입 시 재생될 공통 연출")]
        [SerializeField] private EffectSequence startSequence;
        [Tooltip("씬 퇴장 시 재생될 공통 연출")]
        [SerializeField] private EffectSequence endSequence;

        [Header("Managers")]
        [SerializeField] private ProjectsPrefabManager prefabManager;

        [Header("Buttons")]
        [SerializeField] private CompoundButton prevButton;
        [SerializeField] private CompoundButton nextButton;

        [Header("Effects - Fade Canvas Group")]
        [SerializeField] private UIFadeCanvasGroupEffect uiSlidePanelFadeCanvasGroupEffect;

        [Header("Config - Fade Canvas Group")]
        private FadeCanvasGroupConfig _uiSlidePanelFadeCanvasGroupConfig;

        private void OnEnable()
        {
            if (prefabManager != null)
            {
                prefabManager.OnCanSlideLeftChanged += HandleLeftButtonState;
                prefabManager.OnCanSlideRightChanged += HandleRightButtonState;
            }

            if (prevButton != null)
                prevButton.onClickEvent.AddListener(OnPrevButtonClicked);
            if (nextButton != null)
                nextButton.onClickEvent.AddListener(OnNextButtonClicked);

            if (backBtn != null)
                backBtn.onClickEvent.AddListener(BackButtonOnClick);
        }

        private void OnDisable()
        {
            if (prefabManager != null)
            {
                prefabManager.OnCanSlideLeftChanged -= HandleLeftButtonState;
                prefabManager.OnCanSlideRightChanged -= HandleRightButtonState;
            }

            if (prevButton != null)
                prevButton.onClickEvent.RemoveListener(OnPrevButtonClicked);
            if (nextButton != null)
                nextButton.onClickEvent.RemoveListener(OnNextButtonClicked);

            if (backBtn != null)
                backBtn.onClickEvent.RemoveListener(BackButtonOnClick);
        }


        private void CommonReset()
        {
            if (prefabManager != null && prefabManager.IsReady) prefabManager.ResetToInitialState();
        }

        private void HandleLeftButtonState(bool canGoLeft)
        {
            if (prevButton != null)
                prevButton.SetInteractable(canGoLeft);
        }

        private void HandleRightButtonState(bool canGoRight)
        {
            if (nextButton != null)
                nextButton.SetInteractable(canGoRight);
        }

        private void OnPrevButtonClicked()
        {
            prefabManager.OnClickSlideLeft();
        }

        private void OnNextButtonClicked()
        {
            prefabManager.OnClickSlideRight();
        }

        private void BackButtonOnClick()
        {
            StartCoroutine(BackToMainMenuRoutine());
        }

        private IEnumerator BackToMainMenuRoutine()
        {
            if (GameSettingManager.GetDeviceMode())
            {
                yield return StartCoroutine(PlayExitUISingleLoad());
                SceneManager.LoadScene("03_Main");
            }
            else
            {
                yield return StartCoroutine(PlayExitUIPreloadRoutine());
                SceneSlideManager.Instance.ExecuteSlideAsync("03_Main").Forget();
            }
        }

        #region Single Load

        public void ResetUISingleLoad()
        {
            fadeInSequence?.StopAll();
            fadeOutSequence?.StopAll();

            _uiSlidePanelFadeCanvasGroupConfig = uiSlidePanelFadeCanvasGroupEffect.DefaultConfig;
            uiSlidePanelFadeCanvasGroupEffect.GetComponent<CanvasGroup>().alpha = 0;
            uiSlidePanelFadeCanvasGroupEffect.SetProperty(_uiSlidePanelFadeCanvasGroupConfig, fadeInDuration);

            CommonReset();

            Debug.Log("[ProjectsSceneUIController] ResetUISingleLoad: 단일 로드 UI 초기화 완료");
        }

        public void PlayEnterUISingleLoad()
        {
            uiSlidePanelFadeCanvasGroupEffect.GetComponent<CanvasGroup>().alpha = 0.0f;
            _uiSlidePanelFadeCanvasGroupConfig.endAlpha = 1.0f;
            _uiSlidePanelFadeCanvasGroupConfig.blockRaycasts = true;
            uiSlidePanelFadeCanvasGroupEffect.SetProperty(_uiSlidePanelFadeCanvasGroupConfig, fadeInDuration);

            StartCoroutine(fadeInSequence.PlaySequenceRoutine());
            StartCoroutine(startSequence.PlaySequenceRoutine());
        }

        public IEnumerator PlayExitUISingleLoad()
        {
            CommonReset();

            uiSlidePanelFadeCanvasGroupEffect.GetComponent<CanvasGroup>().alpha = 1.0f;
            _uiSlidePanelFadeCanvasGroupConfig.endAlpha = 0.0f;
            _uiSlidePanelFadeCanvasGroupConfig.blockRaycasts = false;
            uiSlidePanelFadeCanvasGroupEffect.SetProperty(_uiSlidePanelFadeCanvasGroupConfig, fadeOutDuration);

            StartCoroutine(endSequence.PlaySequenceRoutine());
            yield return StartCoroutine(fadeOutSequence.PlaySequenceRoutine());
        }

        #endregion

        #region Preload

        public void ResetUIPreload()
        {
            startSequence?.StopAll();
            endSequence?.StopAll();

            CommonReset();

            Debug.Log("[ProjectsSceneUIController] ResetUIPreload: 프리로드 UI 초기화 완료");
        }

        public void PlayEnterUIPreload()
        {
            if (startSequence != null) StartCoroutine(startSequence.PlaySequenceRoutine());
        }

        public void PlayExitUIPreload()
        {
            StartCoroutine(PlayExitUIPreloadRoutine());
        }

        public IEnumerator PlayExitUIPreloadRoutine()
        {
            if (endSequence != null) yield return StartCoroutine(endSequence.PlaySequenceRoutine());
        }

        #endregion
    }
}