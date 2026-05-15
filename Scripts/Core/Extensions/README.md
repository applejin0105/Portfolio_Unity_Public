### Extensions
> **요약:** 프로젝트 진행 중 필요해서 만든, 자주 사용하는 기능들을 분리해서 만든 Extension들입니다.

## CameraExtension
> **요약:** 카메라의 상태를 하나의 데이터 덩어리로 묶어서 관리하고, 카메라 간의 부드러운 전환을 쉽게 만들어주는 유틸리티입니다.

`CameraConfig`: 카메라가 가져야 할 필수 설정값을 담는 컨테이너 역할을합니다. 프로젝트에서 사용하기 위해 필요한 것만 뽑았습니다.
  -  `isLocal`: 카메라가 부모 오브젝트에 종속된 로컬 좌표계를 쓸지 원드 좌표계를 쓸지 구분
  -  `isOrthographic`/`fieldOfView`/`orthographicSize`: 원근 투영, 직교 투영 데이터를 담아 2D와 3D 시점을 모두 커버
`Lerp`: 두 개의 카메라 설정 사이를 t 값(0~1)에 따라 부드럽게 섞어 전환시키는 역할을 합니다.
  - 회전 값을 보간할 때, 오일러 각도를 그대로 섞지 않고 `Quaternion.Lerp`를 사용한 뒤 다시 오일러 각도로 변환하였습니다. 이는 그냥 오일러 각도를 Lerp해버리면 짐벌락이나 엉뚱한 방향으로 섞이는 경우가 있었기에 이렇게 구현했습니다.
```csharp
        public static CameraConfig Lerp(CameraConfig a, CameraConfig b, float t)
        {
            var result = new CameraConfig();
            result.isLocal = b.isLocal;
            result.isOrthographic = b.isOrthographic;

            result.position = Vector3.Lerp(a.position, b.position, t);
            result.rotation = Quaternion.Lerp(Quaternion.Euler(a.rotation), Quaternion.Euler(b.rotation), t)
                .eulerAngles;
            result.fieldOfView = Mathf.Lerp(a.fieldOfView, b.fieldOfView, t);
            result.orthographicSize = Mathf.Lerp(a.orthographicSize, b.orthographicSize, t);

            return result;
        }
```
- `CameraExtension`: 유니티 내장 `Camera` 클래스에 커스텀 기능을 추가하는, **확장 메서드** 패턴을 사용했습니다.
  - 이를 통해, 외부 스크립트에서 카메라에 접근할 때, camera.ApplyConfig(myConfig)와 같이 직관적으로 사용 가능하게 설계했습니다.
  - 또한 `config.isLocal` 여부에 따라 `localPosition/localRotation`을 쓸지, `position/rotation`을 쓸지 자동으로 분기해주며, 투영 방식(Orthographic vs Perspective)에 따라 필요한 카메라 설정값만 정확하게 적용시키고 있습니다.
  - 내부에 구현한 림버스 전투 형태를 최대한 따라해보고자 구현할 때, 편의를 위해 만들었습니다.

## ColorExtensions
> **요약:** HEX 문자열을 Color로 변환합니다. `"#FFFFA1".ToColor` 형태로 사용합니다.

## CustomCursor
> **요약:** 프로젝트에서 사용하는 커서를 변경시켜주는 Extension입니다. 버튼을 눌렀을때, 움직일 때 모두 적용시키고 있습니다.

