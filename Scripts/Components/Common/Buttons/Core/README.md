## Components.Common.Buttons.Core
> **Overview:** 인스펙터에서 애니메이션 데이터를 설정하고, 코드로 이펙트를 제어할 수 있도록 구조를 분리한 커스텀 UI 버튼 시스템입니다. 유지보수와 재사용이 편하도록 이벤트 감지와 실행 로직을 나누어 설계했습니다.


### CompoundButton System

**Description**
**역할:** 버튼의 상태(Hover, Pressed 등)에 따라 크기, 색상, 매테리얼 효과(Glow, Bloom) 등을 자연스럽게 전환해 주는 복합 버튼 컨트롤러입니다.
**구조:**
  - `UIButtonEffect`: 상태 머신 및 인터페이스 이벤트 감지
  - `UIButtonConfigurableEffect<T>`: 이펙트 실행 추상화
  - `ButtonConfig`: 인스펙터에서 제어할 UI 데이터 구조체
**주요 로직:**
  - Unity에서 기본으로 제공하는 Button 컴포넌트에 의존하지 않고, `IPointerClickHandler` 등의 인터페이스를 직접 구현하여 UI 연출과 클릭 이벤트를 더 세밀하게 제어하도록 만들었습니다.
**특징 및 고려사항:**
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
