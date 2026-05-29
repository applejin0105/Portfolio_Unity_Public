# Portfolio_Unity_Public

React로 만든 포트폴리오 홈페이지를, 유니티의 기술을 그대로 담아 재구성한 프로젝트입니다.

> **핵심부터 보고 싶다면** [배틀 시스템 바로가기](./Scripts/Scenes/Battle)
>
> 📁 각 항목의 폴더 링크에는 **자세한 코드 설명(README)** 이 첨부되어 있습니다.

---

## 시연 영상

| React 버전 | Unity 버전 |
|:---:|:---:|
| [![React Version](https://img.youtube.com/vi/TFC5AXB2qPM/maxresdefault.jpg)](https://youtu.be/TFC5AXB2qPM) | [![Unity Version](https://img.youtube.com/vi/k-yr_oxbnEc/maxresdefault.jpg)](https://youtu.be/k-yr_oxbnEc) |

---

## 추천 코드 열람 순서

프로젝트를 처음 보신다면 아래 순서를 추천합니다. **기반 구조 → 공통 컴포넌트 → 씬 흐름 → 핵심(Battle)** 순으로 자연스럽게 이해할 수 있습니다.

| 순서 | 영역 | 보면 좋은 이유 |
|:---:|---|---|
| **1** | [Core](#-core) | `Bootstrapper`, `Manager` 등 게임 전반을 떠받치는 기반 로직. 전체 동작 원리의 출발점입니다. |
| **2** | [Components](#-components) | 모든 씬에서 재사용되는 UI 이펙트 · 버튼 시스템. |
| **3** | [Scenes](#-scenes) | 실제 화면 흐름(Intro → Main → 각 콘텐츠). 씬 단위로 기능을 확인할 수 있습니다. |
| **4** | [Battle ⭐](#battle-) | 프로젝트의 핵심. '롤토체스' 형식의 게임 시스템 전체. |

<br>

---

# Components

## Components.Effects.UI

> 인스펙터에서 애니메이션 데이터를 설정하고, 코드로 이펙트를 제어할 수 있도록 구조를 분리한 **커스텀 UI 이펙트 시스템**입니다. DOTween을 사용하는 상태에서도 막힘없이 동작하도록 설계하여 유지보수와 재사용성을 높였습니다.

| 폴더 | 설명 |
|---|---|
| [📁 `UI Effect Core`](./Scripts/Components/Effects/UI/Core) | UI Effect의 핵심 구성 요소를 담은 Core |
| [📁 `UI Effect Types`](./Scripts/Components/Effects/UI/Types) | 여러 UI Effect를 모아둔 파일 |
| [📁 `Button Core`](./Scripts/Components/Common/Buttons/Core) | 버튼의 핵심 구성 요소를 담은 Core |
| [📁 `Button Types`](./Scripts/Components/Common/Buttons/Types) | Compound Button을 상속받아 각기 다른 효과를 가진 Button들 |

<br><br>
<div align="center"><code> · · · </code></div>
<br>

---

# Core

> 게임에 들어가는 Bootstrapper, 총괄 Manager와 같은 **Core Logic**들을 구현한 네임스페이스입니다.

### Assigner
> Global Bootstrapper에서 관리하는 Preload용 카메라(혹은 이미 존재하는 카메라)를, 캔버스에 존재하는 카메라에 막힘없이 자동 연결하도록 설계했습니다.

| 폴더 | 설명 |
|---|---|
| [📁 `CanvasCameraAssigner`](./Scripts/Core/Assigner) | 카메라를 Camera Canvas에 자동으로 등록 |

### Attributes
> 인스펙터 확장용 에디터 스크립트들을 설계했습니다.

| 폴더 | 설명 |
|---|---|
| [📁 `Attributes`](./Scripts/Core/Attributes) | README 첨부용 Top 딕셔너리 |
| [📁 `Drawer`](./Scripts/Core/Attributes/Drawer) | 화면에 그리는 Drawer |
| [📁 `Editor`](./Scripts/Core/Attributes/Editor) | 실질적 기능들 |

### Bootstrapper
> Scene 진입 시 필요한 모든 요소를 자동으로 할당해주는 Bootstrapper를 설계했습니다.

| 폴더 | 설명 |
|---|---|
| [📁 `Bootstrapper`](./Scripts/Core/Bootstrapper) | 실행 시 자동으로 셋팅되는 Bootstrapper |

### Data
> 프로젝트 내부에서 자주 쓰이는 공통 Enum, PlayedGames · Projects용 json, Scene 전환용 데이터 클래스, 로컬/서버 모드의 데이터 로딩 경로 처리를 일괄 설계했습니다.

| 폴더 | 설명 |
|---|---|
| [📁 `Data`](./Scripts/Core/Data) | 서버 및 로컬 모드 데이터 로딩 로직 |
| [📁 `Models`](./Scripts/Core/Data/Models) | Scene 전환 시 사용되는 TargetSceneName Data |
| [📁 `PlayedGamesData`](./Scripts/Core/Data/Models/PlayedGamesData) | 게임 플레이 데이터 json |
| [📁 `ProjectsData`](./Scripts/Core/Data/Models/ProjectsData) | 개인 프로젝트 데이터 json |
| [📁 `Enums`](./Scripts/Core/Data/Enums) | 프로젝트 내에서 사용하는 enums |

### Events
> 모든 Scene에서 사용하는 공통 Event들과 Scene 전환에 쓰이는 이벤트들을 설계했습니다.

| 폴더 | 설명 |
|---|---|
| [📁 `Events`](./Scripts/Core/Events) | Scene 전환 이벤트, 로딩 Scene 진입 로직 |

### Extensions
> 프로젝트 전반의 재사용성과 개발 편의성을 극대화하는 핵심 유틸리티(Core Utilities) 및 확장(Extensions) 스크립트들을 설계했습니다.

| 폴더 | 설명 |
|---|---|
| [📁 `Extensions`](./Scripts/Core/Extensions) | UI · 그래픽 최적화, 카메라 · 화면 제어, 디버깅 · 시각화, 수학 · 물리 연산, 입력 · 인터랙션 |

### Interfaces
> Scene 전환 과정에서 공통적으로 사용하는 Interface들을 설계했습니다. 내부 로직은 Scene마다 다르지만, OOP에 의거해 일괄 통제하고 통일성을 높이도록 설계했습니다.

| 폴더 | 설명 |
|---|---|
| [📁 `Interfaces`](./Scripts/Core/Interfaces) | SceneUIController와 SceneLifecycle 로직 |

### Managers
> 모든 Scene은 SceneManager와 SceneUIController를 가집니다. 이에 따라 모든 SceneManager의 부모 클래스인 `BaseSceneManager`, 공통 SettingUI를 제어하는 Manager, Scene 전환용 Manager 등 프로젝트의 Core 기능을 제어하는 Manager들을 설계했습니다.

| 폴더 | 설명 |
|---|---|
| [📁 `Managers`](./Scripts/Core/Managers) | BaseSceneManager를 비롯한 Core Manager 모음 |

<br><br>
<div align="center"><code> · · · </code></div>
<br>

---

# 🎬 Scenes

> 실제 화면 흐름 순서대로 정리되어 있습니다. (Intro → Warning → Loading → Main → 콘텐츠 씬 → Battle)

### Intro
> 가장 첫 번째 Scene. 비밀번호를 통한 접근 절차를 사용하며, 내부 확장을 통해 Local 혹은 Server로 비밀번호를 검증할 수 있도록 설계했습니다.

| 폴더 | 설명 |
|---|---|
| [📁 `Manager`](./Scripts/Scenes/Intro/Manager) | Intro Scene을 관리하는 매니저 |
| [📁 `Controller`](./Scripts/Scenes/Intro/Controller) | Intro Scene의 UI를 관리하는 Controller |

### Warning
> 두 번째 Scene. 프로젝트에 기재된 내용과 프로젝트 자체에 대한 간단한 설명이 담겨 있습니다.

| 폴더 | 설명 |
|---|---|
| [📁 `Manager`](./Scripts/Scenes/Warning/Manager) | Warning Scene을 관리하는 매니저 |
| [📁 `Controller`](./Scripts/Scenes/Warning/Controller) | Warning Scene의 UI를 관리하는 Controller |

### Loading
> 모든 씬 전환에서 공통적으로 사용되는 로딩 Scene입니다.

| 폴더 | 설명 |
|---|---|
| [📁 `Manager`](./Scripts/Scenes/Loading/Manager) | Loading Scene을 관리하는 매니저 |
| [📁 `Controller`](./Scripts/Scenes/Loading/Controller) | Loading Scene의 UI를 관리하는 Controller |

### Main
> 프로젝트의 Main Scene. 각기 다른 Scene으로 접근할 수 있으며, 저에 대한 기본 정보와 설명이 담겨 있습니다.

| 폴더 | 설명 |
|---|---|
| [📁 `Manager`](./Scripts/Scenes/Main/Manager) | Main Scene을 관리하는 매니저 |
| [📁 `Controller`](./Scripts/Scenes/Main/Controller) | Main Scene의 UI를 관리하는 Controller |

### BasicInfo
> '포트폴리오' 버튼을 누르면 이동하는 Scene. 저에 대한 자세한 정보와 소개, 경력, 기술 스택 등을 담고 있습니다.

| 폴더 | 설명 |
|---|---|
| [📁 `BasicInfo`](./Scripts/Scenes/BasicInfo) | BasicInfo에서 사용하는 Button들의 Mapping용 클래스 |
| [📁 `Manager`](./Scripts/Scenes/BasicInfo/Manager) | BasicInfo Scene을 관리하는 매니저 |
| [📁 `Controller`](./Scripts/Scenes/BasicInfo/Controller) | BasicInfo Scene의 UI를 관리하는 Controller |

### Projects
> '프로젝트' 버튼을 누르면 이동하는 Scene. 지금까지 진행한 프로젝트와 공부에 대한 소개를 담고 있으며, 각 프로젝트에서 깃 · 유튜브 · 티스토리 링크로 이동할 수 있습니다.

| 폴더 | 설명 |
|---|---|
| [📁 `Manager`](./Scripts/Scenes/Projects/Manager) | Projects Scene을 관리하는 매니저 |
| [📁 `Controller`](./Scripts/Scenes/Projects/Controller) | Projects Scene의 UI를 관리하는 Controller |
| [📁 `Data`](./Scripts/Scenes/Projects/Data) | Projects의 json을 동적 할당하기 위한 Data |

### PlayedGames
> '게임 플레이' 버튼을 누르면 이동하는 Scene. 지금까지 플레이한 게임 중 일부를 소개합니다.

| 폴더 | 설명 |
|---|---|
| [📁 `Manager`](./Scripts/Scenes/PlayedGames/Manager) | PlayedGames Scene을 관리하는 매니저 |
| [📁 `Controller`](./Scripts/Scenes/PlayedGames/Controller) | PlayedGames Scene의 UI를 관리하는 Controller |
| [📁 `Data`](./Scripts/Scenes/PlayedGames/Data) | PlayedGames의 json을 동적 할당하기 위한 Data |

### RoadMaps
> '로드맵' 버튼을 누르면 이동하는 Scene. 회사의 인재상에 맞춰 제가 생각하는 로드맵과 목표를 담고 있습니다.

| 폴더 | 설명 |
|---|---|
| [📁 `Manager`](./Scripts/Scenes/RoadMaps/Manager) | RoadMaps Scene을 관리하는 매니저 |
| [📁 `Controller`](./Scripts/Scenes/RoadMaps/Controller) | RoadMaps Scene의 UI를 관리하는 Controller |
| [📁 `Trigger`](./Scripts/Scenes/RoadMaps/Trigger) | RoadMaps Scene의 RadialTransition을 발동시키는 Trigger |

### Battle ⭐
> 프로필 카드를 좌측 상단 버튼으로 전환하고, 버튼을 누르면 접근 가능한 **'롤토체스' 형식의 게임**입니다. 이 프로젝트의 핵심입니다.

| 폴더 | 설명 |
|---|---|
| [📁 `Battle`](./Scripts/Scenes/Battle) | Battle Scene의 코드와 예시를 전부 담은 궁극의 폴더 |
| [📁 `Manager`](./Scripts/Scenes/Battle/Manager) | Battle Scene을 관리하는 매니저 |
| [📁 `Controller`](./Scripts/Scenes/Battle/Controller) | Scene 진입 · 퇴장을 담당하는 Controller (내부 게임 UI 제외) |
| [📁 `Core`](./Scripts/Scenes/Battle/Core) | Core 로직(BattleDirector, BattleManager, TargetingResolver) |
| [📁 `Board`](./Scripts/Scenes/Battle/Board) | Battle Scene의 Board 정의 및 관리 |
| [📁 `Cameras`](./Scripts/Scenes/Battle/Cameras) | Battle Scene의 Camera 정의 및 관리 |
| [📁 `Combat`](./Scripts/Scenes/Battle/Combat) | Battle Scene의 Combat 정의 및 관리 |
| [📁 `Data`](./Scripts/Scenes/Battle/Data) | Battle Scene의 Data 정의 및 관리 |
| [📁 `Entity`](./Scripts/Scenes/Battle/Entity) | Battle Scene에서 사용하는 Entity 정의 및 관리 |
| [📁 `Logic`](./Scripts/Scenes/Battle/Logic) | Battle Scene의 Logic 정의 및 관리 |
| [📁 `Shop`](./Scripts/Scenes/Battle/Shop) | Battle Scene의 Shop 정의 및 관리 |
| [📁 `UI`](./Scripts/Scenes/Battle/UI) | Battle Scene의 모든 UI 정의 및 관리 |

<br>

<div align="center">
  <img width="716" height="790" alt="Battle System Diagram" src="https://github.com/user-attachments/assets/7a91a682-2344-48b2-b698-c1f197dbb302" />
</div>
