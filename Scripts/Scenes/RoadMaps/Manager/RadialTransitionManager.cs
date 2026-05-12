using System.Collections;
using Components.Common.Buttons.Core;
using Components.Effects.UI;
using Components.Effects.UI.Types;
using Core.Extensions;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Scenes.RoadMaps.Manager
{
    public class RadialTransitionManager : MonoBehaviour
    {
        [Header("Effect Component")]
        [SerializeField] private UIRadialEffect radialEffect;

        [Header("Managers")]
        [SerializeField] private CategoryUIManager categoryUIManager;

        [Header("UI Elements (버튼)")]
        [SerializeField] private CompoundButton[] actionButtons;
        [SerializeField] private CompoundButton backButton;
        [SerializeField] private UIFadeEffect backButtonFadeEffect;

        [Header("Scrollbar Control")]
        [SerializeField] private CanvasGroup scrollbarCanvasGroup;
        [SerializeField] private float scrollbarFadeDuration = 0.3f;
        private readonly Color _hiddenTextColor = "#8C8C65".ToColor(Color.gray);

        private readonly Color _selectedTextColor = "#FFFFA1".ToColor(Color.white);

        private int _activeIndex = -1;
        private FadeConfig _backButtonFadeConfig;
        private Coroutine _transitionCoroutine;

        private void Start()
        {
            SetScrollbarVisible(false, 0f);
        }

        public void OnElementClicked(int index, RectTransform clickedRect)
        {
            // 실행 중인 트랜지션 강제 중단
            if (_transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
                _transitionCoroutine = null;
            }

            if (_activeIndex == index)
                // 동일 항목 클릭 시 축소 전환
                _transitionCoroutine = StartCoroutine(ShrinkSequence());
            else
                // 새 항목 클릭 시 기존 항목 축소 후 새 항목 확장
                _transitionCoroutine = StartCoroutine(SwitchSequence(index, clickedRect));
        }

        private IEnumerator SwitchSequence(int newIndex, RectTransform newRect)
        {
            // 활성화된 항목이 있다면 중단 및 축소 실행
            if (_activeIndex != -1)
            {
                SetScrollbarVisible(false, 0f);

                // centerRect를 null로 주어 기존 중심점에서 축소되도록 유도
                var shrinkConfig = new RadialConfig { centerRect = null, isExpand = false, useSound = false };
                radialEffect.SetProperty(shrinkConfig, radialEffect.Duration * 0.5f);

                yield return radialEffect.PlayWaitableEffect();
                categoryUIManager.ChangeContent(-1);
            }

            // 새로운 항목 확장 실행
            _activeIndex = newIndex;
            UpdateButtonConfigs(newIndex, radialEffect.Duration);
            categoryUIManager.ChangeContent(newIndex);

            var expandConfig = new RadialConfig { centerRect = newRect, isExpand = true, useSound = false };
            radialEffect.SetProperty(expandConfig, radialEffect.Duration);

            _backButtonFadeConfig = backButtonFadeEffect.DefaultConfig;
            _backButtonFadeConfig.endAlpha = 0.0f;
            backButtonFadeEffect.SetProperty(_backButtonFadeConfig);

            backButton.IsInteractable = false;
            backButtonFadeEffect.PlayOverrideEffect();

            yield return radialEffect.PlayWaitableEffect();

            SetScrollbarVisible(true, scrollbarFadeDuration);
            _transitionCoroutine = null;
        }

        private IEnumerator ShrinkSequence()
        {
            SetScrollbarVisible(false, 0f);
            UpdateButtonConfigs(-1, radialEffect.Duration);

            var shrinkConfig = new RadialConfig { centerRect = null, isExpand = false, useSound = false };
            radialEffect.SetProperty(shrinkConfig, radialEffect.Duration * 0.5f);

            _backButtonFadeConfig = backButtonFadeEffect.DefaultConfig;
            _backButtonFadeConfig.endAlpha = 1.0f;
            backButtonFadeEffect.SetProperty(_backButtonFadeConfig);

            backButton.IsInteractable = true;
            backButtonFadeEffect.PlayOverrideEffect();

            yield return radialEffect.PlayWaitableEffect();

            _activeIndex = -1;
            categoryUIManager.ChangeContent(-1);
            _transitionCoroutine = null;
        }

        private void SetScrollbarVisible(bool isVisible, float duration)
        {
            if (scrollbarCanvasGroup == null) return;

            scrollbarCanvasGroup.DOKill();

            if (isVisible)
            {
                scrollbarCanvasGroup.DOFade(1f, duration).OnComplete(() =>
                {
                    scrollbarCanvasGroup.interactable = true;
                    scrollbarCanvasGroup.blocksRaycasts = true;
                });
            }
            else
            {
                scrollbarCanvasGroup.interactable = false;
                scrollbarCanvasGroup.blocksRaycasts = false;
                scrollbarCanvasGroup.alpha = 0f;
            }
        }

        private void UpdateButtonConfigs(int selectedIndex, float animTime)
        {
            if (actionButtons == null) return;

            for (var i = 0; i < actionButtons.Length; i++)
            {
                var btn = actionButtons[i];
                if (btn == null) continue;

                if (selectedIndex == -1)
                {
                    var tempConfig = btn.DefaultConfig.Clone();
                    tempConfig.animDuration = animTime;
                    btn.SetProperty(tempConfig);
                    btn.PlayOverrideEffect();

                    DOVirtual.DelayedCall(animTime, () =>
                    {
                        if (btn != null) btn.ClearProperty();
                    }).SetLink(btn.gameObject);
                }
                else
                {
                    var targetColor = i == selectedIndex ? _selectedTextColor : _hiddenTextColor;

                    var newConfig = btn.DefaultConfig.Clone();
                    newConfig.animDuration = animTime;

                    for (var j = 0; j < newConfig.graphicSettings.Count; j++)
                    {
                        var gConfig = newConfig.graphicSettings[j];

                        if (gConfig.target is TextMeshProUGUI)
                        {
                            gConfig.baseState.useColor = true;
                            gConfig.baseState.color = targetColor;
                            gConfig.hoverState.useColor = true;
                            gConfig.hoverState.color = targetColor;
                            gConfig.pressedState.useColor = true;
                            gConfig.pressedState.color = targetColor;
                            gConfig.disabledState.useColor = true;
                            gConfig.disabledState.color = targetColor;

                            if (gConfig.useAlways)
                            {
                                gConfig.alwaysState.useColor = true;
                                gConfig.alwaysState.color = targetColor;
                            }

                            newConfig.graphicSettings[j] = gConfig;
                        }
                    }

                    btn.SetProperty(newConfig);
                    btn.PlayOverrideEffect();
                }
            }
        }
    }
}