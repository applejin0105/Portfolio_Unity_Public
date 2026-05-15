### Inspector Extensions

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

## ShowIf
> Editor -> ShowIfDrawer.cs의 코드 주석에도 설명이 첨부되어 있습니다.

가령 다음과 같이 클래스가 있다고 가정해보자

```csharp
public enum AttackType { Melee, Magic }

[Serializable]
public class Monster
{
    public string monsterName;
    public AttackType atkType; // 공격 타입 (근접, 마법)

    [ShowIf("atkType == Magic")]
    public float mana; // 마나 (마법 공격일 때만 보여야 함!)
}
```

그럼 여기서 원하는건, atkType이 Magic일때만 Mana가 표시되어야한다.
그럼 유니티는 mana 변수를 그리기 전에, ShoIfDrawer를 호출해서, 이 마나 변수 그릴까요 말까요를 요구한다.
그럼당연히 내부 조건인 atkType == Magic이 참인지 거짓인지 알아야한다.

1. 그렇다면 이 atkType은 도대체 어디에 있는가?
그냥 단순히 Monster 클래스에 있는거 아닌가 -> 그럼 내부 enum이나 strucutre 생성하면 이를 어떻게 처리할건가?
만일 그냥 단순 내부 클래스에서 변수 찾기로 코드를 구성했다고 가정해보자. 그럼 코드는 다음과 같이 아주 쉽게 작성될거다.

```csharp
private SerializedProperty FindPropertyRelative(SerializedProperty property, string fieldName)
{
    // 복잡한 마침표(.) 자르기 다 빼고, 그냥 최상위 객체한테 "이름(fieldName)으로 변수 찾아줘!" 라고 부탁함
    return property.serializedObject.FindProperty(fieldName);
}
```

그럼 당연히, 변수들이 클래스의 root에 있을 때만 정상 작동하고, 배열이나 클래스 안에 묶여 있으면 완전히 고장난다.

그러니깐
```csharp
public class Player : MonoBehaviour
{
    public AttackType atkType; // 최상단에 있음
    
    [ShowIf("atkType == Magic")]
    public float mana;         // 최상단에 있음
}
```
이 경우에는 작동한다.

그런데 만일

```csharp
public class GameManager : MonoBehaviour
{
    // GameManager 최상단에는 몬스터 '배열' 하나만 존재함
    public List<Monster> monsters; 
}

[Serializable]
public class Monster
{
    public AttackType atkType; 
    
    [ShowIf("atkType == Magic")]
    public float mana; 
}
```

이런 경우라면? 작동 안한다.

우선 게임 매니저를 한번 보자.
배열 자체의 경로: monsters
첫 번째 몬스터의 0번째 칸: `monsters.Array.data[0]`
첫 번째 몬스터의 공격 타입: `monsters.Array.data[0].atkType`
첫 번째 몬스터의 마나:` monsters.Array.data[0].mana`

그럼 여기서 Monster가 ShowIf를 통해 atkType을 찾을때
`return property.serializedObject.FindProperty(atkType);`
이걸로 찾는다고 한다면?
실제로 GameManager에서 MonoBehaviour가 붙어있으므로, 이게 인게임 오브젝트에 붙게되고, 이 인스펙터에서 List로 선언된 배열을 그려줄 것이다.
GameManager 스크립트 최상단에 atkType 이라는 이름으로 선언된 변수를 찾는다.
그러면 GameManager는 다음과 같은 상황을 기대한다.

```csharp
// 단순화된 코드가 기대하는 GameManager의 모습 (클래스가 꿈꾸는 도원향)
public class GameManager : MonoBehaviour
{
    public List<Monster> monsters; 
    
    public AttackType atkType; // <--- 나!!!!!! 이거!!!!!!!! 찾는다아!!!!!!
}
```

그런데 지금 설계된건 AttackType이 monster 내부에 존재하니깐 이건 찾을 수 없다.
즉, 다음과 같이 된다
1. 첫 번째 몬스터의 mana를 그릴 차례
2. 단순화된 코드가 최상위 객체(GameManager)에게 여기 atkType 있음??
3. GameManager는 나한테는 monsters라는 배열 하나밖에 없는데? atkType이 뭔데 씹덕아 -> 못 찾음 (null 반환) -> 에러 발생

그러면 FindPropertyRelative 이제 다시금 살펴보자. 다시금 다음 클래스 두개가 있다고 가정하자.

```csharp
public class GameManager : MonoBehaviour
{
    // GameManager 최상단에는 몬스터 '배열' 하나만 존재함
    public List<Monster> monsters; 
}

[Serializable]
public class Monster
{
    public AttackType atkType; 
    
    [ShowIf("atkType == Magic")]
    public float mana; 
}
```

