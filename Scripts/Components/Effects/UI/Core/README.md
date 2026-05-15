### UI Effect System

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
