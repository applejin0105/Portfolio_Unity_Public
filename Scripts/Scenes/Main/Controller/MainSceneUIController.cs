using System.Collections;
using System.Collections.Generic;
using Components.Common.Buttons.Core;
using Components.Effects.UI.Core;
using Components.Effects.UI.Types;
using Core.Attributes;
using Core.Managers;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Scenes.Main.Controller
{
    public class MainSceneUIController : MonoBehaviour
    {
        [Header("UI Panels & Elements")]
        [SerializeField] private RectTransform mainPanel;
        [SerializeField] private SerializableDictionary<string, GameObject> sceneButtons;
        [SerializeField] private SerializableDictionary<string, bool> isTopButton;

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

        [Header("Ticket & Profile Card System")]
        [SerializeField] private bool isFront;
        [SerializeField] private CompoundButton ticketBackButtons;
        [SerializeField] private CompoundButton battleButton;

        [Header("Effects - Fade Canvas Group")]
        [SerializeField] private UIFadeCanvasGroupEffect uiSlidePanelFadeCanvasGroupEffect;
        [SerializeField] private UIFadeCanvasGroupEffect uiUserInfoFadeCanvasGroupEffect;
        [SerializeField] private UIFadeCanvasGroupEffect uiClickableDecoFadeCanvasGroupEffect;
        [SerializeField] private UIFadeCanvasGroupEffect uiProfileCardFrontFadeCanvasGroupEffect;
        [SerializeField] private UIFadeCanvasGroupEffect uiProfileCardBackFadeCanvasGroupEffect;

        [Header("Effects - Move")]
        [SerializeField] private UIMoveEffect uiProfileCardFrontMoveEffect;
        [SerializeField] private Vector3 uiProfileCardFrontPosStart;
        [SerializeField] private Vector3 uiProfileCardFrontPosMid;
        [SerializeField] private Vector3 uiProfileCardFrontPosEnd;

        [SerializeField] private UIMoveEffect uiUserInfoMoveEffect;
        [SerializeField] private Vector3 uiUserInfoPosStart;
        [SerializeField] private Vector3 uiUserInfoPosEnd;

        [SerializeField] private UIMoveEffect uiButtonMoveEffect;
        [SerializeField] private Vector3 uiButtonPosStart;
        [SerializeField] private Vector3 uiButtonPosEnd;

        [SerializeField] private UIMoveEffect uiProfileCardBackMoveEffect;
        [SerializeField] private Vector3 uiProfileCardBackPosStart;
        [SerializeField] private float profileCardFrontRot;
        [SerializeField] private float topButtonReleasedY = -141.5f;
        [SerializeField] private float topButtonPressedY = -150.5f;
        [SerializeField] private float bottomButtonReleasedY = -307.5f;
        [SerializeField] private float bottomButtonPressedY = -316.5f;

        [Header("Effects - Miscellaneous")]
        [SerializeField] private List<UIEffect> uiEffects;

        private bool _isExiting;
        private bool _isTicketChanging;

        [Header("Effects - Button Positioning & Animation")]
        private EffectSequence _pressButtonSequence;
        private EffectSequence _releaseButtonSequence;
        private MoveConfig _uiButtonMoveConfig;
        private FadeCanvasGroupConfig _uiClickableDecoFadeCanvasGroupConfig;
        private FadeCanvasGroupConfig _uiProfileCardBackFadeCanvasGroupConfig;
        private MoveConfig _uiProfileCardBackMoveConfig;
        private FadeCanvasGroupConfig _uiProfileCardFrontFadeCanvasGroupConfig;

        [Header("Config - Move")]
        private MoveConfig _uiProfileCardFrontMoveConfig;

        [Header("Config - FadeCanvasGroup")]
        private FadeCanvasGroupConfig _uiSlidePanelFadeCanvasGroupConfig;
        private FadeCanvasGroupConfig _uiUserInfoFadeCanvasGroupConfig;
        private MoveConfig _uiUserInfoMoveConfig;

        [Header("Copy User ID")]
        [SerializeField] private TextMeshProUGUI userId;
        [SerializeField] private CompoundButton copyButton;

        private void Start()
        {
            isFront = true;

            if (uiEffects is not { Count: > 0 }) return;

            foreach (var effect in uiEffects) effect.Play();
        }

        private void OnEnable()
        {
            foreach (var key in sceneButtons.Keys)
            {
                var button = sceneButtons[key];
                var isTop = isTopButton[key];

                var compoundButton = button.GetComponent<CompoundButton>();

                compoundButton.onPointerDownEvent.AddListener(() => GraphicPress(key, isTop));
                compoundButton.onPointerUpEvent.AddListener(() => GraphicRelease(key, isTop));
            }

            if (battleButton != null)
                battleButton.onPointerDownEvent.AddListener(TicketBackButtonOnClick);
        }

        private void OnDisable()
        {
            foreach (var key in sceneButtons.Keys)
            {
                var button = sceneButtons[key];
                var isTop = isTopButton[key];

                var compoundButton = button.GetComponent<CompoundButton>();

                compoundButton.onPointerDownEvent.RemoveListener(() => GraphicPress(key, isTop));
                compoundButton.onPointerUpEvent.RemoveListener(() => GraphicRelease(key, isTop));
            }

            if (battleButton != null)
                battleButton.onClickEvent.RemoveListener(TicketBackButtonOnClick);
        }

        private bool IsDirectSceneLoad()
        {
            return SceneManager.sceneCount == 1;
        }

        private void CommonReset(bool isIn)
        {
            _uiUserInfoFadeCanvasGroupConfig = uiUserInfoFadeCanvasGroupEffect.DefaultConfig;
            _uiClickableDecoFadeCanvasGroupConfig = uiClickableDecoFadeCanvasGroupEffect.DefaultConfig;

            _uiProfileCardFrontMoveConfig = uiProfileCardFrontMoveEffect.DefaultConfig;
            _uiUserInfoMoveConfig = uiUserInfoMoveEffect.DefaultConfig;
            _uiButtonMoveConfig = uiButtonMoveEffect.DefaultConfig;

            if (isIn)
            {
                _uiUserInfoFadeCanvasGroupConfig.endAlpha = 1.0f;
                _uiClickableDecoFadeCanvasGroupConfig.endAlpha = 1.0f;

                _uiProfileCardFrontMoveConfig.position.start = uiProfileCardFrontPosStart;
                _uiProfileCardFrontMoveConfig.position.end = uiProfileCardFrontPosMid;

                _uiUserInfoMoveConfig.position.start = uiUserInfoPosStart;
                _uiUserInfoMoveConfig.position.end = uiUserInfoPosEnd;

                _uiButtonMoveConfig.position.start = uiButtonPosStart;
                _uiButtonMoveConfig.position.end = uiButtonPosEnd;
            }
            else
            {
                _uiUserInfoFadeCanvasGroupConfig.endAlpha = 0.0f;
                _uiClickableDecoFadeCanvasGroupConfig.endAlpha = 0.0f;

                _uiProfileCardFrontMoveConfig.position.start = uiProfileCardFrontPosMid;
                _uiProfileCardFrontMoveConfig.position.end = uiProfileCardFrontPosStart;

                _uiUserInfoMoveConfig.position.start = uiUserInfoPosEnd;
                _uiUserInfoMoveConfig.position.end = uiUserInfoPosStart;

                _uiButtonMoveConfig.position.start = uiButtonPosEnd;
                _uiButtonMoveConfig.position.end = uiButtonPosStart;
            }

            uiUserInfoFadeCanvasGroupEffect.SetProperty(_uiUserInfoFadeCanvasGroupConfig);
            uiClickableDecoFadeCanvasGroupEffect.SetProperty(_uiClickableDecoFadeCanvasGroupConfig);

            uiProfileCardFrontMoveEffect.SetProperty(_uiProfileCardFrontMoveConfig);
            uiUserInfoMoveEffect.SetProperty(_uiUserInfoMoveConfig);
            uiButtonMoveEffect.SetProperty(_uiButtonMoveConfig);
        }

        private void CommonResetUIIn()
        {
            CommonReset(true);
        }

        private void CommonResetUIOut()
        {
            CommonReset(false);
        }

        private IEnumerator ExecuteTransitionRoutine(string targetSceneName)
        {
            yield return new WaitForSeconds(0.15f);

            if (GameSettingManager.GetDeviceMode() || targetSceneName == "08_Battle")
            {
                yield return StartCoroutine(PlayExitUISingleLoad());
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                yield return StartCoroutine(PlayExitUIPreloadRoutine());
                SceneSlideManager.Instance.ExecuteSlideAsync(targetSceneName).Forget();
            }
        }

        private IEnumerator EnableHoverAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
        }

        private void SetAbsoluteMoveConfig(GameObject targetObject, float targetY)
        {
            var moveEffect = targetObject.GetComponent<UIMoveEffect>();
            var moveConfig = moveEffect.DefaultConfig;

            moveConfig.position.start.y = targetObject.GetComponent<RectTransform>().anchoredPosition.y;
            moveConfig.position.end.y = targetY;

            moveEffect.SetProperty(moveConfig);
        }

        private void GraphicPress(string targetSceneName, bool isTop = true)
        {
            if (_isExiting) return;

            var targetButton = sceneButtons[targetSceneName];

            SetAbsoluteMoveConfig(targetButton, isTop ? topButtonPressedY : bottomButtonPressedY);
            targetButton.GetComponent<UIMoveEffect>().PlayOverrideEffect();
        }

        private void GraphicRelease(string targetSceneName, bool isTop = true)
        {
            if (_isExiting) return;
            _isExiting = true;

            var targetButton = sceneButtons[targetSceneName];

            SetAbsoluteMoveConfig(targetButton, isTop ? topButtonReleasedY : bottomButtonReleasedY);
            targetButton.GetComponent<UIMoveEffect>().PlayOverrideEffect();

            StartCoroutine(ExecuteTransitionRoutine(targetSceneName));
        }

        private void TicketChangeEffect(bool front)
        {
            Vector3 frontPosStart, frontPosEnd;
            Vector3 backPosStart, backPosEnd;
            float rot, frontEndAlpha, backEndAlpha, duration;
            bool frontBlockRay, backBlockRay;

            duration = 0.3f;

            _uiProfileCardFrontMoveConfig = uiProfileCardFrontMoveEffect.DefaultConfig;

            if (front)
            {
                frontPosStart = uiProfileCardFrontMoveEffect.GetComponent<RectTransform>().anchoredPosition;
                frontPosEnd = uiProfileCardFrontPosEnd;
                rot = profileCardFrontRot;

                backPosStart = uiProfileCardBackMoveEffect.GetComponent<RectTransform>().anchoredPosition;
                backPosEnd = uiProfileCardFrontPosMid;

                frontEndAlpha = 0.3f;
                backEndAlpha = 1.0f;
                frontBlockRay = false;
                backBlockRay = true;
            }
            else
            {
                frontPosStart = uiProfileCardFrontMoveEffect.GetComponent<RectTransform>().anchoredPosition;
                frontPosEnd = uiProfileCardFrontPosMid;
                rot = 0;

                backPosStart = uiProfileCardBackMoveEffect.GetComponent<RectTransform>().anchoredPosition;
                backPosEnd = uiProfileCardBackPosStart;

                frontEndAlpha = 1.0f;
                backEndAlpha = 0.0f;
                frontBlockRay = true;
                backBlockRay = false;
            }

            _uiProfileCardFrontMoveConfig.position.start = frontPosStart;
            _uiProfileCardFrontMoveConfig.position.end = frontPosEnd;

            _uiProfileCardFrontMoveConfig.rotation.start.z =
                uiProfileCardFrontMoveEffect.GetComponent<RectTransform>().localEulerAngles.z;
            _uiProfileCardFrontMoveConfig.rotation.end.z = rot;

            _uiProfileCardBackMoveConfig = uiProfileCardBackMoveEffect.DefaultConfig;
            _uiProfileCardBackMoveConfig.position.start = backPosStart;
            _uiProfileCardBackMoveConfig.position.end = backPosEnd;

            _uiProfileCardFrontFadeCanvasGroupConfig = uiProfileCardFrontFadeCanvasGroupEffect.DefaultConfig;
            _uiProfileCardFrontFadeCanvasGroupConfig.endAlpha = frontEndAlpha;
            _uiProfileCardFrontFadeCanvasGroupConfig.blockRaycasts = frontBlockRay;

            _uiProfileCardBackFadeCanvasGroupConfig = uiProfileCardBackFadeCanvasGroupEffect.DefaultConfig;
            _uiProfileCardBackFadeCanvasGroupConfig.endAlpha = backEndAlpha;
            _uiProfileCardBackFadeCanvasGroupConfig.blockRaycasts = backBlockRay;

            uiProfileCardFrontFadeCanvasGroupEffect.SetProperty(_uiProfileCardFrontFadeCanvasGroupConfig, duration);
            uiProfileCardBackFadeCanvasGroupEffect.SetProperty(_uiProfileCardBackFadeCanvasGroupConfig, duration);

            uiProfileCardFrontMoveEffect.SetProperty(_uiProfileCardFrontMoveConfig, duration);
            uiProfileCardBackMoveEffect.SetProperty(_uiProfileCardBackMoveConfig, duration);
        }

        public void TicketChangeButtonOnClick()
        {
            if (_isExiting) return;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);

            StartCoroutine(TicketChangeRoutine());
        }

        private IEnumerator TicketChangeRoutine()
        {
            _isTicketChanging = true;

            var frontRect = uiProfileCardFrontMoveEffect.GetComponent<RectTransform>();

            if (isFront)
            {
                var lerpDuration = 0.15f;
                var elapsedTime = 0f;
                var startRotation = frontRect.localRotation;
                var targetRotation = Quaternion.identity;

                while (elapsedTime < lerpDuration)
                {
                    frontRect.localRotation =
                        Quaternion.Slerp(startRotation, targetRotation, elapsedTime / lerpDuration);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                frontRect.localRotation = targetRotation;
            }

            TicketChangeEffect(isFront);
            isFront = !isFront;

            uiProfileCardFrontFadeCanvasGroupEffect.PlayOverrideEffect();
            uiProfileCardFrontMoveEffect.PlayOverrideEffect();
            uiProfileCardBackFadeCanvasGroupEffect.PlayOverrideEffect();
            uiProfileCardBackMoveEffect.PlayOverrideEffect();

            _isTicketChanging = false;
        }

        public void TicketBackButtonOnClick()
        {
            if (_isExiting) return;
            _isExiting = true;

            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);

            StartCoroutine(ExecuteTransitionRoutine("08_Battle"));
        }

        public void PressCopyButtonOnClick()
        {
            GUIUtility.systemCopyBuffer = userId.text;
        }

        #region Single Load UI

        public void ResetUISingleLoad()
        {
            _isExiting = false;
            isFront = true;
            _isTicketChanging = false;

            fadeInSequence?.StopAll();
            fadeOutSequence?.StopAll();

            _uiSlidePanelFadeCanvasGroupConfig = uiSlidePanelFadeCanvasGroupEffect.DefaultConfig;
            uiSlidePanelFadeCanvasGroupEffect.GetComponent<CanvasGroup>().alpha = 0f;
            uiSlidePanelFadeCanvasGroupEffect.SetProperty(_uiSlidePanelFadeCanvasGroupConfig, fadeInDuration);

            CommonResetUIIn();

            Debug.Log("[MainSceneUIController] ResetUISingleLoad: 단일 로드 UI 초기화 완료");
        }

        public void PlayEnterUISingleLoad()
        {
            _isExiting = false;
            uiSlidePanelFadeCanvasGroupEffect.GetComponent<CanvasGroup>().alpha = 0;
            _uiSlidePanelFadeCanvasGroupConfig.endAlpha = 1.0f;
            uiSlidePanelFadeCanvasGroupEffect.SetProperty(_uiSlidePanelFadeCanvasGroupConfig, fadeOutDuration);

            StartCoroutine(fadeInSequence.PlaySequenceRoutine());
            StartCoroutine(startSequence.PlaySequenceRoutine());
            StartCoroutine(EnableHoverAfterDelay(fadeInDuration));
        }

        public IEnumerator PlayExitUISingleLoad()
        {
            if (_isExiting) yield break;
            _isExiting = true;

            // 뒷면일 경우 앞면으로 되돌리고 대기
            if (!isFront)
            {
                StartCoroutine(TicketChangeRoutine());
                yield return new WaitForSeconds(0.35f);
            }

            CommonResetUIOut();

            uiSlidePanelFadeCanvasGroupEffect.GetComponent<CanvasGroup>().alpha = 1.0f;
            _uiSlidePanelFadeCanvasGroupConfig.endAlpha = 0.0f;
            uiSlidePanelFadeCanvasGroupEffect.SetProperty(_uiSlidePanelFadeCanvasGroupConfig, fadeInDuration);

            StartCoroutine(endSequence.PlaySequenceRoutine());
            yield return StartCoroutine(fadeOutSequence.PlaySequenceRoutine());
        }

        #endregion

        #region Preload UI

        public void ResetUIPreload()
        {
            _isExiting = false;
            isFront = true;
            _isTicketChanging = false;

            startSequence?.StopAll();
            endSequence?.StopAll();

            if (uiSlidePanelFadeCanvasGroupEffect != null)
                uiSlidePanelFadeCanvasGroupEffect.GetComponent<CanvasGroup>().alpha = 1.0f;

            if (uiUserInfoFadeCanvasGroupEffect != null)
                uiUserInfoFadeCanvasGroupEffect.GetComponent<CanvasGroup>().alpha = 0f;

            if (uiClickableDecoFadeCanvasGroupEffect != null)
                uiClickableDecoFadeCanvasGroupEffect.GetComponent<CanvasGroup>().alpha = 0f;

            // 카드의 물리적 위치와 투명도를 초기 상태로 강제 Snap (튕김 방지)
            if (uiProfileCardFrontFadeCanvasGroupEffect != null)
                uiProfileCardFrontFadeCanvasGroupEffect.GetComponent<CanvasGroup>().alpha = 1f;

            if (uiProfileCardBackFadeCanvasGroupEffect != null)
                uiProfileCardBackFadeCanvasGroupEffect.GetComponent<CanvasGroup>().alpha = 0f;

            if (uiProfileCardFrontMoveEffect != null)
            {
                uiProfileCardFrontMoveEffect.GetComponent<RectTransform>().anchoredPosition =
                    uiProfileCardFrontPosStart;
                uiProfileCardFrontMoveEffect.GetComponent<RectTransform>().localEulerAngles = Vector3.zero;
            }

            if (uiProfileCardBackMoveEffect != null)
            {
                uiProfileCardBackMoveEffect.GetComponent<RectTransform>().anchoredPosition = uiProfileCardBackPosStart;
                uiProfileCardBackMoveEffect.GetComponent<RectTransform>().localEulerAngles = Vector3.zero;
            }

            if (uiUserInfoMoveEffect != null)
                uiUserInfoMoveEffect.GetComponent<RectTransform>().anchoredPosition = uiUserInfoPosStart;

            if (uiButtonMoveEffect != null)
                uiButtonMoveEffect.GetComponent<RectTransform>().anchoredPosition = uiButtonPosStart;

            CommonResetUIIn();

            Debug.Log("[MainSceneUIController] ResetUIPreload: 프리로드 UI 초기화 및 물리적 Snap 완료");
        }

        public void PlayEnterUIPreload()
        {
            _isExiting = false;
            CommonResetUIIn();

            StartCoroutine(startSequence.PlaySequenceRoutine());
        }

        public void PlayExitUIPreload()
        {
            if (_isExiting) return;
            StartCoroutine(PlayExitUIPreloadRoutine());
        }

        public IEnumerator PlayExitUIPreloadRoutine()
        {
            if (_isExiting) yield break;
            _isExiting = true;

            // 뒷면이 켜져있다면 앞면으로 먼저 뒤집고 대기
            if (!isFront)
            {
                StartCoroutine(TicketChangeRoutine());
                yield return new WaitForSeconds(0.35f);
            }

            CommonResetUIOut();

            if (endSequence != null && endSequence.steps.Count > 0)
                yield return StartCoroutine(endSequence.PlaySequenceRoutine());
        }

        #endregion
    }
}