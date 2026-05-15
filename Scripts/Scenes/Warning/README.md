### Warning Scene
> **Overview:** 프로젝트 진입 전 간단한 프로젝트 소개와 기술 스택에 관한 소개를 담은 화면입니다.

<img width="800" height="450" alt="Warning" src="https://github.com/user-attachments/assets/945faaaf-34ec-498b-8efa-694204a02db1" />

- `WarningSceneManager`: 씬을 전체적으로 관리하는 매니저입니다. Warning Scene의 경우, Preload와 SingleLoad 판별전이라 오직 단일 씬으로 구성되어 있기에 BaseSceneManager를 상속받지 않습니다.
- `WarningSceneUIController`: 페이드인이 끝나면, Typwrite 이펙트가 재생되며 가운데에 모든 글씨를 타이핑 효과로 출력합니다. Click To Start 버튼을 누르면 다음 씬으로 넘어갑니다. 이때, 타이핑 효과 중이라면, 모든 글씨를 노출시키고 넘어갑니다.
