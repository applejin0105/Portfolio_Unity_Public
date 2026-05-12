using System.Collections;
using System.Collections.Generic;
using Components.Common.Buttons.Core;
using Components.Effects.UI;
using Components.Effects.UI.Core;
using Components.Effects.UI.Types;
using Core.Interfaces;
using Core.Managers;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scenes.RoadMaps.Controller
{
    public class RoadMapsSceneUIController : MonoBehaviour, ISceneUIController
    {
        [Header("UI Panels & Elements")]
        [SerializeField] private RectTransform mainPanel;
        [SerializeField] private CompoundButton backBtn;
        [SerializeField] private Image fillImage;
        [SerializeField] private Image fillBgImage;

        [Tooltip("씬 진입 시 재생될 공통 연출")]
        [SerializeField] private EffectSequence startSequence;
        [Tooltip("씬 퇴장 시 재생될 공통 연출")]
        [SerializeField] private EffectSequence endSequence;

        [Header("Effects")]
        [SerializeField] private UIFillEffect uiFillEffect;
        [SerializeField] private UIFillEffect uiFillEffectBg;
        [SerializeField] private UIMoveEffect uiMoveEffectLeftDeco;
        [SerializeField] private UIMoveEffect uiMoveEffectRightDeco;
        [SerializeField] private UIFadeEffect uiFadeEffectLeftDeco;
        [SerializeField] private UIFadeEffect uiFadeEffectRightDeco;
        [SerializeField] private UIFadeEffect backBtnFadeEffect;

        [SerializeField] private Vector3 uiMoveDecoPosStart;
        [SerializeField] private Vector3 uiMoveDecoPosEnd;

        [Header("Types")]
        [SerializeField]
        private List<CompoundButton> types = new();

        [SerializeField] private float duration = 1.0f;
        private FadeConfig _fadeConfig;

        private FillConfig _fillConfig;
        private int _index;
        private MoveConfig _moveConfig;
        private TypewriterConfig _typewriterConfig;

        private void OnEnable()
        {
            if (backBtn != null)
                backBtn.onClickEvent.AddListener(BackButtonOnClick);
        }

        private void OnDisable()
        {
            if (backBtn != null)
                backBtn.onClickEvent.RemoveListener(BackButtonOnClick);
        }

        private void ResetIn()
        {
            fillImage.fillAmount = 0;
            fillBgImage.fillAmount = 0;
            _fillConfig = uiFillEffect.DefaultConfig;
            _fillConfig.startFillAmount = 0;
            _fillConfig.endFillAmount = 1;

            uiFillEffectBg.SetProperty(_fillConfig, duration);
            uiFillEffect.SetProperty(_fillConfig, duration);

            _moveConfig = uiMoveEffectLeftDeco.DefaultConfig;
            _moveConfig.position.start = uiMoveDecoPosStart;
            _moveConfig.position.end = uiMoveDecoPosEnd;

            uiMoveEffectLeftDeco.SetProperty(_moveConfig, duration);
            uiMoveEffectRightDeco.SetProperty(_moveConfig, duration);


            _fadeConfig = uiFadeEffectLeftDeco.DefaultConfig;
            _fadeConfig.endAlpha = 1.0f;
            uiFadeEffectLeftDeco.SetProperty(_fadeConfig, duration);
            uiFadeEffectRightDeco.SetProperty(_fadeConfig, duration);

            foreach (var type in types)
            {
                var typewriterEffect = type.GetComponent<UITypewriterEffect>();
                typewriterEffect.ResetToHiddenState();
                type.IsInteractable = false;
            }

            backBtn.GetComponent<Image>().color = new Color(255, 255, 255, 0);
            _fadeConfig = backBtnFadeEffect.DefaultConfig;
            backBtn.IsInteractable = true;
            _fadeConfig.endAlpha = 1.0f;
            backBtnFadeEffect.SetProperty(_fadeConfig, duration);
        }

        private void ResetOut()
        {
            fillImage.fillAmount = 1;
            fillBgImage.fillAmount = 1;
            _fillConfig = uiFillEffect.DefaultConfig;
            _fillConfig.startFillAmount = 1;
            _fillConfig.endFillAmount = 0;

            uiFillEffectBg.SetProperty(_fillConfig, duration);
            uiFillEffect.SetProperty(_fillConfig, duration);

            _moveConfig = uiMoveEffectLeftDeco.DefaultConfig;
            _moveConfig.position.start = uiMoveDecoPosEnd;
            _moveConfig.position.end = uiMoveDecoPosStart;

            uiMoveEffectLeftDeco.SetProperty(_moveConfig, duration);
            uiMoveEffectRightDeco.SetProperty(_moveConfig, duration);

            _fadeConfig = uiFadeEffectLeftDeco.DefaultConfig;
            _fadeConfig.endAlpha = 0.0f;
            uiFadeEffectLeftDeco.SetProperty(_fadeConfig, duration);
            uiFadeEffectRightDeco.SetProperty(_fadeConfig, duration);

            _fadeConfig = backBtnFadeEffect.DefaultConfig;
            backBtn.IsInteractable = false;
            _fadeConfig.endAlpha = 0.0f;
            backBtnFadeEffect.SetProperty(_fadeConfig, duration);
        }

        private void OnTypeWriteTypes()
        {
            foreach (var type in types) type.IsInteractable = true;
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

        private void CloseButtonOnClick()
        {
            if (_index >= types.Count) return;

            var typewriter = types[_index].GetComponent<UITypewriterEffect>();
            if (typewriter != null) typewriter.PlayOverrideEffect();

            _index++;
        }

        #region Single Load

        public void ResetUISingleLoad()
        {
            ResetIn();
            Debug.Log("[RoadMapsSceneUIController] ResetUISingleLoad: 단일 로드 UI 초기화 완료");
        }

        public void PlayEnterUISingleLoad()
        {
            StartCoroutine(startSequence.PlaySequenceRoutine());
            OnTypeWriteTypes();
        }

        public IEnumerator PlayExitUISingleLoad()
        {
            ResetOut();
            yield return StartCoroutine(endSequence.PlaySequenceRoutine());
        }

        #endregion

        #region Preload

        public void ResetUIPreload()
        {
            ResetIn();
            Debug.Log("[ProjectsUIController] ResetUIPreload: 프리로드 UI 초기화 완료");
        }

        public void PlayEnterUIPreload()
        {
            StartCoroutine(startSequence.PlaySequenceRoutine());
            OnTypeWriteTypes();
        }

        public void PlayExitUIPreload()
        {
            StartCoroutine(PlayExitUIPreloadRoutine());
        }

        public IEnumerator PlayExitUIPreloadRoutine()
        {
            ResetOut();
            yield return StartCoroutine(endSequence.PlaySequenceRoutine());
        }

        #endregion
    }
}