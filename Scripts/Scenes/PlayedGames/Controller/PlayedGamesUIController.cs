using System.Collections;
using Components.Common.Buttons.Core;
using Components.Effects.UI;
using Components.Effects.UI.Core;
using Components.Effects.UI.Types;
using Core.Interfaces;
using Core.Managers;
using Cysharp.Threading.Tasks;
using Scenes.PlayedGames.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scenes.PlayedGames.Controller
{
    public class PlayedGamesUIController : MonoBehaviour, ISceneUIController
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
        [SerializeField] private PlayedGamesPrefabManager prefabManager;

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
            // true 전달 시 활성화 상태, false 전달 시 즉시 Disabled 애니메이션 적용 및 클릭 차단
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
                PlayExitUIPreload();
                SceneSlideManager.Instance.ExecuteSlideAsync("03_Main").Forget();
            }
        }

        #region Single Load (단일 로드 UI 연출)

        /// <summary>
        ///     단일 로드 씬의 UI 상태를 초기화합니다.
        /// </summary>
        public void ResetUISingleLoad()
        {
            fadeInSequence?.StopAll();
            fadeOutSequence?.StopAll();

            _uiSlidePanelFadeCanvasGroupConfig = uiSlidePanelFadeCanvasGroupEffect.DefaultConfig;
            uiSlidePanelFadeCanvasGroupEffect.GetComponent<CanvasGroup>().alpha = 0;
            uiSlidePanelFadeCanvasGroupEffect.SetProperty(_uiSlidePanelFadeCanvasGroupConfig, fadeInDuration);

            CommonReset();

            Debug.Log("[MainSceneUIController] ResetUISingleLoad: 단일 로드 UI 초기화 완료");
        }

        /// <summary>
        ///     단일 로드 씬 입장 UI 연출을 시작합니다.
        /// </summary>
        public void PlayEnterUISingleLoad()
        {
            uiSlidePanelFadeCanvasGroupEffect.GetComponent<CanvasGroup>().alpha = 0;
            _uiSlidePanelFadeCanvasGroupConfig.endAlpha = 1.0f;
            _uiSlidePanelFadeCanvasGroupConfig.blockRaycasts = true;
            uiSlidePanelFadeCanvasGroupEffect.SetProperty(_uiSlidePanelFadeCanvasGroupConfig, fadeOutDuration);

            StartCoroutine(fadeInSequence.PlaySequenceRoutine());
            StartCoroutine(startSequence.PlaySequenceRoutine());
        }

        /// <summary>
        ///     단일 로드 씬 퇴장 UI 연출을 시작합니다.
        /// </summary>
        public IEnumerator PlayExitUISingleLoad()
        {
            CommonReset();

            uiSlidePanelFadeCanvasGroupEffect.GetComponent<CanvasGroup>().alpha = 1.0f;
            _uiSlidePanelFadeCanvasGroupConfig.endAlpha = 0.0f;
            _uiSlidePanelFadeCanvasGroupConfig.blockRaycasts = false;
            uiSlidePanelFadeCanvasGroupEffect.SetProperty(_uiSlidePanelFadeCanvasGroupConfig, fadeInDuration);

            StartCoroutine(endSequence.PlaySequenceRoutine());
            yield return StartCoroutine(fadeOutSequence.PlaySequenceRoutine());
        }

        #endregion

        #region Preload

        /// <summary>
        ///     프리로드 씬의 UI 상태를 초기화합니다.
        /// </summary>
        public void ResetUIPreload()
        {
            startSequence?.StopAll();
            endSequence?.StopAll();

            CommonReset();

            Debug.Log("[PlayedGamesUIController] ResetUIPreload: 프리로드 UI 초기화 완료");
        }

        /// <summary>
        ///     프리로드 씬 입장 UI 연출을 시작합니다.
        /// </summary>
        public void PlayEnterUIPreload()
        {
            if (startSequence != null && startSequence.steps.Count > 0)
                StartCoroutine(startSequence.PlaySequenceRoutine());
        }

        /// <summary>
        ///     프리로드 씬 퇴장 UI 연출을 시작합니다.
        /// </summary>
        public void PlayExitUIPreload()
        {
            if (endSequence != null && endSequence.steps.Count > 0) StartCoroutine(endSequence.PlaySequenceRoutine());
        }

        #endregion
    }
}