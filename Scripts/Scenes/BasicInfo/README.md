### Basic Info Scene
> **요약:** 제작자의 기본 정보(정보, 소개, 지원 동기, 철학, 기술 스택, 경력)을 카테고리별로 열람할 수 있는 씬입니다. 데이터 간의 이동이 잦은 UI 구조를 고려하여, 컴포넌트 간 결합도를 낮춘 이벤트 기반의 상태 관리(State Management)를 구현했습니다.

<img width="800" height="450" alt="BasicInfo" src="https://github.com/user-attachments/assets/edcec215-39ea-4c2d-85a9-3646e2e3e6ac" />

#### 주요 로직
* **Enum과 Action을 결합한 옵저버(Observer) 패턴 설계 (`ReturnIndex` & `UIController`):** * 각 탭(Tab) 버튼이 `String` 대신 `Enum(Index)` 값을 부여받도록 설계하여 하드코딩으로 인한 오타나 참조 오류를 원천 차단했습니다.
  * 탭을 클릭하면 `UIController`가 직접 버튼들을 제어하는 대신, 변경된 인덱스를 브로드캐스팅(`OnIndexChangedEvent`)합니다.
  * 각각의 `ReturnIndex` 컴포넌트는 이 이벤트를 구독하여, 자신의 `myIndex`와 전달받은 상태를 비교한 뒤 스스로 활성화/비활성화 상태를 결정하도록 구현하여 UI 객체 간의 결합도(Coupling)를 최소화했습니다.
* **통합 트랜지션 관리 (`CommonReset`):** 씬에 진입(`isIn = true`)하거나 퇴장(`isIn = false`)할 때, 분산된 UI 이펙트(Fade, Move 등)들의 `Start/End` 속성값과 물리적 포지션을 한 번에 스왑(Swap)하고 초기화하는 공통 함수를 구축하여 애니메이션 코드의 중복을 제거했습니다.

#### 최적화 포인트: 불필요한 상태 갱신 방지 및 이벤트 최적화
* **설명:** 좌측 인덱스(목차) 탭을 사용자가 빠르게 연속 클릭하거나, 이미 활성화된 현재 탭을 다시 클릭할 때 발생하는 불필요한 코루틴 호출과 UI 렌더링 갱신을 방지해야 했습니다.
* **해결:** `OnClickIndexTitle` 메서드 최상단에 `if (currentIndex == clickedIndex) return;` 방어 로직을 추가하여, 상태가 실제로 변경되었을 때만 이벤트를 발생시키도록 최적화했습니다. 이를 통해 무의미한 연산과 애니메이션 트리거를 차단하여 UI의 반응성과 안정성을 확보했습니다.
