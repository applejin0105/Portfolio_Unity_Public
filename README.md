# Portfolio_Unity_Public
---
# Components

## Components.Effects.UI
> **Overview:** 인스펙터에서 애니메이션 데이터를 설정하고, 코드로 이펙트를 제어할 수 있도록 구조를 분리한 커스텀 UI 이펙트 시스템입니다. 유지보수와 재사용이 편하도록, DOTween을 사용하거나 사용하는 상태에서도 막힘없이 사용 가능하도록 설계했습니다.
> 
> 해당 깃 페이지에(하단 링크) 자세한 코드 설명이 첨부되어있습니다.

**Links**

*UI Effect*
- [📁 `UI Effect Core` (UI Effect의 핵심 구성 요소를 담고있는 Core)](./Scripts/Components/Effects/UI/Core)
- [📁 `UI Effect Types` (여러 UI Effect를 모아둔 파일)](./Scripts/Components/Effects/UI/Types)

*Buttons*
- [📁 `Button Core` (버튼의 핵심 구성 요소를 담고있는 Core)](./Scripts/Components/Common/Buttons/Core)
- [📁 `Button Types` (Compound Button을 상속받으면서, 다른 효과를 가진 Button들)](./Scripts/Components/Common/Buttons/Types)
<br><br><br>
<div align="center">
  <code> · · · </code>
</div>
<br><br><br>

# Core
> 게임에 들어가는 Bootstrapper, 총괄 Manager와 같은 Core Logic들을 구현한 네임스페이스입니다.

## Assigner
> **Overview:** Global Bootstrapper에서 관리하는 Preload용 카메라 혹은 카메라가 이미 존재하는 경우에도 막힘없이 이를 캔버스에 존재하는 카메라에 자동으로 연결하도록 설계했습니다.
> 
> 해당 깃 페이지에(하단 링크) 자세한 코드 설명이 첨부되어있습니다.

**Links**
- [📁 `CanvasCameraAssigner` (카메라를 Camera Canvas에 자동으로 등록)](./Scripts/Core/Assigner)

<br>

## Attributes
> **Overview:** 인스펙터 확장용 에디터 스크립트들을 설계했습니다.
> 
> 해당 깃 페이지에(하단 링크) 자세한 코드 설명이 첨부되어있습니다.

**Links**
- [📁 `Attributes` (화면에 그리는 Drawer)](./Scripts/Core/Attributes)
- [📁 `Editor` (실질적 기능들)](./Scripts/Core/Attributes/Editor)

<br>

## Bootstrapper
> **Overview:** Scene 진입 시 필요한 모든 요소들을 자동으로 할당해주는 Bootstrapper를 설계했습니다.
> 
> 해당 깃 페이지에(하단 링크) 자세한 코드 설명이 첨부되어있습니다.

**Links**
- [📁 `Bootstrapper` (실행 시 자동으로 셋팅되는 Bootstrapper)](./Scripts/Core/Bootstrapper)

<br>

## Data
> **Overview:** 프로젝트 내부에서 자주 사용되는 공통 Enum, Played Games와 Projects에서 사용하는 json, Scene 전환용 데이터 클래스, 로컬 및 서버 모드에서의 데이터 로딩 경로 데이터 처리를 일괄적으로 설계했습니다.
> 
> 해당 깃 페이지에(하단 링크) 자세한 코드 설명이 첨부되어있습니다.

**Links**
- [📁 `Data` (서버 및 로컬 모드 데이터 로딩 로직)](./Scripts/Core/Data)
- [📁 `Models` (Scene 전환 시 사용되는 TargetSceneName Data)](./Scripts/Core/Data/Models)
- [📁 `PlayedGamesData` (게임 플레이 데이이터 json)](./Scripts/Core/Data/Models/PlayedGamesData)
- [📁 `ProjectsData` (개인 프로젝트 데이터 json)](./Scripts/Core/Data/Models/ProjectsData)
- [📁 `Enums` (프로젝트내에서 사용하는 enums)](./Scripts/Core/Data/Enums)


<br>

## Events
> **Overview:** 모든 Scene에서 사용하는 공통적인 Events들, Scene 전환에 사용하는 이벤트들을 설계했습니다.
> 
> 해당 깃 페이지에(하단 링크) 자세한 코드 설명이 첨부되어있습니다.

**Links**
- [📁 `Events` (Scene 전환 이벤트, 로딩 Scene 진입 로직)](./Scripts/Core/Events)


<br>

## Extensions
> **Overview:** 프로젝트 전반에서 재사용성을 높이고 개발 편의성을 극대화하는 핵심 유틸리티(Core Utilities) 및 확장(Extensions) 스크립트들을 설계했습니다.
> 
> 해당 깃 페이지에(하단 링크) 자세한 코드 설명이 첨부되어있습니다.

