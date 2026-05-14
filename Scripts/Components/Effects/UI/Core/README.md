### UI Effect System

**Description**
- **역할:** UI의 여러 효과를 제어하고, 동시에 혹은 순차적으로 출력 가능한 시스템입니다.
- **구조**
  - `[UIEffect]`: 모든 UI Effect의 공통 분모(타겟의 RectTransform, 애니메이션 재생시간, 애니메이션 타입 -Normal/DOTween-을 관리)
  - `[UIConfigurableEffect]`: 이펙트 실행 추상화. Data-Driven을 통해 이펙트가 '언제' 실행되는지는 공통으로 관리하고, 눌렸을 때 '어떻게' 변할지는 각 이펙트가 개별적으로 주입하도록 구분.
  - `[EffectStep]`: 순차적으로 실행될 이펙트들을 담는 클래스. 동시실행 혹은 대기 후 실행 로직으로 구분.
  - `[EffectSequence]`: EffectStep들을 모아서 여러개의 시퀀스 루틴을 수행하는 클래스. 모든 이펙트들을 실행하거나 정지하는 역할.
  - `[EffectSequenceExtensions]`: 이펙트 랜덤 실행을 위한 클래스. 기존 `EffectSequence`의 말 그대로 Extension.
  - `[KoreanTypingHelper]`: 한글 타이핑 효과만을 위한 Helper 클래스. 한글 타이핑 시 '완성된' 하나의 글자가 아닌 글자 하나하나를 합치기 위해 구성한 클래스.
- **주요 로직:**
  - 단순한 페이드 효과부터 글리치, 텍스트 타이핑까지 다양한 프로젝트에서 즉시 재사용할 수 있도록 범용성을 목표로 구현한 UI 이펙트 모음입니다.
- **특징 및 고려사항:**
  - 이 프로젝트에서 배틀 시스템과 더불어 시간을 가장 많이 잡아먹은 구현 부분. 요구사항이 늘어나며 시스템의 규모가 커졌지만, 모바일 환경에서의 퍼포먼스를 유지하기 위해 가비지 컬렉션(GC) 최소화와 메모리 최적화를 최우선으로 고려하며 구현했습니다.
  - 현재의 통합된 구조에서 나아가, 향후에는 각 이펙트 모듈의 결합도를 더욱 낮추고 응집도를 높이는 방향으로 지속적인 리팩토링을 계획하고 있습니다.

**Core Code Snippet**
```csharp
        public virtual void Play()
        {
            if (!gameObject.activeInHierarchy) return;

            if (EffectCoroutine != null) Stop(false);

            EffectCoroutine = StartCoroutine(ExecuteEffect());
        }

        public virtual IEnumerator PlayWaitable()
        {
            if (!gameObject.activeInHierarchy) yield break;

            if (EffectCoroutine != null) Stop(false);

            EffectCoroutine = StartCoroutine(ExecuteEffect());
            yield return EffectCoroutine;
        }
```

**Visuals**
![[이미지/GIF 설명]]([이미지/GIF 경로])
[![[영상 썸네일 설명]]([썸네일 경로])]([영상 링크 주소])


## Components.Common.Buttons.Core
> **Overview:** 인스펙터에서 애니메이션 데이터를 설정하고, 코드로 이펙트를 제어할 수 있도록 구조를 분리한 커스텀 UI 버튼 시스템입니다. 유지보수와 재사용이 편하도록 이벤트 감지와 실행 로직을 나누어 설계했습니다.

---

### CompoundButton System

**Description**
- **역할:** 버튼의 상태(Hover, Pressed 등)에 따라 크기, 색상, 매테리얼 효과(Glow, Bloom) 등을 자연스럽게 전환해 주는 복합 버튼 컨트롤러입니다.
- **구조:**
  - `UIButtonEffect`: 상태 머신 및 인터페이스 이벤트 감지
  - `UIButtonConfigurableEffect<T>`: 이펙트 실행 추상화
  - `ButtonConfig`: 인스펙터에서 제어할 UI 데이터 구조체
- **주요 로직:**
  - Unity에서 기본으로 제공하는 Button 컴포넌트에 의존하지 않고, `IPointerClickHandler` 등의 인터페이스를 직접 구현하여 UI 연출과 클릭 이벤트를 더 세밀하게 제어하도록 만들었습니다.
- **특징 및 고려사항:**
  - Material 연출 시 생성되는 인스턴스를 배열로 따로 관리하고, `OnDestroy` 시 직접 해제하도록 처리하여 메모리 누수를 방지했습니다.
  - DOTween을 사용하는 방식과 패키지 의존성이 없는 기본 Coroutine 방식을 모두 구현하여, 프로젝트 환경에 맞춰 선택해 쓸 수 있도록 유연성을 두었습니다.

**Core Code Snippet**
```csharp
// 상태 처리 로직과 이펙트 실행 로직의 분리
protected void UpdateButtonState()
{
    ButtonState newState;

    if (!IsInteractable) newState = ButtonState.Disabled;
    else if (IsPressed) newState = IsHovering ? ButtonState.Pressed : ButtonState.Normal;
    else newState = IsHovering ? ButtonState.Hover : ButtonState.Normal;

    if (currentState != newState)
    {
        currentState = newState;
        Play(); // 상태가 변경되었을 때만 Effect 실행
    }
}

protected override IEnumerator ExecuteEffect()
{
    var configToUse = _overrideConfig ?? defaultConfig;
    
    // 설정된 애니메이션 타입에 따라 분기 처리
    yield return animType switch
    {
        AnimType.Normal => CompoundButtonRoutine(configToUse, configToUse.animDuration),
        AnimType.DoTween => CompoundButtonDoTweenRoutine(configToUse, configToUse.animDuration),
        _ => throw new ArgumentOutOfRangeException()
    };
}
```

**Visuals**
![[이미지/GIF 설명]]([이미지/GIF 경로])
[![[영상 썸네일 설명]]([썸네일 이미지 경로])]([영상 링크 주소])
