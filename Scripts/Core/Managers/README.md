### Managers
> **Overview:** 프로젝트에서 공통적으로 사용되거나 Base가 되는 manager들

## BaseSceneManager
> **Overview:** 모든 씬에 공통적으로 들어가는 SceneManager의 Base Class입니다. Scene의 UI를 담당하는 컨트롤러를 가지며, 씬 입장, 퇴장, Preload 방식에서 슬라이딩일 통제합니다.

## GameSettingManager
> **Overview:**: 모든 씬에 공통적으로 호출 가능한 GameSetting Menu를 관리하는 Manager입니다. 모니터 해상도, 창모드, 화면 모드, 각 사운드 볼륨에 대한 기초적인 설정 정보를 관리하고, 가지고 있습니다.

## SceneSlideManager
> **Overview:**: Preload의 경우에 사용되는 씬 로딩 방식인 슬라이드 시스템을 관리합니다.
- 실제 림버스 컴퍼니가 어떻게 작동되나 유심히 관찰한 결과 나름대로 구현해보았습니다. 일단 기본적으로 가운데에 Main Scene이 존재하고, 동서남북 네 방위에 각 씬을 ***소환*** 합니다.
- 기본적인 동작은 각 씬을 이렇게 배치한 뒤, 활성화된 '씬 패널을 이동' 시키는 것입니다.
- 앞서 BaseSceneManager가 필요한 이유가 여기 있습니다. 이 Preload 방식의 경우 씬 진입이 애매한 점이 있습니다. 아무래도 모든 씬을 전부 로딩시켜두는 방식이라 그냥 Awake와 Start로 씬을 초기화 하는 것에는 무리가 있었기에, 각 씬을 초기화하고, 이벤트 방식으로 씬 진입 시 초기화 및 입장 및 연출 로직을 수행하게 진행했습니다.
- 처음 시작하면 Scene들은 비활성화 상태로 대기시킵니다. 그리고 어떤 씬으로 이동한다는 정보(`public async UniTask ExecuteSlideAsync(string targetSceneName)`)가 들어오면 해당 씬을 우선 활성화 해줍니다.
- 활성화하면 해당 타겟 씬의 OnEnable 이벤트 구독이 이루어질 수 있게 되어  `GlobalSceneEvents.OnSceneSlideStarted?.Invoke(previousSceneName, targetSceneName);` 코드를 통해 슬라이드 시작 이벤트를 발송하면 수신 가능합니다.
- 이 이벤트 구독 및 처리 순서에 신경을 많이 썼습니다. 그리고 당연히 이펙트 구현할때와 마찬가지로
```csharp
            switch (sceneSlideType)
            {
                case SceneSlideType.Normal:
                    await SlideRoutineAsync(targetSceneName);
                    break;
                case SceneSlideType.DoTween:
                    await SlideDoTweenRoutineAsync(targetSceneName);
                    break;
                default:
                    throw new NotImplementedException();
            }

```
- DOTWeen 여부를 여기서도 사용합니다.
- 추가로 게임에서도 그렇 듯, 약간 뒤로 당겼다가 튕기는 듯한 자연스러운 느낌을 주고 싶어서, InOutBack을 넣어서 활용했습니다.

```csharp
        // 뒤로 당겼다가 튕겨가는 자연스러운 수학 공식 (InOutBack) 개쩜
        private float EaseInOutBack(float x)
        {
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;

            return x < 0.5f
                ? Mathf.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2) / 2f
                : (Mathf.Pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2f;
        }
```

## SettingUIManger
> **Overview:** 앞서 구현한 Setting Manager가 정보를 가지고 처리하는 공간이었다면, 이건 Setting UI를 직접적으로 다루고 사용할 수 있는 매니저입니다. 해상도를 비롯해 여러 설정을 드롭박스 및 직접 입력으로 처리 가능하며, 소리 조절 또한 가능합니다.

<img width="800" height="486" alt="2026-05-1522-02-32-ezgif com-video-to-gif-converter" src="https://github.com/user-attachments/assets/b31930c0-46c5-4afb-89f1-e47828092de3" />

## SoundManager
> **Overview:** 만들때 제법 뿌듯했던 Manager입니다. 뭔가 소리를 전체적으로 관리하는게 있으면 좋겠다 싶어서 구현해보았습니다. 처음에는 단일 오디오 소스로 사용했는데 소리가 중간에 많이 겹치면 터지거나 꺼지는 현상을 해결하고자 오디오 소스를 여러개 두고, 라운드 로빈 방식으로 번갈아가며 사용하게 했습니다. BGM은 당연히 겹치면 안되므로 단일 소스를 사용했습니다.
- 제가 생각했을 때 필요한 기능은 전부 넣었습니다. BGM을 자연스럽게 교체하는 로직, Queue로 등록해서 여러개를 출력, 랜덤 출력, 각 효과음 출력등 신경을 많이 썼습니다.
