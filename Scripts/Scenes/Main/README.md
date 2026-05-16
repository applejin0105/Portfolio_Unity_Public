### Main Scene
> **요약:** 프로젝트의 허브 역할을 하는 메인 로비 씬입니다. 가장 처음 마주하는 공간으로, 모든 씬으로의 접근을 제어하며 다채로운 인터랙티브(Interactive) UI 이펙트를 제공합니다.

<img width="800" height="450" alt="Main" src="https://github.com/user-attachments/assets/72004e94-c330-4c93-8aa0-a120f6d7dcde" />

#### 주요 로직
* **수학적 보간을 활용한 인터랙티브 UI 연출 (`ClickSpinController`):** * 사용자의 클릭 위치(좌측/우측)를 `ScreenPointToLocalPointInRectangle`로 계산하여 회전 방향을 동적으로 결정합니다.
  * `Cubic Ease Out` 공식(`1f - Mathf.Pow(1f - t, 3f)`)을 적용하여, UI가 단순히 도는 것이 아니라 튕기듯 회전하다가 서서히 감속하는 '팅!' 하는 타격감 있는 물리적 연출을 구현했습니다.
  * `HashSet`을 도입해 현재 회전 중인 타겟을 관리하여, 무분별한 중복 클릭 시 연출이 꼬이거나 프레임이 저하되는 현상을 방지했습니다.
* **상태 동기화 및 카드 플립 로직 (`MainSceneUIController`):** 프로필 티켓의 앞/뒷면 전환 시 `Quaternion.Slerp`을 이용한 3D 회전과 함께 Alpha 값, Raycast Block 상태를 정밀하게 동기화하여 앞면과 뒷면 UI가 자연스럽게 교차되도록 구현했습니다.
* **자료구조를 활용한 상태 순환 (`ClickableDecoController`):** 마우스 오버/아웃 시 데코레이션 이미지가 변경되는 로직에 `LinkedList`를 적용하여, 배열 인덱스 초과 오류 없이 무한으로 스프라이트가 순환하도록 안정성을 높였습니다.

#### 트러블슈팅: 비동기 UI 연출 중 MissingReferenceException 에러 해결
* **문제:** `ClickSpinController`의 UI 회전(Spin) 연출 도중, 유저의 빠른 조작으로 인해 씬이 전환되거나 UI 객체가 파괴될 경우 간헐적으로 `MissingReferenceException`이 발생하며 크래시가 일어나는 현상.
* **분석:** 회전 애니메이션이 코루틴(비동기)으로 작동하고 있어, 타겟 `RectTransform`의 생명주기가 끝난 후에도 `while` 루프가 매 프레임 해당 타겟의 `localRotation`을 수정하려 시도하면서 발생한 메모리 댕글링(Dangling) 참조 오류였습니다.
* **해결 (Null-Safe 코루틴 및 가비지 클린업):**
  * 코루틴의 `while` 루프 내부 최상단에 `if (target == null) yield break;` 방어 로직을 추가하여, 대상 객체가 메모리에서 해제되는 즉시 코루틴도 함께 안전하게 종료되도록 라이프사이클을 맞췄습니다.
  * 이벤트 진입점(`OnPointerClick`)에 `targetRects.RemoveAll(rect => rect == null);`을 추가하여, 이미 파괴된 오브젝트가 리스트에 남아 연산을 방해하지 않도록 사전 정리 로직을 구축하여 예외 발생을 원천 차단했습니다.