**Links**
- [📁 `Extensions` (UI 및 그래픽 최적화, 카메라 및 화면 제어, 디버깅 및 시각화, 수학 및 물리연산, 입력 및 인터렉션)](./Scripts/Core/Extensions)
- [📁 `Editor` (이미 존재하는 ECNaming을 조금 수정하여 사용하고 있습니다. 클릭 시, 해당 깃 페이지로 이동합니다.)](https://github.com/hahahohohun/PublicCode/blob/main/README.md#ecnamingcs)

<br>

## Interfaces
> **Overview:** Scene 전환 과정에서 공통적으로 사용하는 Interface들을 설계했습니다. 내부 로직은 각 Scene마다 다르지만 OOP에 의거하여 일괄적으로 통제할 수 있게, 그리고 통일성을 높이도록 설계했습니다.
> 
> 해당 깃 페이지에(하단 링크) 자세한 코드 설명이 첨부되어있습니다.

**Links**
- [📁 `Interfaces` (SceneUIController와 SceneLifecycle 로직)](./Scripts/Core/Interfaces)

<br>

## Managers
> **Overview:** Scene에서 공통적으로 사용하는 Manager들을 구현했습니다. 모든 Scene에는 SceneManager와 SceneUIController를 가지고 있습니다. 이에 따라 모든 SceneManager의 Parent Class인 BaseSceneManager, 공통적으로 사용되는 SettingUI를 제어하는 Manager, Scene 전환에 사용되는 Manager와 같이 프로젝트의 Core 기능들을 제어하는 Manger들을 설계했습니다.
> 
> 해당 깃 페이지에(하단 링크) 자세한 코드 설명이 첨부되어있습니다.

**Links**
- [📁 `Managers` (BaseSceneManager를 비롯한 프로젝트에서 사용하는 Core Manager 모음)](./Scripts/Core/Managers)

<br><br><br>
<div align="center">
  <code> · · · </code>
</div>
<br><br><br>

<br>

# Scenes

## Intro
> **Overview:** 가장 첫 번째 Scene인 Intro Scene입니다. 비밀번호를 통한 접근 절차를 사용하였고, 내부 확장을 통해 Local 혹은 Server로 비밀번호를 검증 가능하게 설계했습니다.
> 
> 해당 깃 페이지에(하단 링크) 자세한 코드 설명이 첨부되어있습니다.

**Links**
- [📁 `Manager` (Intro Scene을 관리하는 매니저)](./Scripts/Scenes/Intro/Manager)
- [📁 `Controller` (Intro Scene의 UI를 관리하는 Controller)](./Scripts/Scenes/Intro/Controller)
<br>

## Warning
> **Overview:** 두 번째 Scene인 Warning Scene입니다. 프로젝트에 기재된 내용에 대한 간단한 설명과, 프로젝트에 대한 간단한 설명이 담겨있습니다.
> 
> 해당 깃 페이지에(하단 링크) 자세한 코드 설명이 첨부되어있습니다.

**Links**
- [📁 `Manager` (Warning Scene을 관리하는 매니저)](./Scripts/Scenes/Warning/Manager)
- [📁 `Controller` (Warning Scene의 UI를 관리하는 Controller)](./Scripts/Scenes/Warning/Controller)

<br>

## Loading
> **Overview:** : 공통적으로 사용되는 로딩 Scene입니다.
> 
> 해당 깃 페이지에(하단 링크) 자세한 코드 설명이 첨부되어있습니다.

**Links**
- [📁 `Manager` (Loading Scene을 관리하는 매니저)](./Scripts/Scenes/Loading/Manager)
- [📁 `Controller` (Loading Scene의 UI를 관리하는 Controller)](./Scripts/Scenes/Loading/Controller)

<br>

## Main
> **Overview:** 프로젝트의 Main Scene입니다. 각기 다른 Scene들로 접근이 가능하며, 저에 대한 기본적인 정보와 설명이 담겨있습니다.
> 
> 해당 깃 페이지에(하단 링크) 자세한 코드 설명이 첨부되어있습니다.

**Links**
- [📁 `Manager` (Main Scene을 관리하는 매니저)](./Scripts/Scenes/Main/Manager)
- [📁 `Controller` (Main Scene의 UI를 관리하는 Controller)](./Scripts/Scenes/Main/Controller)

<br>

## BasicInfo
> **Overview:** '포트폴리오' 버튼을 누르면 이동되는 Scene입니다. 저에 대한 자세한 정보와 소개, 경력, 기술 스택등을 담고 있습니다.
> 
> 해당 깃 페이지에(하단 링크) 자세한 코드 설명이 첨부되어있습니다.

**Links**
- [📁 `BasicInfo` (BasicInfo에서 사용하는 Button들의 Mapping을 위한 클래스)](./Scripts/Scenes/BasicInfo)
- [📁 `Manager` (BasicInfo Scene을 관리하는 매니저)](./Scripts/Scenes/BasicInfo/Manager)
- [📁 `Controller` (BasicInfo Scene의 UI를 관리하는 Controller)](./Scripts/Scenes/BasicInfo/Controller)

<br>

## Projects
> **Overview:** '프로젝트' 버튼을 누르면 이동되는 Scene입니다. 제가 지금까지 했던 프로젝트와 공부들에 대한 설명, 소개를 담고 있습니다. 각 프로젝트 내부에는 깃, 유튜브, 티스토리 링크로 이동 가능합니다.
> 
> 해당 깃 페이지에(하단 링크) 자세한 코드 설명이 첨부되어있습니다.

**Links**
- [📁 `Manager` (Projects Scene을 관리하는 매니저)](./Scripts/Scenes/Projects/Manager)
- [📁 `Controller` (Projects Scene의 UI를 관리하는 Controller)](./Scripts/Scenes/Projects/Controller)
- [📁 `Data` (Projects의 json을 동적 할당하기 위한 Data)](./Scripts/Scenes/Projects/Data)
<br>

## PlayedGames
> **Overview:** '게임 플레이' 버튼을 누르면 이동되는 Scene입니다. 제가 지금까지 했던 게임들 중 일부를 소개하고 있습니다.
> 
> 해당 깃 페이지에(하단 링크) 자세한 코드 설명이 첨부되어있습니다.

**Links**
- [📁 `Manager` (PlayedGames Scene을 관리하는 매니저)](./Scripts/Scenes/PlayedGames/Manager)
- [📁 `Controller` (PlayedGames Scene의 UI를 관리하는 Controller)](./Scripts/Scenes/PlayedGames/Controller)
- [📁 `Data` (PlayedGames의 json을 동적 할당하기 위한 Data)](./Scripts/Scenes/PlayedGames/Data)
<br>

## RoadMaps
> **Overview:** '로드맵' 버튼을 누르면 이동되는 Scene입니다. 회사의 인재상에 맞게 제가 생각하는 로드맵과 목표를 담고 있습니다.
> 
> 해당 깃 페이지에(하단 링크) 자세한 코드 설명이 첨부되어있습니다.

**Links**
- [📁 `Manager` (RoadMaps Scene을 관리하는 매니저)](./Scripts/Scenes/RoadMaps/Manager)
- [📁 `Controller` (RoadMaps Scene의 UI를 관리하는 Controller)](./Scripts/Scenes/RoadMaps/Controller)
- [📁 `Trigger` (RoadMaps Scene에서 사용하는 RadiatTransition을 발동시키는 Trigger)](./Scripts/Scenes/RoadMaps/Trigger)

<br>

## Battle
> **Overview:** 프로필 카드를 왼쪽 상단 버튼을 통해 전환하고, 버튼을 누르면 접근 가능한 '롤토체스' 형식의 게임을 담고 있습니다.
> 
> 해당 깃 페이지에(하단 링크) 자세한 코드 설명이 첨부되어있습니다.

**Links**
- [📁 `Manager` (Battle Scene을 관리하는 매니저)](./Scripts/Scenes/Battle/Manager)
- [📁 `Controller` (Battle Scene의 UI를 관리하는 Controller. 이때 UI는 내부 게임 UI가 아닌 Scene 진입 및 퇴장 관련만 담당)](./Scripts/Scenes/Battle/Controller)
- [📁 `Board` (Battle Scene의 Board를 정의하고 관리)](./Scripts/Scenes/Battle/Board)
- [📁 `Cameras` (Battle Scene의 Camera를 정의하고 관리)](./Scripts/Scenes/Battle/Cameras)
- [📁 `Combat` (Battle Scene의 Combat을 정의하고 관리)](./Scripts/Scenes/Battle/Combat)
- [📁 `Core` (Battle Scene의 Core 로직들(Battle Director, BattleManager, TargetingResolver)를 관리)](./Scripts/Scenes/Battle/Core)
- [📁 `Data` (Battle Scene의 Data를 정의하고 관리)](./Scripts/Scenes/Battle/Data)
- [📁 `Entity` (Battle Scene에서 사용하는 Entity를 정의하고 관리)](./Scripts/Scenes/Battle/Entity)
- [📁 `Logic` (Battle Scene의 Logic을 정의하고 관리)](./Scripts/Scenes/Battle/Logic)
- [📁 `Shop` (Battle Scene의 Shop을 정의하고 관리)](./Scripts/Scenes/Battle/Shop)
- [📁 `UI` (Battle Scene의 모든 UI를 정의하고 관리)](./Scripts/Scenes/Battle/UI)
