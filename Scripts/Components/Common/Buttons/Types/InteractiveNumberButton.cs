using System;
using Components.Common.Buttons.Core;
using Core.Data.Enums;
using Core.Managers;
using TMPro;
using UnityEngine;

namespace Components.Common.Buttons
{
    public class InteractiveNumberButton : MonoBehaviour
    {
        [Header("Number Data")]
        public int value;
        [HideInInspector] public int index = -1;
        private CompoundButton _compoundButton;

        private TextMeshProUGUI _currentText;

        private void Awake()
        {
            _currentText = GetComponentInChildren<TextMeshProUGUI>();
            _compoundButton = GetComponent<CompoundButton>();
        }

        public event Action<int, int> OnValueChanged;

        public void SetupIndex(int newIndex)
        {
            index = newIndex;
            UpdateNumberUI(value);
        }

        public void OnInteractiveNumberButtonClicked()
        {
            if (index == -1) return;

            value = (value + 1) % 10;

            UpdateNumberUI(value);

            var overrideConfig = _compoundButton.DefaultConfig.Clone();

            SetBloomGraphicSetting(overrideConfig);

            OnValueChanged?.Invoke(index, value);
        }

        private void SetBloomGraphicSetting(ButtonConfig overrideConfig)
        {
            if (overrideConfig.graphicSettings == null || overrideConfig.graphicSettings.Count == 0) return;

            var graphicConfig = overrideConfig.graphicSettings[0];
            graphicConfig.hoverState.useBloom = true;
            graphicConfig.hoverState.bloomScaleZ = 1;

            if (value == 1)
            {
                graphicConfig.hoverState.bloomScaleX = 0.5f;
                graphicConfig.hoverState.bloomScaleY = 0.8f;
            }
            else
            {
                graphicConfig.hoverState.bloomScaleX = 1;
                graphicConfig.hoverState.bloomScaleY = 1;
            }

            overrideConfig.graphicSettings[0] = graphicConfig;
            _compoundButton.SetProperty(overrideConfig);
            _compoundButton.PlayOverrideEffect();
        }

        public void SucEffect(int newValue)
        {
            value = newValue % 10;
            UpdateNumberUI(value);
            OnValueChanged?.Invoke(index, value);

            if (_compoundButton != null)
            {
                var overrideConfig = _compoundButton.DefaultConfig.Clone();

                if (overrideConfig.graphicSettings != null && overrideConfig.graphicSettings.Count > 0)
                {
                    var graphicConfig = overrideConfig.graphicSettings[0];

                    // Glow 설정 (색상 반드시 추가안하면 ㅈ됨)
                    graphicConfig.disabledState.useGlow = true;
                    graphicConfig.disabledState.glowPower = 0.5f;
                    // 인스펙터에 지정했던 원본 Glow 색상을 그대로 가져옴. (또는 Color.yellow 등 직접 지정 가능)
                    graphicConfig.disabledState.glowColor = graphicConfig.baseState.glowColor;

                    // Bloom 설정 (색상 반드시 추가안하면 ㅈ됨 진짜)
                    graphicConfig.disabledState.useBloom = true;
                    graphicConfig.disabledState.bloomAlpha = 0.5f;
                    graphicConfig.disabledState.bloomScaleX = 1f;
                    graphicConfig.disabledState.bloomScaleY = 1f;
                    graphicConfig.disabledState.bloomScaleZ = 1f;
                    // 인스펙터에 지정했던 원본 Bloom 색상을 그대로 가져옴
                    graphicConfig.disabledState.bloomColor = graphicConfig.baseState.bloomColor;

                    overrideConfig.graphicSettings[0] = graphicConfig;
                }

                // 3. 실행 순서 최적화 (셋팅 먼저 -> 비활성화 나중에)
                _compoundButton.SetProperty(overrideConfig);

                // 인터랙션을 끄는 순간, 내부적으로 상태가 Disabled로 변하면서 
                // 방금 넣은 overrideConfig를 읽어 자동으로 Play()가 실행
                _compoundButton.IsInteractable = false;
            }

            if (SoundManager.Instance != null) SoundManager.Instance.PlaySfx(SfxSoundType.Dding);
        }

        private void UpdateNumberUI(int num)
        {
            var numStr = num.ToString();

            if (_compoundButton != null)
                _compoundButton.SetText(numStr);
            else if (_currentText != null)
                // CompoundButton이 없을 때만 직접 변경
                _currentText.text = numStr;
        }
    }
}
