using System;
using System.Collections;
using Components.Common.Buttons.Core;
using Components.Effects.UI;
using Components.Effects.UI.Core;
using Components.Effects.UI.Types;
using Core.Interfaces;
using Core.Managers;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scenes.BasicInfo.Controller
{
    public enum Index
    {
        BasicInfo,
        Introduce,
        Reason,
        Philosophy,
        Stack,
        Career
    }

    public class BasicInfoSceneUIController : MonoBehaviour, ISceneUIController
    {
        private const string MainScene = "03_Main";
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

        [Header("UI Sequences (Preload)")]
        [Tooltip("씬 진입 시 재생될 공통 연출")]
        [SerializeField] private EffectSequence startSequence;
        [Tooltip("씬 퇴장 시 재생될 공통 연출")]
        [SerializeField] private EffectSequence endSequence;

        [Header("Index")]
        [SerializeField] private Index currentIndex = Index.BasicInfo;

        [Header("Effects - Fade Canvas Group")]
        [SerializeField] private UIFadeCanvasGroupEffect uiSlidePanelFadeCanvasGroupEffect;

        [Header("Effects - Fade")]
        [SerializeField] private UIFadeEffect backBtnFadeEffect;

        [Header("Effects - Move")]
        [SerializeField] private UIMoveEffect uiIndexMoveEffect;
        [SerializeField] private Vector3 uiIndexPositionStart;
        [SerializeField] private Vector3 uiIndexPositionEnd;

        [SerializeField] private UIMoveEffect uiIndexMainMoveEffect;
        [SerializeField] private Vector3 uiIndexMainPositionStart;
        [SerializeField] private Vector3 uiIndexMainPositionEnd;

        [SerializeField] private UIMoveEffect uiIndexSubMoveEffect;
        [SerializeField] private Vector3 uiIndexSubPositionStart;
        [SerializeField] private Vector3 uiIndexSubPositionEnd;

        [Header("Text")]
        [SerializeField] private TextMeshProUGUI basicInfoText;
        [SerializeField] private TextMeshProUGUI introduceText;
        [SerializeField] private TextMeshProUGUI reasonText;
        [SerializeField] private TextMeshProUGUI philosophyText;
        [SerializeField] private TextMeshProUGUI stackText;
        [SerializeField] private TextMeshProUGUI careerText;

        [Header("Config - Fade")]
        private FadeConfig _backButtonFadeConfig;
        private MoveConfig _uiIndexMainMoveConfig;

        [Header("Config - Move")]
        private MoveConfig _uiIndexMoveConfig;
        private MoveConfig _uiIndexSubMoveConfig;

        [Header("Config - Fade Canvas Group")]
        private FadeCanvasGroupConfig _uiSlidePanelFadeCanvasGroupConfig;
        public Index CurrentIndex => currentIndex;

        private void OnEnable()
        {
            if (backBtn != null)
                backBtn.onClickEvent.AddListener(OnClickBack);
        }

        private void OnDisable()
        {
            if (backBtn != null)
                backBtn.onClickEvent.RemoveListener(OnClickBack);
        }

        public event Action<Index, Index> OnIndexChangedEvent;

        private void CommonReset(bool isIn)
        {
            _uiIndexMoveConfig = uiIndexMoveEffect.DefaultConfig;
            _uiIndexMainMoveConfig = uiIndexMainMoveEffect.DefaultConfig;
            _uiIndexSubMoveConfig = uiIndexSubMoveEffect.DefaultConfig;

            if (isIn)
            {
                _uiIndexMoveConfig.position.start = uiIndexPositionStart;
                _uiIndexMoveConfig.position.end = uiIndexPositionEnd;
                uiIndexMoveEffect.GetComponent<RectTransform>().anchoredPosition = uiIndexPositionStart;

                _uiIndexMainMoveConfig.position.start = uiIndexMainPositionStart;
                _uiIndexMainMoveConfig.position.end = uiIndexMainPositionEnd;
                uiIndexMainMoveEffect.GetComponent<RectTransform>().anchoredPosition = uiIndexMainPositionStart;

                _uiIndexSubMoveConfig.position.start = uiIndexSubPositionStart;
                _uiIndexSubMoveConfig.position.end = uiIndexSubPositionEnd;
                uiIndexSubMoveEffect.GetComponent<RectTransform>().anchoredPosition = uiIndexSubPositionStart;

                backBtn.GetComponent<Image>().color = new Color(255, 255, 255, 0);
                _backButtonFadeConfig = backBtnFadeEffect.DefaultConfig;
                backBtn.IsInteractable = true;
                _backButtonFadeConfig.endAlpha = 1.0f;
            }
            else
            {
                _uiIndexMoveConfig.position.start = uiIndexPositionEnd;
                _uiIndexMoveConfig.position.end = uiIndexPositionStart;
                uiIndexMoveEffect.GetComponent<RectTransform>().anchoredPosition = uiIndexPositionEnd;

                _uiIndexMainMoveConfig.position.start = uiIndexMainPositionEnd;
                _uiIndexMainMoveConfig.position.end = uiIndexMainPositionStart;
                uiIndexMainMoveEffect.GetComponent<RectTransform>().anchoredPosition = uiIndexMainPositionEnd;

                _uiIndexSubMoveConfig.position.start = uiIndexSubPositionEnd;
                _uiIndexSubMoveConfig.position.end = uiIndexSubPositionStart;
                uiIndexSubMoveEffect.GetComponent<RectTransform>().anchoredPosition = uiIndexSubPositionEnd;

                _backButtonFadeConfig = backBtnFadeEffect.DefaultConfig;
                backBtn.IsInteractable = false;
                _backButtonFadeConfig.endAlpha = 0.0f;
            }

            backBtnFadeEffect.SetProperty(_backButtonFadeConfig, 1.0f);
            uiIndexMoveEffect.SetProperty(_uiIndexMoveConfig);
            uiIndexMainMoveEffect.SetProperty(_uiIndexMainMoveConfig);
            uiIndexSubMoveEffect.SetProperty(_uiIndexSubMoveConfig);
        }

        private void CommonResetUIIn()
        {
            CommonReset(true);
        }

        private void CommonResetUIOut()
        {
            CommonReset(false);
        }

        private void OnClickBack()
        {
            StartCoroutine(BackToMainMenuRoutine());
        }

        private IEnumerator BackToMainMenuRoutine()
        {
            if (GameSettingManager.GetDeviceMode())
            {
                yield return StartCoroutine(PlayExitUISingleLoad());
                SceneManager.LoadScene(MainScene);
            }
            else
            {
                // 뒤로가기 버튼 클릭 시에도 퇴장 연출 완료 대기
                yield return StartCoroutine(PlayExitUIPreloadRoutine());
                SceneSlideManager.Instance.ExecuteSlideAsync("03_Main").Forget();
            }
        }

        public void OnClickIndexTitle(Index clickedIndex)
        {
            // 동일한 인덱스 클릭 시 무시 (불필요한 연산 방지)
            if (currentIndex == clickedIndex) return;

            var oldIndex = currentIndex;
            currentIndex = clickedIndex;

            // 상태가 변경되었음을 모든 구독자에게 알림
            OnIndexChangedEvent?.Invoke(currentIndex, oldIndex);
        }

        #region Single Load

        /// <summary>
        ///     단일 로드 씬의 UI 상태를 초기화합니다.
        /// </summary>
        public void ResetUISingleLoad()
        {
            fadeInSequence?.StopAll();
            fadeOutSequence?.StopAll();

            _uiSlidePanelFadeCanvasGroupConfig = uiSlidePanelFadeCanvasGroupEffect.DefaultConfig;
            uiSlidePanelFadeCanvasGroupEffect.GetComponent<CanvasGroup>().alpha = 0f;
            uiSlidePanelFadeCanvasGroupEffect.SetProperty(_uiSlidePanelFadeCanvasGroupConfig, fadeInDuration);

            CommonResetUIIn();

            Debug.Log("[BasicInfoSceneUIController] ResetUISingleLoad: 단일 로드 UI 초기화 완료");
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
            CommonResetUIOut();

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

            if (uiSlidePanelFadeCanvasGroupEffect != null)
                uiSlidePanelFadeCanvasGroupEffect.GetComponent<CanvasGroup>().alpha = 1.0f;

            CommonResetUIIn();

            Debug.Log("[BasicInfoSceneUIController] ResetUIPreload: 프리로드 UI 초기화 및 Snap 완료");
        }

        /// <summary>
        ///     프리로드 씬 입장 UI 연출을 시작합니다.
        /// </summary>
        public void PlayEnterUIPreload()
        {
            StartCoroutine(startSequence.PlaySequenceRoutine());
        }

        /// <summary>
        ///     프리로드 씬 퇴장 UI 연출을 시작합니다.
        /// </summary>
        public void PlayExitUIPreload()
        {
            StartCoroutine(PlayExitUIPreloadRoutine());
        }

        public IEnumerator PlayExitUIPreloadRoutine()
        {
            CommonResetUIOut();

            yield return StartCoroutine(endSequence.PlaySequenceRoutine());
        }

        #endregion
    }
}