함수를 단위 하나씩 뜯어보자.

```csharp
private SerializedProperty FindPropertyRelative(SerializedProperty property, string fieldName)
```
`property`: 찾고자 하는 마나변수(mana)
`fieldName`: 최종적으로 찾을 목적지 "atkType"

```csharp
var propertyPath = property.propertyPath;
```
가장 먼저, 현재 변수(mana)가 유니티 내부적으로 어떤 주소를 가지고 있는지 문자열로 뽑아온다.
-> `monsters.Array.data[0].mana`

```csharp
var dotIndex = propertyPath.LastIndexOf('.');
```
유니티의 주소는 마침표(.)로 폴더(계층)를 구분하므로, 가장 마지막에 있는 마침표의 위치를 찾는다.
-> `monsters.Array.data[0].(이 부분을 찾는다.)mana`

```csharp
var conditionPath = dotIndex < 0 ? fieldName : propertyPath.Substring(0, dotIndex) + "." + fieldName;
```
만약 마침표가 없다면 (dotIndex < 0), 내가 최상위 변수라는 뜻이므로 그냥 찾으려는 이름(fieldName)이 곧 주소
마침표가 있다면, 마침표 앞부분까지의 문자열을 싹둑 잘라낸다
-> `monsters.Array.data[0]`

알아낸 부모 주소 뒤에 마침표(.)를 찍고, 찾고 싶었던 형제 이름("atkType")을 이어 붙인다.
-> `monsters.Array.data[0].atkType`

```csharp
var foundProp = property.serializedObject.FindProperty(conditionPath);
return foundProp;
```
이제 완성된 완벽한 절대 경로(`monsters.Array.data[0].atkType`)를 최상위 객체(`serializedObject`)의 `FindProperty`에게 넘겨준다.
유니티는 넘겨받은 경로를 따라 트리 탐색을 한다.

즉 정리해보면
1. 유니티 엔진 내부(C++ 영역)에서 전달받은 문자열을 마침표(.)를 기준으로 조각조각 낸다.
2. 최상위 GameManager 객체의 직렬화 데이터(메모리 덩어리)를 뒤져서 "monsters"라는 이름을 가진 자식 속성(Property) 노드를 찾는다.
3. 그 노드가 가진 메모리 주소(포인터)를 타고 들어가서 "Array"라는 구조를 찾는다.
4. 배열 구조 안에서 0번째 인덱스에 해당하는 데이터 노드로 메모리를 건너뛴다 (data[0])
5. 마지막으로 그 데이터 노드 안에서 "atkType"이라는 이름을 가진 필드의 메모리 주소를 찾아내어 반환한다.

## SerializableDictionary
> Editor -> SerializableDictionaryDrawer.cs의 코드 주석에도 설명이 첨부되어 있습니다.

기본적으로 유니티의 직렬화는 직선적인 형태(1차원 배열, List)만 이해할 수 있다. 그래서 그냥 public Dictionary<int, string> myDict; 라고 쓰면 인스펙터에 아무것도 뜨지 않고, 프리팹으로 저장도 안 된다.
그럼 다음과 같이 생각해볼 수 있다.
`딕셔너리를 통째로 저장하지 말고, 키(Key)만 모아둔 리스트와 값(Value)만 모아둔 리스트 2개로 분해해서 유니티를 속이자!`
즉, 1차원 직렬화 2개를 써서 딕셔너리 인 것 처럼 속이는 것이다.

`ISerializationCallbackReceiver`: 인터페이스는 유니티가 이 객체를 저장(Serialize)하기 직전과 불러온(Deserialize) 직후에 내가 원하는 코드를 가로채서 실행할 수 있게 해주는 인터페이스입니다.

`List<TKey> keys / List<TValue> values`: 각각 실제 인스펙터에 노출되고 하드디스크에 저장될 두 개의 List이다. 당연히 개별 직렬화를 위해서 각각 `[SerializeField]`를 부착해두었습니다.

간단하게 생각해서, List 두개를 딕셔너리 처럼 보이게 만들고, 저장하면 해당 딕셔너리로 쓸 수 있게 만들어주고있습니다.

## MinMaxSliderAttribute
> 간단하게 유니티에서 슬라이더를 생성하고, 양쪽에서 조절이 가능하며 추가로 숫자까지 넣을 수 있게하는 인스펙터 확장입니다.

## DynamicFillOriginAttribute
> 간단하게 유니티의 FillMethod에 따라 FillOrigin을 드롭박스 형태로 보여줄 수 있는 Drawer입니다.
