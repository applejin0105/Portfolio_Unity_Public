### Loading Scene
> **Overview:** 프로젝트의 모든 로딩을 처리합니다. 이 과정에서 기기의 사양을 점검하여 Preload인지 SingleLoad인지 구분합니다.

<img width="800" height="450" alt="Loading" src="https://github.com/user-attachments/assets/1a6476b4-d97a-41eb-85a4-4ca81642cadd" />

- `LoadingSceneManager`: 기기의 가용 RAM 용량에 따라 단일 씬 로딩(저사양)과 다중 씬 사전 로딩(고사양)을 동적으로 분기하여 처리하여, 이를 시각적인 로딩 UI 애니메이션을 함께 관리합니다.
- `LightningManager`: 로딩 씬 뒤에 출력되는 번개가 치는 프리팹들을 일괄적으로 관리합니다.
- `loadingUIController`: 로딩씬의 페이드인 및 페이드 아웃을 관리합니다.
- `LightningController`: 로딩씬 뒤에 출력되는 변개의 실질적인 재생과 출력을 관리합니다.

## LoadingSceneManager
> Preload/SingleLoad, Progressbar

- `ProcessLightQueue()`: 로딩 진행률에 맞춰 UI 노드를 순차적으로 켜고있습니다. 이때 Fill Effect를 사용하여 차오르는 느낌을 구현하였습니다. 기본적으로 실제 로딩 진행률(`_targetLitCount`)이 UI 애니메이션 상태(`_currentLitCount`)보다 앞서가면, 그 격차(`backlog`)를 계산하여 애니메이션 속도를 동적으로 조절(`dynamicDuration`)합니다. 그렇게 모든 노드가 켜지면 `_isUIAnimationComplete`을 true로 변경합니다.
- 이렇게 로딩이 진행되기 전, 사용자의 기기 메모리 상태를 계산합니다. 그리고 6000MB 미만일 경우 저사양 SingleLoad를 수행하고, 그렇지 않으면 고사양 PreLoad 방식을 진행합니다.
- 싱글모드로 로딩된 경우, 로딩이 90%될 때 까지 진행률을 UI에 반영합니다. 로딩이 완료되고 UI 애니메이션도 끝날 때까지 대기합니다. **로딩 화면 퇴장 이펙트를 재생한 후 씬 전환을 허용`allowSceneActivation = true`**합니다.
- 프리로딩 방식의 경우, `preloadSceneNames`에 정의된 모든 씬을 `LoadSceneMode.Additive`로 동시에 비동기 로딩합니다. 이때 **모든 씬의 로딩 진행률의 평균을 계산해서 `UpdateUI`로 넘깁니다.**
- 로딩이 완료되면 각 씬을 활성화하는데, ***이 부분이 아주 짜증나고 어려웠던 부분입니다.*** 계속해서 화면이 번쩍 거리고 겹쳐지면서 로딩 화면을 아주 더럽게 만들었습니다. 몇번씩이나 계속해서 이런 현상이 발생해서 다음 꼼수를 부렸습니다. 바로 **씬의 최상단 UI 패널을 카메라 밖의 아주 먼 좌표(`new Vector2(99999, 99999)`)로 즉시 이동시킵니다. 이후 프레임을 양보(`yield return null`)하여 무거운 Awake/Start 연산 부하를 분산했습니다.** 이 과정을 해결하자, 프로파일러를 통해 그래프가 치솟던 부분도 해결하였고, 번쩍임도 해결되었습니다.
- 그렇게 로드가 끝나면 `SetupOffScreenPanels`로 로드된 씬들을 위치에 맞게 재배열하고 메인씬을 제외한 모든 씬을 비활성화하여 '준비' 상태로 만들었습니다.

## Lightning Manager & LightningController
- 캔버스에 번개 효과를 가진 프리팹을 생성하고, 랜덤으로 배치하고, 내부의 이페그를 재생시킵니다. 번쩍! 쿠르릉... 서서히 사라지기... 를 원했기 때문에 `effectsequence`를 `LightningController`에서 처리하고, 이곳에서는 나타나고 -> `FireLightning`를 통해 `LightningController`에서 프리팹의 이펙트를 재생시키고 사라지게 만들었습니다. 무작위 랜덤성을 강조하고 번쩍거리고 천천히 사라지며 잔상까지 남는 효과 구현을 위해 노력했습니다. 이미지 개별에 빛나는 효과를 추가하면 당연히 연산을 과하게 사용하므로 각각의 Lightning 이미지에 Bloom 이미지를 추가하여 개별적으로 관리하고 사용하였습니다.
- 또한 유니티 자체 파티클 시스템도 이곳에서 사용해보았습니다.
