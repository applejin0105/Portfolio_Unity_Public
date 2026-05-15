### Loading Scene
> **Overview:** 프로젝트의 모든 로딩을 처리합니다. 이 과정에서 기기의 사양을 점검하여 Preload인지 SingleLoad인지 구분합니다.

<img width="800" height="450" alt="Loading" src="https://github.com/user-attachments/assets/1a6476b4-d97a-41eb-85a4-4ca81642cadd" />

### 1. 동적 로딩 시스템 및 메모리 최적화 (`LoadingSceneManager`)
사용자의 기기 가용 RAM 상태를 프로파일링하여 환경에 맞는 최적의 로딩 방식을 동적으로 선택하도록 구현했습니다.
* **저사양 기기 (RAM 6000MB 미만):** `LoadSceneMode.Single`을 통한 안전한 단일 씬 로딩 수행.
* **고사양 기기:** `LoadSceneMode.Additive`를 활용한 다중 씬 사전 로딩(Preload) 수행. 모든 씬의 로딩 진행률 평균을 계산하여 UI에 매끄럽게 반영.

### 2. 트러블슈팅: Additive 다중 씬 로드 시 렌더링 겹침 및 프레임 스파이크 해결
* **문제:** 다중 씬을 비동기로 병합하는 과정에서 씬이 활성화(`allowSceneActivation = true`)될 때마다 화면이 번쩍거리고 UI가 겹치는 현상 및 순간적인 프레임 드랍 발생.
* **분석:** 모든 씬의 `Awake/Start` 연산이 한 프레임에 집중되면서 CPU 부하 급증 및 카메라 렌더링 충돌 확인.
* **해결 (Off-screen 기법 및 프레임 분산):** * 로드된 씬의 최상단 UI 패널을 즉각적으로 카메라 밖 화면 외곽(`Vector2(99999, 99999)`)으로 이동시켜 시각적 충돌 원천 차단.
    * `yield return null`을 통해 무거운 초기화 연산을 여러 프레임으로 분산시켜 프로파일러 상의 메모리 스파이크 억제.
    * 로딩 화면의 부드러운 퇴장 연출 보장 및 사용자 경험 개선.

### 3. 비동기 진행률 동기화 UI (`LoadingUIController`)
실제 로딩 진행률과 UI 애니메이션 간의 시각적 괴리를 방지하기 위해 큐(Queue) 시스템을 도입했습니다.
* 실제 로딩 데이터가 UI 애니메이션보다 앞서갈 경우, 그 격차(`backlog`)를 계산하여 `UI Fill Effect` 재생 속도를 동적으로 가속화함으로써 정밀한 상태 피드백 구현.

### 4. 리소스 최적화를 고려한 이펙트 연출 (`LightningManager`)
* **오브젝트 풀링(Object Pooling):** 빈번하게 생성/소멸되는 번개 이펙트 UI에 풀링 시스템을 적용하여 가비지 컬렉터(GC) 호출 및 런타임 오버헤드 최소화.
* **퍼포먼스 셰이딩 우회:** 무거운 실시간 Post-Processing Bloom 효과 대신, 발광용 Bloom 스프라이트를 분리하여 Alpha 값을 제어하는 방식으로 모바일 환경에서도 가벼우면서도 화려한 뇌우 효과 구현.