## DrawColliderGL
> **요약:** Collider를 그리는 Extension입니다. Battle Scene에서 플레이어가 드래그 엔 드랍할 때 3D 공간 **어디에** 내려 놓을 지 시각적으로 표시해줍니다.
> 다른 프로젝트에서 구현했던 (DrawCollider)[https://github.com/applejin0105/Hitbox-Maker/blob/main/Unity/DrawCollider.cs]를 일정부분 수정했습니다.

  - 변경점
    - Gizoms -> GL
      - 에디터 종속성 탈피: 기존 Gizoms는 에디터에서만 작동합니다. 해당 DrawCollider를 사용하는 프로젝트에서는 에디터 모드에서만 사용하면 되었지만, 지금은 인게임에서 실시간으로 보여줘야 하기 때문에 실제 GPU에 직접 선을 그리라고 명령 할 수 있는 저수준 API인 GL을 사용하였습니다. 이를 통해 빌드된 게임(Play Mode)에서도 콜라이더를 렌더링 할 수 있습니다.
      - 렌더 파이프라인 호환성 확보: 기존 DrawCollider는 Built-in 파이프라인에만 종속적으로 사용했습니다. 하지만 이번에는 URP도 enum을 통해 선택해서 적용 가능하도록 설계했습니다. 다른 프로젝트에서도 사용하게 된다면 유동적으로 사용하기 위해 구현했습니다. 이를 통해 최신 URP를 쓰는 프로젝트든, 옛날 Built-in 게임이든 다 사용 가능합니다.
      - 드로잉 함수 수동 구현: 이렇게 되면 Gizmos에서 제공하던 DrawWireCube나 DraWireSphere같은 함수를 사용할 수 없으므로, 점과 점을 잇는 수학적 계산으로 모든 도형을 직접 와이어 프레임으로 구현했습니다.

```csharp
    public enum RenderPipelineType
    {
        BuiltIn,
        URP
    }

        // Built-in 렌더링 콜백
        private void OnRenderObject()
        {
            if (pipelineType == RenderPipelineType.BuiltIn)
                if (Camera.current != null &&
                    (Camera.current == Camera.main || Camera.current.cameraType == CameraType.SceneView))
                    RenderColliders();
        }

        // URP 렌더링 콜백
        private void OnEndCameraRendering(ScriptableRenderContext context, Camera cam)
        {
            if (pipelineType == RenderPipelineType.URP)
                // 메인 카메라(게임 뷰) 또는 씬 뷰 카메라일 때만 렌더링 (UI 전용 카메라는 무시)
                if ((cam.cameraType == CameraType.Game && cam == Camera.main) || cam.cameraType == CameraType.SceneView)
                    RenderColliders();
        }
```

  - GL은 기본적으로 GPU에 할당하는 명령이므로, 카메라가 화면을 다 그린 직후에 덧그려야한다. 이를 Built-in에서는 `OnRenderObject()`를 사용해서 씬 뷰나 메인 카메라일 때만 그리도록 제어했습니다.
  - URP에서는 RenderPipelineManager.endCameraRendering 이벤트에 OnEndCameraRendering 메서드를 구독(Subscribe)시켜, URP 환경에서도 카메라 렌더링이 끝난 직후 정확한 타이밍에 선을 그리도록 로직을 분기했습니다.
  - 최종적으로 선 렌더링을 해야하는데, GL로 선을 그리려면 화면에 '어떻게' 그릴지(투명도, 컬링 여부 등)을 정의하는 메테리얼이 꼭 필요합니다.
    - `Hidden/Internal-Colored` 쉐이더를 동적으로 찾아 머티리얼을 생성했습니다.
    - `_ZWrite`를 0으로 설정하고, `BlendMode`를 조작하여 콜라이더 선이 불투명한 오브젝트를 가리지 않고 반투명하게 자연스럽게 겹치도록(Alpha Blending) 세팅했습니다.
      
```csharp
        private void OnEndCameraRendering(ScriptableRenderContext context, Camera cam)
        {
            if (pipelineType == RenderPipelineType.URP)
                // 메인 카메라(게임 뷰) 또는 씬 뷰 카메라일 때만 렌더링 (UI 전용 카메라는 무시)
                if ((cam.cameraType == CameraType.Game && cam == Camera.main) || cam.cameraType == CameraType.SceneView)
                    RenderColliders();
        }
```

  - 이렇게 로컬 좌표계 기준으로 정점을 계산한 뒤, `GL.MultMatrix(col.transform.localToWorldMatrix)`를 통해 한 번에 월드 좌표와 회전/스케일 값을 적용하는 방식을 사용하여 연산량을 최소화했습니다.
    - BoxCollider는 center와 size를 기반으로 8개의 꼭짓점을 구하고, 이들을 12개의 선으로 그어 직육면체를 만들었습니다.
    - SphereCollider는 삼각함수(Mathf.Cos, Mathf.Sin)를 사용해 XY, XZ, YZ 3개의 평면에 각각 원을 그려 구체를 시각화했습니다. (`DrawCircle` 메서드 활용)
    - CapsuleCollider는 방향(`direction`)에 따라 기준축을 잡고, 양 끝에 반구를 그린 뒤, 그 사이를 4개의 직선으로 이어 캡슐 형태를 완벽하게 재현했습니다.
    - MeshCollider는 메쉬의 `vertices`(정점)와 `triangles`(인덱스) 배열을 가져와 3개의 점을 하나의 삼각형으로 이어주도록 구현했습니다.
- 현재는 만일, 폴리곤이 많은 메쉬를 `MeshCollider`로 사용할 경우, 매 프레임마다 수천~수만 개의 선을 GL.LINES 루프를 돌며 그리게 되므로 FPS가 엄청나게 떨어질 수 있어서 이를 사용하는건, 특히 모바일 환경에서는 좋지 않습니다.

- 애초에, 대체방법으로 DropZone에 UI 이미지를 씌우거나 별도의 3D 오브젝트를 만들거나, 3D공간에 UI를 놓으면 되면 훨씬 괜찮으므로... 이건 사실상 그냥 개발용으로 편하게 구현한 임시방편 코드입니다.

## EmptyGraphic
> **요약:** 레이케스트만 가능하게 만드는 비어있는 그래픽

## FixedAspectRatio
> **요약:** 화면을 항상 16:9 비율로 유지시켜 주는 Extension입니다. 모든 Scene이 load 될 때, 그리고 카메라가 변경 될때를 감지해서 자동으로 언제나 16:9 비율로 유지시켜줍니다.
- 싱글톤 패턴과 `DontDestroyOnLoad`를 통해서, 하나만 사용 가능하게 구성했습니다.
- `SceneManager.sceneLoaded`를 구독하여, `FindMainCameraAndUpdate()`를 호출해서 씬이 새로 로드될 때마다 씬 안에 MainCamera 태그를 가진 카메라만 있다면 알아서 찾아서 화면 비율을 적용합니다.
- 또한 Update 내에서 계속해서 비율 유지를 시키는게 아니라, `_lastWidth`와 `_lastHeight`를 캐싱해두고, Update문에서 현재 화면 크기와 비교합니다. 그리고 **화면 크기가 변경**될 때만 UpdateCameraRect()를 호출해서 매 프레임 발생하는 연산 낭비를 줄이고, PC에서 실시간으로 드래그해서 화면 비율을 바꾸거나, 모바일 기기 특성상 화면 크기가 다른 경우를 대비했습니다.

## GraphicExtensions
> **요약:** 유니티에서 그래픽의 알파값을 쉽게 변화시킬 수 있는 Extension입니다.

```csharp
image.color.a = 0.5f; // 컴파일 에러

Color tempColor = image.color; // 1. 기존 색상을 임시 변수에 담고
tempColor.a = 0.5f;            // 2. 임시 변수의 알파값을 수정하고
image.color = tempColor;       // 3. 수정한 색상을 다시 덮어씌움
// 이렇게 해야 알파값이 적용 
```

- 일단 이런 현상이 왜 일어나는지 알려면 구조체의 타입과 프로퍼티에 대해 알아야합니다.
- 우선, C#에서는 기본적으로 데이터를 저장하는 형태가 `Value Type` 그리고 `Reference Type` 두 가지로 나뉩니다.
  - Value Type: int, float, Vector3, Color, Struct 등
  - Reference Type: C++로 치면 포인터로, 실제 데이터가 저장된 메모리 공간의 주소를 저장하는 데이터 타입입니다. C#에서는 클래스, 배열, 문자열 등이 이에 속하며, 별도의 기호 없이 변수명만으로도 자동으로 참조로 동작합니다. (C++에서는 &을 사용하는데 햇갈리지 마십시오 휴-만)
 
- 값 타입의 가장 큰 특징은, 데이터 자체가 변수에 직접 저장되며, 주로 **Stack** 메모리에 할당된다는 점입니다. 변수를 다른 변수에 대입하거나 함수의 인자로 전달할 때, 참조가 넘어가는 것이 아니라 메모리상의 데이터 자체가 통째로 복사(Bitwise Copy)됩니다.
- 레퍼런스 타입은 반대로, 원본을 주고받습니다.

```csharp
public struct Point 
{
    public float x;
    public float y;
}

// 1. 값 타입의 동작 (Struct)
Point p1 = new Point { x = 10, y = 10 };
Point p2 = p1;  // p1의 데이터가 p2로 '복사'됨 (별개의 메모리 공간)

p2.x = 20;      // p2를 수정해도
Console.WriteLine(p1.x); // p1.x는 여전히 10입니다. 완전히 독립적입니다.

// ---------------------------------------------------------
// (비교) 참조 타입의 동작 (Class)
public class PointClass { public float x, y; }

PointClass c1 = new PointClass { x = 10, y = 10 };
PointClass c2 = c1; // c1이 가리키는 힙(Heap) 주소의 '참조'만 복사됨

c2.x = 20;
Console.WriteLine(c1.x); // c1.x도 20으로 바뀝니다. 같은 객체를 가리키기 때문입니다.

// class 키워드를 사용하여 레퍼런스 타입 정의
public class PlayerStat 
{
    public int hp;
}

public void TestReferenceType()
{
    // 1. 메모리 할당
    // new 연산자를 통해 힙(Heap) 메모리에 실제 PlayerStat 객체가 생성됩니다.
    // 변수 p1은 스택(Stack)에 생성되며, 방금 힙에 만들어진 객체의 '메모리 주소'를 가집니다.
    PlayerStat p1 = new PlayerStat();
    p1.hp = 100;

    // 2. 참조 복사 (Reference Copy)
    // p1이 가진 '메모리 주소'만 p2로 복사됩니다. 
    // 즉, p1과 p2는 힙에 있는 완벽히 동일한 객체를 가리킵니다. (데이터는 1개, 포인터가 2개)
    PlayerStat p2 = p1;

    // 3. 데이터 수정
    // p2를 통해 힙에 있는 원본 데이터의 hp를 수정합니다.
    p2.hp = 50;

    // 4. 결과 확인
    // p1과 p2는 같은 곳을 바라보고 있으므로, p1의 hp도 50으로 변경되어 있습니다.
    Debug.Log(p1.hp); // 출력: 50
    Debug.Log(p2.hp); // 출력: 50
}
```
- 결론적으로, 값 타입을 다룰 때는 항상 원본이 아닌 독립된 복사본을 주고받습니다. 하지만 레퍼런스 타입의 경우 항상 원본의 주소를 주고 받습니다.

- 본론으로 돌아와서, Color는 클래스가 아닌 구조체로, 원본이 아닌 복사본을 주고 받습니다.

```csharp
// 유니티 내부의 Graphic 클래스 (대략적인 구조)
public class Graphic
{
    private Color m_Color; // 진짜 데이터 (숨겨져 있음)

    public Color color     // 우리가 접근하는 프로퍼티
    {
        get { return m_Color; } // 데이터를 복사해서 건네줌!
        set { m_Color = value; UpdateGeometry(); }
    }
}
```
- 그러면 `image.color.a = 0.5f;`를 호출하면 다음과 같이 작동할겁니다.
  - `image.color` 호출: 프로퍼티의 `get` 실행, 원본 컬러의 '복사본'이 생성
  - `.a = 0.5f` 적용: '복사본'의 알파 값을 0.5로 변경
  - 원본에 아무 변경 없음. -> C# 컴파일러에서 허접허접 그거 아무것도 못하는데~♡ 호출
- 그럼 class로 만들면 되는거 아님???? -> 게임에서는 매 프리엠 수만 개의 색상과 위치 데이터가 생성되고 지워지는데 이걸 전부 클래스로 만들면 GC가 죽어나간다...
  
- 그러면 다음과 같이 생각할수 있습니다 (내가 그랬다)
```csharp
Color tempColor = image.color;
```
- 이것도 결국 복사본 생성이라 의미 없는거 아닌가???
- 생각을 좀 깊게 해보면 `Color tempColor = image.color;`는 원본 색상 데이터의 복사본을 생성하고, 그걸 `tempColor`에 담습니다.
- 그럼 이제 image.color아 tempColor는 한때의 불장난이었지만, 지금은 그냥 아무 사이가 아니게 되어버립니다.
- 거기에 `Color` 구조체 내부를 보면 `a`, `r`, `g`, `b`는 프로퍼티가 아니라 단순한 공용 변수라서 단순 변수에 접근해서 수정하는건 가능합니다.
- 그리고 마지막으로 `image.color = tempColor;`에서는, `image.color`의 Set을 호출하여 설정하기 때문에 가능한겁니다.

- 정리해보면, "클래스의 **프로퍼티(Property)**를 통해 구조체 데이터를 편집하려면, 프로퍼티의 `get`을 통해 원본을 복사해오고, 내 변수에서 그 복사본의 내부 값을 수정한 다음, 클래스의 프로퍼티 `set`에 그 완성본을 통째로 던져주어 원본을 덮어씌워야 합니다.

## IntersectionCalculator
> **요약:** 두 물체가 만나는 중간 지점을 계산합니다. 림버스 컴퍼니 전투에서 보면, 타겟팅된 두 캐릭터가 접근하여 어느정도 중간 지점에서 만나고 있습니다. 이를 수학적 계산을 통해 만나는 시간과 지점을 계산하였습니다.

- 다만, 환상체의 경우 간혹 이동하지 않는 경우도 있는 것은 배틀 로직에 캐릭터나 적의 '무게'를 넣었는데, 이를 활용하여 제작해보았습니다.
- 물리 시스템을 구성하여, 무거우면 느리게 움직이거나 거의 움직이지 않고, 그에 따라 상대적으로 가벼운 쪽은 튕겨나가는게 많게끔 설계했습니다.
