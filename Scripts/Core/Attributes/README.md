### Inspector Extensions

**Description**
- **역할:** 기획자 및 디자이너의 작업 효율을 높이고, 잘못된 데이터 입력을 방지하기 위해 유니티 기본 인스펙터(Inspector)의 GUI를 확장하고 커스텀하는 에디터 툴킷입니다.
- **구조:**
  - 모든 확장은 런타임에 데이터 명찰 역할을 하는 Attribute 클래스와, 유니티 에디터 환경(UNITY_EDITOR)에서만 동작하며 실제 화면을 그리는 PropertyDrawer 클래스로 완벽히 분리되어 있습니다.
  - `ShowIf`: 직렬화된 변수(SerializedProperty)의 값이나 문자열 형태의 논리 조건식(&&, ||, ==, !=)을 파싱하여, 조건에 따라 GetPropertyHeight를 0으로 만들어 인스펙터에서 동적으로 UI를 숨기거나 노출합니다.
  - `SerializableDictionary`: 유니티의 기본 직렬화(Serialization) 시스템이 Dictionary를 지원하지 않는 한계를 우회하기 위해, ISerializationCallbackReceiver 인터페이스를 활용해 직렬화 전후로 List<Key>, List<Value>로 데이터를 변환하고, Drawer를 통해 이를 테이블 형태의 GUI로 렌더링합니다.
  - `MinMaxSlider`: Vector2 데이터 타입을 활용하여, EditorGUI.MinMaxSlider와 두 개의 FloatField를 조합해 최소/최대 범위를 직관적으로 조절할 수 있는 커스텀 슬라이더를 제공합니다.
  - `DynamicFillOrigin`: 동일한 클래스 내에 있는 다른 변수(Sibling Property, 예: Image.FillMethod)의 현재 상태를 실시간으로 추적하여, 해당 타입에 맞는 Enum 드롭다운 창으로 동적 스와핑(Swapping)을 수행합니다.
- **주요 로직:**
  - 직렬화 프로퍼티 제어: 실제 오브젝트(Instance)에 직접 접근하지 않고, SerializedProperty를 통해 데이터를 읽고 씀으로써 유니티의 Undo/Redo 시스템 및 프리팹(Prefab) 오버라이드 시스템과 안전하게 호환되도록 구성했습니다.
  - 상대 경로 탐색 (FindPropertyRelative): 배열이나 중첩된 클래스 내부에서도 Attribute가 정상 작동하도록, 현재 변수의 propertyPath를 파싱하여 부모 경로를 역추적해 조건 대상 변수를 찾아내는 상대 경로 탐색 알고리즘을 적용했습니다.
- **특징 및 고려사항:**
  - EditorGUI.BeginChangeCheck()와 EndChangeCheck()를 철저히 사용하여, 실제로 인스펙터에서 값이 변경되었을 때만 데이터를 저장(Dirty)하도록 최적화했습니다.
  - 런타임 빌드 시 에디터 관련 코드(UnityEditor 네임스페이스)가 포함되어 빌드 에러가 발생하는 것을 방지하기 위해 #if UNITY_EDITOR 매크로(또는 Assembly Definition) 분리를 철저히 고려하여 설계했습니다.

**Core Code Snippet**
```csharp
    // [ShowIf 및 DynamicFillOrigin의 핵심 로직: Sibling Property 안전 탐색]
    // 단순 객체 참조가 아닌, SerializedProperty의 path를 파싱하여 
    // 배열이나 중첩 클래스 내부에서도 대상 변수를 정확히 찾아내는 헬퍼 메서드
    private SerializedProperty FindPropertyRelative(SerializedProperty property, string fieldName)
    {
        string propertyPath = property.propertyPath;
        int dotIndex = propertyPath.LastIndexOf('.');
        
        // dotIndex가 0보다 작으면 최상위 변수, 아니면 부모 객체의 경로를 추출하여 대상 필드와 결합
        string conditionPath = dotIndex < 0 ? fieldName : propertyPath.Substring(0, dotIndex) + "." + fieldName;

        SerializedProperty foundProp = property.serializedObject.FindProperty(conditionPath);
        if (foundProp == null)
        {
            Debug.LogWarning($"[CustomDrawer] '{conditionPath}' 변수를 찾을 수 없습니다!");
        }

        return foundProp;
    }
```
- 기획과 틀을 잡고, 에디터 스크립팅은 AI의 도움을 받아 구현하였습니다. 하지만 코드를 적용하면서 단순히 Ctrl+C/V를 한 것이 아니라, Attribute와 Drawer가 분리되어야 하는 이유(빌드 종속성), 그리고 SerializedProperty를 통해 값을 수정해야만 유니티의 Undo(Ctrl+Z) 기능이 고장 나지 않는다는 점을 학습하고 구조를 다듬었습니다. 특히 ShowIf나 DynamicFillOrigin처럼 다른 변수의 상태를 참조해야 할 때, 중첩 클래스나 리스트 내부에서 경로가 꼬이는 문제를 해결하기 위해 propertyPath를 파싱하는 로직을 깊게 파고들며 유니티 인스펙터 구조를 완벽히 이해하게 되었습니다.
