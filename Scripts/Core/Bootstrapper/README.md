### Bootstrapper
- 프로젝트내에서 필수적으로 사용되며, 하나만 존재해야하는, 일관성이 요구되는 각 Manager들을 사전에 프리팹화하여 이 Bootstrapper에서 불러옵니다.
- `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]` 메타데이터를 통해, 씬 로딩 직전에 자동으로 수행되게끔 설계했습니다.
