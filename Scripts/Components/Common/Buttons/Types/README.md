## ClickableDeco.cs
> 클릭 가능한 데코용 버튼입니다. PlayEffect시 사진과 이펙트 여부를 통해 사진이 교체되거나 이펙트가 작동하거나 둘 다 작동하는 방식으로 구현한 장식용 이펙트입니다.
<img width="302" height="314" alt="ClickableDeco" src="https://github.com/user-attachments/assets/40a13393-e171-4e6e-b70f-eced733d43f0" />

```csharp
        private void PlayEffectType(bool useSprite, bool useEffect)
        {
            switch (useSprite)
            {
                case true when useEffect:
                    StartCoroutine(PlaySimultaneous(decoConfig.useRandomSprite, decoConfig.useRandomEffect));
                    break;
                case true:
                    StartCoroutine(ChangeSprite(decoConfig.useRandomSprite));
                    break;
                default:
                {
                    if (useEffect)
                        StartCoroutine(PlayEffect(decoConfig.useRandomEffect));
                    break;
                }
            }
        }
```

## InteractiveNumberButton.cs
> 상호작용 가능한 숫자 버튼입니다. 버튼을 누르면, 버튼이 변경되고, 이에 맞게 숫자가 갱신됩니다. 각각의 숫자에 맞는 Bloom 효과도 구현되어 있으며 비밀번호를 맞췄으면 띵띵띵 소리가 나면서 멋진 연출도 구현했습니다. (이건 좀 멋짐)
<img width="800" height="438" alt="Interact01" src="https://github.com/user-attachments/assets/17750a95-6699-434f-a47e-4aea9ab53e50" />

<img width="800" height="431" alt="Interact02" src="https://github.com/user-attachments/assets/cee772e8-b45d-46d8-9f40-5af7454bbce7" />

```csharp
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
```
내부 버튼들의 번호를 갱신하고, IntroSceneManager와 연동하여 데이터를 공유합니다.
Bloom도 숫자에 맞게 이쁘게 표시되게 하였습니다. (진짜 Bloom이 아닌 Fake Bloom을 사용하기 때문에 배경 Bloom 이미지를 작게 만들어야 했습니다.)
