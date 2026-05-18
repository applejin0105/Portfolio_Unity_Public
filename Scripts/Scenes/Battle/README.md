# Battle Scene
> **요약:** 오토체스와 림버스 컴퍼니 전투 방식을 결합하여 만든 게임

<img width="800" height="450" alt="BattleEnter" src="https://github.com/user-attachments/assets/99485910-5877-4b71-9991-6a4cd187c5f0" />

<img width="800" height="442" alt="Battle" src="https://github.com/user-attachments/assets/1568ff3d-bb7c-497f-b8b2-824a7630e3d6" />


## 개요
로드맵까지 끝내고 나자 아쉬웠습니다. 게임 개발자로 취업하고자 한다면, 게임이 있어야 하지 않을까? 라는 안일한 생각의 시작으로 **어떤**게임을 만들까 고민했습니다. 예전에 물리까지 전부 구현해서 만든 수박게임을 만들까? 하다가, 생각을 바꾸었습니다. 최근 가장 많이하고 오래 한 게임을 이용해서 게임을 만들어보자! 그게 시작이었습니다.

## 목표
게임을 제작하기 전 다음과 같은 목표를 잡았습니다.

1. 현재 제작중인 **Cultist**에서도 이를 활용할 수 있어야한다. 즉, 재사용성을 염두에 두어야한다.
2. 게임 시스템을 그대로 베끼는게 아닌, 오리지널리티를 추가해야한다.
3. 단순히 프론트엔드만 비슷하게 만드는게 아닌, 실제 게임이 돌아가는 로직이어야한다.
4. '재미'가 있어야한다. 특히 '타격감'과 '내가 생각하는 연출'을 '제대로 구현'하는게 중요하다.

이렇게 목표를 잡고, 어떤 게임을 만들지 고민했습니다. 림버스 컴퍼니 2482시간(스팀 기준, 모바일 합산시 +α), LV.115의 업적(?)을 보유하고 있으므로, 그리고 프로젝트 문이라는 회사를 목표로 하고 있는 만큼, 당연히 베이스는 림버스 컴퍼니로 잡았습니다. 그렇다면 문제는 이 림버스 컴퍼니로 무엇을 만들까? 였습니다.

<img width="1907" height="991" alt="개요" src="https://github.com/user-attachments/assets/61cff4a1-40ef-4a33-b220-127f83e70773" />

이 즈음 '플러리'라는 하스스톤 스트리머의 방송을 몇번 보며, 고등학교때 했던 하스스톤이 정말 많이 변하기도 했고 거기에 지금 제작중인 'Cultist'라는 게임이 카드게임이므로, 림버스 컴퍼니 + 카드게임을 생각해보았습니다. 가급적이면 림버스 컴퍼니의 전투나 모션, 백엔드를 일정 부분 구현해보면서 제작중인 게임에 사용할 요소들을 미리 공부해보고 싶었습니다. 그래서 고민을 하던 중, 하스스톤의 전장과 같은 오토체스 형식에 림버스 컴퍼니의, 제가 가장 자주 사용하는 방식인 승률->딸깍 방식을 결합해서 **림토체스**를 만들기로 결정하였습니다. 지금부터 이 배틀 시스템을 구현하면서, 코드를 어떻게 짯는지 설명하겠습니다.

각 코드의 이름을 누르면 깃의 해당 코드로 즉시 이동 가능합니다.

## 게임 설계

## Chapter 1. 프로젝트 조망
> 게임의 핵심 로직 및 루프를 간단히 설명

### [`Manager/BattleSceneManager.cs`](./Manager/BattleSceneManager.cs)
> 씬 진입점

씬에 진입하는 로직을 담았습니다. 전투에 사용하는 BGM(림버스 여러 전투 BGM중 가장 좋아하는 노래들)들을 QUeue에 저장하고, 이 음악들을 재생시킵니다. 이와 동시에 씬 진입 기본 연출인 페이드인을 수행합니다.


### [`Core/BattleDirector.cs`](./Core/BattleDirector.cs)
> 게임 루프 관리(라운드·턴·스폰·경제·게임오버) *현재는 간단하게 흐름만 살펴보고, 이부분은 추후 Chapter 9 마지막에서 다시 다루겠습니다.*

게임의 장르는 **림버스 컴퍼니의 전투 방식을 사용한, 2.5D 오토배틀러**입니다.

BattleDirector는 라운드/턴/스폰/경제/승패를 호출하는 게임 Director로, 다음과 같은 작업이 진행됩니다.
1. 유닛 배치
2. 전투 시작
3. 자동 전투
4. 결산
5. 보상·리롤
6. 다음 라운드

이를 다이어그램으로 표현해보면, 다음과 같이 볼 수 있습니다.

<img width="3616" height="4388" alt="Flow" src="https://github.com/user-attachments/assets/1019207d-c47f-49a1-9bd8-a146a126cc6a" />

그리고 BattleDirector의 프로퍼티는 다음과 같습니다.

```csharp
        [Header("System Managers")]
        public BattleManager battleManager;
        public PlayerManager playerManager;
        public FieldManager fieldManager;
        public ShopManager shopManager;
        public MergeManager mergeManager;
        public HandManager handManager;
        public GameOverUI gameOverUI;
        public CameraController cameraController;
        public BattleSceneUIController battleSceneUIController;

        [Header("Rosters")]
        public List<Character> players = new();
        public List<Character> enemies = new();

        [Header("Stage & Spawning")]
        public StageData currentStageData;
        public GameObject enemyPrefab;
        [SerializeField] private int currentRoundIndex;
        [SerializeField] private Transform[] enemySlots;
        private TargetingResolver _resolver;
        public int CurrentRoundIndex => currentRoundIndex;

        private bool _isGameOver = false;

        [Header("Statistics")]
        public int TotalAlliesDead { get; private set; }

        public int TotalEnemiesKilled { get; private set; }
```

이제 본격적으로, 게임 Director를 바탕으로 인게임 내부 로직이 어떻게 구성되어있는지 설명드리겠습니다.

## Chapter 2. 데이터 토대 및 용어 사전 정의
> 게임에 사용되는 데이터, ScripatableObject 유닛, 유닛 데이터 베이스 풀링과 스테이지와 확률 데이터

### [`Data/BattleData.cs`](./Data/BattleData.cs)
> 모든 시스템의 공용 어휘

게임에서 사용되는 모든 enum과 struct를 정의해두었습니다.

1. Enums
```csharp
    public enum SkillType { Attack, Defend, Avoid, Counter }

    public enum BattleMatchType { Clash, OneSided }

    public enum CameraViewType { Start, Normal, ShowAll, FocusTarget, End }

    public enum BattleState { Idle, Execution, CheckResult }
```
- `SkillType`: 스킬 타입은 실제 인게임처럼 공격, 방어, 회피, 반격으로 준비해두었습니다. 인게임에서는 오직 공격만 존재하지만, 확장성을 고려해 선언해두었습니다.
- `BattleMatchType`: 실제 게임에서와 마찬가지로 합으로 진행할건지, 일방 공격으로 진행할건지 정의합니다.
- `CameraViewType`: 개인적으로 림버스를 플레이하며 늘 신기하게, 그리고 로직을 궁금해하면서 봤던 '카메라 시스템'을 구현하고자 선언한 카메라 타입입니다. 게임 시작 시 카메라의 위치, 평상시, 모든 플레이어와 캐릭터를 보여야 할 때, 타겟에 카메라 집중, 그리고 게임이 끝났을 때 카메라의 상태를 분리해두어 enum 기반 호출로 카메라를 자유자재로 움직일 수 있게 설계하였습니다.
- `BattleState`: 현재 게임 상태를 표시합니다. 전투를 하고 있지 않은 상태인지(`Idle`), 전투중인지(`Execution`), 결과 확인중인지(`CheckResult`) 구분하여 인게임에서 게임 상태를 받아오기 편하게 설계했습니다.

2. Struct

`Coin`
```csharp
    [Serializable]
    public struct Coin
    {
        [Header("Base Info")]
        public int index;
        public int frontValue;
        public int backValue;
        public bool isBroken;

        [Header("Action Details")]
        public string animTrigger;
        public int hitCount;
        public float hitDelay;
        [Tooltip("각 타격 후 대기할 시간을 순서대로 입력. 비워두면 기본 hitDelay를 사용.")]
        public float[] customHitDelays;
        public float attackDashDistance;
        public GameObject vfxPrefab;

        [Header("Sound Settings")]
        public BattleSoundType coinTossSound;
        public BattleSoundType attackSound;
        public BattleSoundType hitSound;

        [Header("Custom Logic")]
        public CoinActionLogic customLogic;
    }
```
림버스 전투의 꽃이라고 할 수 있는 코인 시스템입니다. 게임을 플레이 해보면서 각 코인에 '애니메이션'과 'hit'로직이 구분되어 있다고 생각했습니다. 또한 각 코인 -> 1회 타격이 아닌건 여러 전투를 통해 익히 알고 있었습니다. 즉, 코인으로 인한 타격은 키워드 활성화를 진행하고, 각 코인은 고유의 공격 애니메이션과 공격 횟수, 데미지 계산, 애니메이션등을 담고 있다고 가정하고 코인 작업을 진행했습니다.

코인의 개수는 유동적입니다. 또한 림버스 전투의 핵심인 코인 파괴(`isBroken`)와 앞(`frontValue`), 뒤(`backValue`) 값에 의한 값 추가 필드를 추가해두었습니다. 인스펙터에서 이를 통해 코인 개수를 자유롭게 조절하고, 이에 맞는 애니메이션 vfx를 할당하고, 그 애니메이션에 맞는 공격 횟수나 딜레이를 설정하게 해서, 코드 수정을 통한 로직 수정이 아닌, 인스펙터에서 편하게 수정하게끔 설계했습니다. 여기에 코인이 토스될 때 나는 띵 소리와, 해당 코인의 공격 사운드, 피격 사운드를 별도로 구분했습니다. 피격 사운드까지 구분할 필요는 없었지만, 각 코인마다 피격 사운드가 다르면 추후에 다른 방식으로 사용할 수 있지 않을까, 하는 생각으로 넣었습니다.

또한 hitDelay와 customHitDelays를 넣어서 공격 딜레이, 그러니깐 다단 히트 사이사이 시간을 자유롭게 조절할 수 있게 구현했습니다.

`vfxPrefab`은 코인 1개가 타격될 때 생성할 이펙트 프리팹 참조입니다. 이는 추후 `BattleManager.ExecuteMultiHitAttack`에서 사용합니다. 일단 피격과 공격시 vfx를 할당할 수 있게 해두었습니다.
현재는 생성 위치가 단순히 공격자 위치이기 때문에, 복잡한 vfx 연출은 불가능하지만, 설계 당시 나중에 이펙트도 넣을 수 있어야 하겠다라는 생각으로 설계했습니다.
```csharp
if (coin.vfxPrefab != null)
    Instantiate(coin.vfxPrefab, attackerSlot.Owner.transform.position, Quaternion.identity);
```

`customLogic`은 `CoinActionLogic`에 대한 참조, Strategy 패턴의 주입 지점입니다. 이 로직 자체는 특별한 3스킬을 위해 구성했습니다. 림버스 전투를 보면, 3스킬 사용 시 캐릭터에게 포커싱되며 다채로운 연출을 하는 것을 보고, 이 로직을 추가했습니다.
`ExecuteMultiHitAttack`의 다음 분기에서 이를 중심적으로 처리합니다.
```csharp
if (coin.customLogic != null)
    yield return StartCoroutine(coin.customLogic.Execute(attacker, target, coin, totalDamage, isFocus));
else
{
    // 내장 기본 타격 루프 (hitCount만큼 균등 분할 대미지)
}
```
   - `customLogic`이 존재하면 특별한 3스킬을 수행합니다.
   - 없다면, 기존 `BattleManager`에 하드코딩된 기본 루프로 폴백합니다.
   - vfxPrefab과 마찬가지로, 새 공격 패턴을 만드려면 코드를 수정할게 아니라 SO 하나 만들어서 할당만 하면 됩니다.

`Skill`
```csharp
   [Serializable]
    public struct Skill
    {
        [Header("Skill Identity")]
        public string skillName;
        public Sprite skillIcon;
        public Color skillColor;
        public bool isFocusSkill;

        [Header("Clash Animation Trigger")]
        public string clashAnimTrigger;

        [Header("Power Settings")]
        [Range(0f, 100f)] public float probability;
        public int basicValue;
        public bool sign;

        [Header("Coins")]
        public List<Coin> coins;
    }
```
코인이 실질적인 데이터를 담고 있다면, 스킬은 그 데이터를 묶어 관리합니다. 각 스킬은 스킬 이름, 아이콘, 스킬 색상을 가지고 있고 3스킬인 경우 focusSkill인지 여부를 검토합니다. 또한 애니메이션에 연결된 합 애니메이션 트리거가 존재하여 스킬별로 별도의 합 애니메이션을 재생할 수 있게 구성하였습니다.
그 다음으로 해당 스킬이 작동할 때, 코인을 몇 퍼센트의 확률로 앞면 혹은 뒷면을 발화할지, 스킬이 가지는 기본 값과 이 스킬이 앞면용 스킬인지 뒷면용 스킬인지 sign으로 구분하고 있습니다. 다만 여기에서 sign은 직접적인 연산 보다는 (연산은 코인의 front와 back value가 계산해주므로) UI 표기 플래그로 사용하고 있습니다.

`BattleMatchup`
```csharp
    public struct BattleMatchup
    {
        public ActionSlot AttackerSlot;
        public ActionSlot DefenderSlot;
        public BattleMatchType MatchType;
        public CameraViewType ViewType;
    }
```
BattleMatchup은 전투 1건이 가지는 정보를 정리하고있습니다. 한 턴에 여러 교전이 발생하는데, 기본적으로 가중치가 2이상인 스킬이 아니라면 1대1이 중심이고, 아직 1대n 공격을 구현할 기술력은 부족해서 1대1 정보 수집에만 포커싱을 두었습니다.

***(가중치가 2이상이어도 합은 하나의 적과만 합니다만, 생각해보면 다중 가중치 스킬이 여러명과 동시에 합을 진행해도 재미있을 것 같습니다. 가중치가 n인 스킬이 m인원과 합을 진행할 때, m인원은 가중치를 일정 비율로 합해서 계산하고, 가중치 n 스킬을 쓰는 캐릭터는 특별 효과나 버프로 가중치 합 위력 증가시켜서 전투하면 재미있을 것 같습니다!!! 만일 이런걸 구현해볼 기회가 있다면 꼭 해보고 싶습니다.)***

그리고 여기에 선언된 ActionSlot은 다음과 같이 정의되어있습니다.

```csharp
    public class ActionSlot
    {
        public Character Owner { get; set; }
        public Skill SelectedSkill { get; set; }
        public ActionSlot TargetSlot { get; set; }
    }
```

액션 슬롯은 캐릭터가 이번 턴에 하는 행동 1번을 뜻합니다. 추후에 설명하겠지만 `GenerateSlots`을 보면 다음과 같이 구성되어 있습니다.

```csharp
for (int i = 0; i < character.ActionSlotCount; i++)
    slots.Add(new ActionSlot { Owner = character, SelectedSkill = 랜덤스킬 });
```

이는 ActionSlotCount가 2개라면 한턴에 2번의 행동이 가능하고, 3개라면 3번 행동이 가능함을 뜻합니다. 기존 림버스와는 다르게, 성급(star)에 따라서 차별점을 이 스킬에 두었습니다. 림버스 시스템에서도 각 스킬의 개수가 다르고, 이 스킬이 랜덤하게 표시됩니다. 따라서 저도 여기에서 이와 비슷한 방식을 구현하려고 했습니다. 1성인 경우 액션 슬롯을 1개, 2성은 2개, 3성은 3개로 두어 3성일 때 3스킬이 해제되는 방식으로 구성했습니다.

전투는 기본적으로 다음과 같이 이루어집니다.
   1. 속도 굴림: 모든 생존 캐릭터(적 포함)가 해당 턴에 랜덤으로 Speed를 뽑아 돌립니다. (`RollBattleSpeed`)
   2. 슬롯 생성: 플레이어·적이 각각 ActionSlot 리스트를 생성합니다.
   3. 적 행동: 적이 먼저 아무나 찍습니다. `foreach enemy in enemySlots: enemy.TargetSlot = playerSlots[랜덤]`
   4. 플레이어 슬롯을 속도 내림차순으로 정렬합니다.
   5. 플레이어가 "합 상대"를 정해서 잠가버립니다.
      1. 이때 합이 가능한 후보가 있다면 그 중 하나를 랜덤으로 선택하고 `pSlot.TargetSlot = eSlot; eSlot.TargetSlot = pSlot;`를 통해 상호 링크합니다.
      2. 합이 가능한 후보가 없다면 (주로 이미 합을 진행한다고 정해졌거나, 상대의 속도가 나보다 빨라서 합을 못하는 경우) 아무나 일방적으로 겨룹니다.
      3. 즉, 기존 림버스 시스템인 자기보다 느리거나 같은 적만 합으로 붙잡고, 내 속도가 적보다 느려서 적이 없거나 이미 합을 채갔으면 합은 생략하고 일방적으로 줘팹니다.
    6. 이렇게 나온 결과를 `BattleMatchup`으로 변환합니다.
        ``` csharp
        if (defenderSlot.TargetSlot == pSlot)   // 내 타겟도 나를 가리킴 = 상호 = 합
            → BattleMatchup { MatchType = Clash }
        else                                     // 내 타겟은 딴 데 봄
            → BattleMatchup { MatchType = OneSided }
        ```

        ``` csharp
        if (eSlot.TargetSlot.TargetSlot != eSlot)   // 상호가 아님 = 합이 아님
            → BattleMatchup { MatchType = OneSided }   // 적 → 타겟 일방공격 추가
        // 상호(== eSlot)면 이미 플레이어 루프에서 Clash로 추가됐으니 건너뜀
        ```

이 코인과 스킬들을 구조체로 선언한 이유는 GC 성능을 고려해서입니다. 클래스로 선언하면 인게임 중에 코인이 여러개 생성되고 유닛이 변경될때마다 새로이 코인을 지우고 생성해야하는데 이는 GC 성능에 악영향을 끼치기 때문에 구조체로 선언하였습니다.

하지만, Action Slot은 class로 선언했습니다. **이는 구조체는 자기 자신의 타입의 필드를 가질수가 없기 때문이고, ActionSlot은 그래프 노드이기 때문입니다.**

추후에 설명하겠지만(그리고 이미 앞에 나왔지만), `TargetingResolver`에는 다음과 같은 코드가 존재합니다.
```csharp
    pSlot.TargetSlot = eSlot;
    eSlot.TargetSlot = pSlot;        // 둘이 서로를 가리킴 (상호 링크)
    ...
    if (pSlot.TargetSlot.TargetSlot == pSlot)   // 내 타겟이 나인지 검사
```
당연히 상호 링크, 연결을 하려면 '주소'를 공유해야하는데 구조체는 '값'을 복사해버리기 때문에 슬롯의 주소가 아닌 내용물 자체를 복사해버려서 문제가 발생합니다. 물론, IDE 자체에서, C#에서 컴파일 에러를 발생시켜 막히겠지만(`CS0523, "struct member causes a cycle in the struct layout"`), 이 점은 추가로 공부해보았습니다.

즉, 하고싶은건 서로 '참조'하고 있다는 것인데 구조체로 해버리면 그냥 값을 덮어씌우기 때문에 ActionSlot은, 공격자와 대상을 타겟팅하기 위해 반드시 클래스로 선언해야합니다!!!!


### [`Data/UnitData.cs`](./Data/UnitData.cs)
> SO 유닛 정의 + 성급(Star) 보너스 구조 정의

`UnitState`, `UnitRarity`, `UnitStats`, `SkillBonus`는 이름 그대로의 단순 데이터 묶음이라 설명을 생략합니다. 주목할 부분은 UnitData입니다.

```csharp
[CreateAssetMenu(fileName = "NewUnitData", menuName = "Battle/UnitData")]
public class UnitData : ScriptableObject
{
    // ... id, cost, rarity 등 기본 정보 ...

    public Skill skillStar1;   // 1성부터 보유
    public Skill skillStar2;   // 2성 도달 시 해금
    public Skill skillStar3;   // 3성 도달 시 해금

    public StarBonus[] starBonuses = new StarBonus[3];   // 성급별 보너스
}
```

UnitData는 Scritable Object(이하 SO)로 구성했습니다. json에 미쳐있었던 시절(아옛날이여) 이런 데이터들도 전부 json으로 처리하려고 했습니다. 실제로 Cultist에서는 SO를 일절 사용하지 않고 오직 json만을 사용했습니다. 이번에는 조금 다르게 접근해보았습니다. 유니티 자체 기능이기도 하고, 이 부분에 대해 공부해보고 싶었습니다.

SO는 유니티 Native 데이터 컨테이너입니다. 주로 게임 내에서 변하지 않는 정적 데이터를 저장하고 공유하는데 사용합니다. 그렇다면, Cultist에서도 카드들은 사실 SO로 만들어야했습니다. 다만, Cultist에서의 카드는 추후 서버 업데이트를 통해 서버에서 일괄적으로 보내야 하고, 런쳐 업데이트를 거치지 않고 카드 데이터 업데이트를 진행할 수 있어야 하므로 json으로 설정했습니다. 하지만, 이번 게임에서는 다르게 접근했습니다. 업데이트를 통해 수치가 변경되어도 문제가 없고, 무엇보다 내부 에셋을 직접 참조해야 했기에(앞선 vfx, 스킬의 각 이미지 등) json을 통한 파싱 과정을 줄이고 직접 인스펙터에서 할당할수 있게 SO로 구성하였습니다. 또한 인게임에서 몇번씩 불러와야하기에, 파싱 비용이 발생해서 상대적으로 로딩 성능이 떨어지는 JSON보다는, SO를 활용했습니다. 다음은 SO와 JSON을 간단하게 공부해서 비교한 표입니다.

| 구분         | ScriptableObject (SO) | JSON                     |
| ---------- | --------------------- | ------------------------ |
| 주요 용도      | 정적 데이터, 에셋 연결, 설정 값   | 세이브 데이터, 서버 통신, 밸런스 패치   |
| 유니티 에셋 참조  | 매우 쉬움 (직접 참조)         | 불가능 (ID/경로를 통한 간접 참조 필요) |
| 외부 툴 연동    | 어려움 (별도 툴 개발 필요)      | 매우 쉬움 (엑셀 등과 연동 용이)      |
| 런타임 데이터 저장 | 불가능 (빌드 후 초기화됨)       | 가능 (세이브/로드 기능에 적합)       |
| 로딩 성능      | 매우 빠름                 | 상대적으로 느림 (파싱 비용 발생)      |

물론, 더 중요한 이유로는 디자이너와 QA에서 직접 수치를 쉽게 조절하고 적용 가능하게 설계하는게 목표였습니다. json으로 사용해도 괜찮지만, SO가 더 직관적이기 때문에 이를 사용했습니다.

앞서 말하긴했지만, 여기에서 캐릭터의 성급에 따라 스킬 해금이 가능하도록 설계했습니다. 즉, 유닛 데이터에 이렇게 3성까지의 데이터들을 일괄적으로 담아서, 성급에 맞춰 하나하나 전부 만들지 않고 하나의 캐릭터 데이터만을 사용해서 수정과 검토가 가능하게, 디자이너와 QA에서 쉽게 접근 가능하도록 설계했습니다.


### [`Data/UnitDatabase.cs`](./Data/UnitDatabase.cs)
> 희귀도별 풀링

UnitData를 받아서 초기화합니다. 미리 만들어둔 캐릭터 SO들을 할당합니다. 여기서 사용하는 UnitDatabase는 인게임에서 유닛을 참조하는데도 사용하고, Shop에서 Unit 개별의 레어도에 따라 등장 확률을 조절하기도 합니다.

### [`Data/ShopLevelProbability.cs`](./Data/ShopLevelProbability.cs)
> 상점 확률 데이터

상점에 레벨에 따라 레어도별 유닛의 출현 정도를 나타냅니다.

### [`Data/StageData.cs`](./Data/StageData.cs)
> 스테이지 데이터

게임 스테이지 데이터입니다. 스테이지에 따라 적의 정보와 그 적의 성급을 할당할 수 있습니다.

## Chapter 3. 엔티티 계층 설계
> 캐릭터, 적, 플레이어 정의

### [`Entity/Character.cs`](./Entity/Character.cs)
> 핵심 베이스 클래스(스탯·코인·UI·애니메이션)

캐릭터는 기본적인 정보들을 담고있습니다. 직관적으로 알 수 있는 요소들 말고, 따로 설정한 프로퍼티들에 대해서 설명하겠습니다.
- `CombatRange`: 캐릭터와 적이 '충돌' 할 때, 너무 가까워져서 붙는 것을 방지하기 위해 설정한 일종의 간격입니다. 원래는 개별적으로 Collider를 붙여 충돌 감지로 연산하려고 했으나, 이는 내부 연산에 대한 부담을 줄이고, 이 부분 자체를 콜라이더 연산으로 처리하면 너무 과한 설계인것 같아 이것으로 대체했습니다.
- `Weight`: 본 게임에서는 아마 없을 것으로 추정되는 무게입니다. 게임을 하며 타격감이 굉장하다는 느낌을 자주 받았습니다. (아마 히오스로 다져진 인생이라 더 그렇게 느꼈을지도 모릅니다.) 이게 단순히 효과음 뿐 만 아니라 탕! 했을 때 서로 밀려나는 연출이나 카메라 워킹도 제 몫을 단단히 하고 있다고 느꼈습니다. 그런데 어느 순간에는 한쪽이 느리게, 한쪽이 빠르게 하는 등의 모습을 보았고, 공통적으로 '중앙'에서 만난다는 규칙을 발견했습니다. 그래서, 간단한 물리와 수학을 섞기로 했습니다. 이는 **`Chapter 5. 전투 코어`**에서 더 자세히 다루겠습니다. 이 무게는 해당 물리 연산에서 사용합니다.
- `IsEngaged`: 현재 해당 캐릭터가 전투중인지 아닌지 판별하는 프로퍼티입니다.
- `IsImmovable`: 림버스 전투에서 보면, 몇몇 환상체는 '니가 와'를 시전합니다. 그런 환상체를 위해 준비했습니다.

원래는 공통적으로 `[SerializeField]`가 아닌 `[field: SerializeField]`를 붙였습니다. 이는 프로퍼티로 유지하면서도, 인스펙터에 노출시키고 싶기 때문에 이렇게 선언했습니다. 스탯 전체를 프로퍼티로 둔 이유는 다음 장점 때문이었습니다.
  1. 읽기/쓰기 권한을 개별적으로 부여
  2. 추후 검증 로직 `ex(set => _x = Mathf.Max(0, value);)` 같은걸 넣기도 편함
그리고 이걸 다 구현하고 캐릭터 SO까지 설계를 마친 뒤, 이건 좀 아닌 것 같다는 생각을 하면서 유니티 관련 코드를 보고 자료를 조사했습니다. 당장 제 코드 여러군데를 보아도 필드 선언은 private으로, 프로퍼티 선언은 public으로 하면서 둘 다 작성한게 대부분이었습니다. 그래서, 정통 패턴으로 전부 수정하였습니다.

이 밖에는 정신력(SP)이 -45~45를 가지고 이에 따라 확률이 5~95%로 앞면이 나오게 조정해야 하므로 이를 쉽게 계산해주는 `_a`와 `_b` 필드를 두었습니다.

다음은 메서드들을 설명하겠습니다.

- `Awake`: 3D 필드를 활용하는 만큼 각 캐릭터에게 소리가 나게끔 하여 3D 사운드를 재생시키기 위해 spatialBlend와 spread를 설정하였습니다. 이때, 오디오 리스너가 중앙에 배치되어있고 이를 프리팹화해서 사용중이라 위치 조절이 번거로워 지나치게 민감한 3D를 줄이고자 각각의 값을 완화하였습니다.
- `ApplyUnitData`: 유닛 데이터를 기본 설정하는 과정에서, 현재 캐릭터의 성급을 파악하고, 1성 2성 3성에 맞춰서 보너스 스탯을 적용한 뒤, 스킬을 해금합니다. 이를 통해 스탯 증가치만 사용해서 쉽게 캐릭터 적용이 가능합니다.
```csharp
        private void ApplyUnitData()
        {
            if (currentUnitData == null) return;

            Name = currentUnitData.unitName;
            var currentBonus = currentUnitData.starBonuses[StarLevel - 1];

            Hp = currentUnitData.baseStats.hp + currentBonus.addedStats.hp;
            Sp = currentUnitData.baseStats.sp + currentBonus.addedStats.sp;
            MinSpeed = currentUnitData.baseStats.minSpeed + currentBonus.addedStats.minSpeed;
            MaxSpeed = currentUnitData.baseStats.maxSpeed + currentBonus.addedStats.maxSpeed;
            Weight = currentUnitData.baseStats.weight + currentBonus.addedStats.weight;
            Strength = currentUnitData.baseStats.strength + currentBonus.addedStats.strength;
            ActionSlotCount = currentUnitData.actionSlotCount + currentBonus.addedActionSlotCount;
            CombatRange = currentUnitData.combatRange;

            AvailableSkills = new List<Skill>();

            // 1. 1성 스킬은 무조건 해금
            AvailableSkills.Add(ApplySkillBonus(currentUnitData.skillStar1, currentBonus.addedSkillBonus));

            // 2. 2성 이상일 경우 2성 스킬 해금
            if (StarLevel >= 2)
                AvailableSkills.Add(ApplySkillBonus(currentUnitData.skillStar2, currentBonus.addedSkillBonus));

            // 3. 3성일 경우 3성 스킬 해금
            if (StarLevel >= 3)
                AvailableSkills.Add(ApplySkillBonus(currentUnitData.skillStar3, currentBonus.addedSkillBonus));
        }

        private Skill ApplySkillBonus(Skill baseSkill, SkillBonus bonus)
        {
            var upgradedSkill = baseSkill;
            upgradedSkill.basicValue += bonus.addedBasicValue;

            if (upgradedSkill.coins != null)
            {
                var upgradedCoins = new List<Coin>();
                foreach (var baseCoin in upgradedSkill.coins)
                {
                    var upgradedCoin = baseCoin;
                    upgradedCoin.frontValue += bonus.addedFrontValue;
                    upgradedCoin.backValue += bonus.addedBackValue;
                    upgradedCoins.Add(upgradedCoin);
                }

                upgradedSkill.coins = upgradedCoins;
            }

            return upgradedSkill;
        }

```

- `GetRandomSkillByProbability`: 기존 림버스 전투 시스템과 달리 스킬을 선택해서 전투가 진행되는게 아니라 전부 자동으로 진행되기 때문에 스킬도 확률에 기반하여 랜덤으로 받아옵니다. 각 스킬별로 확률 가중치를 더해서 확률을 계산하고 이에 맞추어서 선택된 스킬들을 복사해옵니다. 내부 코인을 복사할때는, 앞서 설명한 바 있지만 구조체로 선언해두어서 내부 값 까지 단순하게 `new List<Coin>(original)`만으로도 안전하게 복사가 가능하며, GC 부담을 줄였습니다.

```csharp
        public Skill GetRandomSkillByProbability()
        {
            if (AvailableSkills == null || AvailableSkills.Count == 0)
            {
                Debug.LogWarning($"[{Name}] 설정된 스킬이 없습니다.");
                return default;
            }

            var totalWeight = 0f;
            foreach (var skill in AvailableSkills) totalWeight += skill.probability;

            Skill selectedSkill;
            if (totalWeight <= 0f)
            {
                selectedSkill = AvailableSkills[Random.Range(0, AvailableSkills.Count)];
            }
            else
            {
                var randomVal = Random.Range(0f, totalWeight);
                var cumulative = 0f;
                selectedSkill = AvailableSkills[AvailableSkills.Count - 1]; // 기본값 설정

                foreach (var skill in AvailableSkills)
                {
                    cumulative += skill.probability;
                    if (randomVal <= cumulative)
                    {
                        selectedSkill = skill;
                        break;
                    }
                }
            }

            // 깊은 복사, 새로운 리스트 인스턴스를 생성하여 참조를 끊음
            if (selectedSkill.coins != null)
            {
                // Coin이 struct이므로 new List<Coin>(original)만으로도 내부 값까지 안전하게 복사.
                selectedSkill.coins = new List<Coin>(selectedSkill.coins);
            }

            Debug.Log($"[{Name}] 스킬 선택 완료: {selectedSkill.skillName}");
            return selectedSkill;
        }

```

- `ResetStateAndPosition`: 캐릭터들의 기존 위치를 초기화, 애니메이션 설정을 초기화합니다.

```csharp
        public void ResetStateAndPosition()
        {
            transform.position = HomePosition;
            transform.rotation = Quaternion.identity;
            IsEngaged = false;
            _isDead = false;
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
            if (Anim != null)
            {
                Anim.Rebind();
                Anim.Update(0f);
            }
        }
```

- `UpdateCoinUI`: 코인의 시각적 UI를 조절합니다. CoinResolver에서 코인 관련 로직을 수행하고 이 메서드로 Update된 코인의 정보를 전송합니다. 코인은 현재 1개 ~ 5개로 하드코딩 되어있습니다. 이에 맞춰서 코인 개수에 맞게 유동적으로 사용하고 있는데, 최근 출시한 인격인 검지아비 이상의 `Furioso-Replica`같은 스킬을 보면 이 코인도 유동적으로 생성되면서 다중 타겟 선택 가능한 형태로 확장하고 싶습니다.
- `OnDrawGizmosSelected`: 캐릭터의 `CombatRange`를 시각적으로 표현합니다. 디버깅용으로 사용하며, 각 캐릭터별로 지나치게 겹치는걸 방지하기 위해, 이걸 시각적으로 확인하고 구분 가능하게 사용하고 있습니다. 추후에는 각 애니메이션 별로 해당 부분을 조절하게 확장하여 손을 크게 휘두르거나 무기를 사용하거나 하는 등 '반드시 이만큼은 벌어져야 하는 정도'를 계산 가능하게 확장하고 싶습니다.

### [`Entity/Enemy.cs`](./Entity/Enemy.cs)
> 상속 + 오버라이드(환상체 느낌으로, 정신력 없는 확률 고정)
- Character를 상속받는 적입니다. 코인은 무조건 50% 확률로 고정하기 위해 `ChangeSp`를 오버라이드하며 내부를 비워두었습니다. 나머지 공격, 액션 등은 확장을 위해 남겨두었습니다.


### [`Entity/Player.cs`](./Entity/Player.cs)
> 상속 + 드래그 앤 드롭 입력 처리
- 전투 상태 여부를 이벤트로 처리하여, 전투 상태인지 아닌지 브로드캐스팅 합니다.
```csharp
        private void OnEnable()
        {
            BattleManager.OnCombatStateChanged += HandleCombatState;
        }

        private void OnDisable()
        {
            BattleManager.OnCombatStateChanged -= HandleCombatState;
        }

        private void HandleCombatState(bool isCombat)
        {
            _isInCombat = isCombat;
        }
```
- **드래그엔 드랍 로직**
  - 단순히 클릭으로만 가능한 로직이 있고, 그렇지 않은 로직이있습니다. 오토체스류 게임이라면 유닛을 구매하고 판매하는 과정은 필수입니다. 만일 이를 드래그 엔 드랍으로 구현하지 않는다면, 만일 필드에 존재한다면, 클릭 -> UI 띄우기(판매?핸드로 복귀?) 그리고 핸드에 존재한다면, 클릭 -> UI 띄우기(판매? 필드로 배치?)를 결정해야 합니다. 그럼 그만큼 UI를 그려야 하므로 GPU 낭비기도 하고, 이미 여러 게임에서 사용중인 드래그 엔 그랍을 사용하지 않으므로, 그만큼 UX와 직관성을 해치게 됩니다.
  - 처음 드래그엔 드랍 로직을 구상할때 가장 큰 고민은 이거였습니다. '3D 월드에 배치할 때, 내가 만일 *원하는 위치*에 배치하고 싶다면 이를 드래그 엔 드랍으로 어떻게 처리할까?' 가장 쉬운 방법은 아무래도 UI에 별도의 창을 띄워서 해당 위치에 부착시키는 것입니다. 아마 이게 정석적이고, 쉬운방법이고, 효율적인 방법일것입니다.
  - 하지만, 제작중인 Cultist에서 이를 적용할 수 있을까, 그리고 더 좋은 방법은 없을까 고민했습니다. 피격 시스템 구현을 위해 Collider를 자동으로 생성하는 기능까지 만들어본 입장에서, 조금 더 멋지고 괜찮은 방법을 고민했습니다.
  - 아이디어는 [언리얼 엔진을 통한 게임 개발에서 배운 레이케스팅 기법](https://youtu.be/U-z1V5-GAzI?si=1kZcCXaUfWXtkTKm)을 사용해보자, 였습니다. 해당 작업에서는 플레이어의 시선에 따라 레이케스팅 빔-을 쭈와아악 날려보내고 이에 닿으면 (물론 무한대로 뻗어나가서 이세상 모든걸 뚫어버리는 레이케스팅 라인을 만들수는 없으니 타협한 작은 선) 해당 오브젝트에 담긴 info를 출력하는 방식이었습니다. 이를 응용하면, 제법 재미있는 작업이 될거라 생각했습니다.
  - 이를 통해 아이디어를 다음과 같이 정리해보았습니다.
    1. UI에서 오브젝트를 마우스 홀드(드래그 On)하면 해당 객체 뒤로 raycast 라인이 생성된다.
    2. 이 Line이 만일 Field Zone에 존재하는 투명한 콜라이더에 들어간 상태에서, drop 하면 필드로 이벤트를 전송한다.
    3. 만일 필드에서 핸드로, 혹은 핸드에서 Sell Zone으로 혹은 Field에서 Sell Zone으로 이동시킨다면? 그럼 UI의 Enter로 처리하면 되므로 간단하다.
  - 이렇게 생각을 마치고, 필요하거나 알아야할 항목들에 대해 정리해보았습니다.
    1. 그럼 단순히 마우스로 이동시키는건 직관성도 떨어지고 '쭈인님 이동시켜유!' 하는 모습을 제대로 보여주려면 뭐가 필요할까? -> 최근 슬더슬2를 재미있게 했으니깐, 여기서 나오는 *선*을 사용해보자.
    2. UI의 Enter 이벤트는 기본 제공 인터페이스도 있고, 사실상 구현은 아주 쉽다. 이벤트 처리로 연결하면 되는거고, 거기에 따른 판매같은건 쉬운데, 그럼 필드에서의 레이케스팅은 어떻게 처리할건가?
       1. Radial Effect 할때도 좌표계 차이로 힘들었는데 이거 100% 문제가 있다.
       2. Scene View에서 보면, Canvas에 따라 Overlay든 Camera space든 World Space든 UI와 3D 월드 상에서 레이케스팅은 아주 민감하고 어려운 문제다.
  - 여러번 시도를 하며 이 부분이 가장 어려웠습니다. 레이케스팅이 원하는 위치에 되는것도 아니고, 자꾸만 꼬여버렸기에 차근차근 기초부터 공부해보았습니다.

#### Unity Canvas mode
1. Screen Space - Overlay
 - 카메라와 상관 없이 무조건 모니터 화면에 맨 앞단, 최상단에 딱 붙어서 그려지며, 별도의 UI 카메라가 필요 없이 해상도를 기준으로 좌표가 결정됩니다. 어떤 캔버스가 존재하든 무조건 가장 위에 그려집니다. 만일 Overlay 캔버스가 여러개라면 우선순위는 다음과 같습니다.
    1. Canvas 컴포넌트의 Sort Order
    2. 하이라키창 배치 순서 (아래쪽에 있는 게임 오브젝트가 맨 위에) -> 이건 동일한 캔버스에 존재한다는 가정이 꼭 필요합니다.
       1. 이때, 만일 Sort Order가 같다면, 재미있게도 하이라키상 위에 있는 오브젝트가 앞에 나옵니다.
        - 이는 유니티 공식적인 엔진 스펙 상, **여러 캔버스의 Sort Order가 같을 때 어떤 것이 위에 렌더링될지는 보장할 수 없다(Undefined)**가 오피셜입니다.
          -  관련 [문서](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Canvas-sortingOrder.html)를 보면 이렇게 표현하고 있습니다. *"When comparing canvases in the same sorting layer, the one with a higher sorting order is displayed above the one with a lower sorting order."* 즉, 높을 때와 낮을 때의 규칙만 엄격하게 정의하고, 같을 때 하이라키를 따른다는 규칙은 볼 수 없습니다. 그러니깐 이건 아마, 엔진 내부의 렌더링 큐 진입 순서나 메모리 주소와 같이 통제할 수 없는 요인에 의해 결정될겁니다. 이걸 손보려면 Unity Pro로 들어가서 C++로 엔진 코딩을 해야할것... 같습니다. (언리얼은 엔진 단위 프로그래밍이 가능하지만, 유니티는 잘 모르겠습니다...)
          - 다른 유니티 [QnA](https://discussions.unity.com/t/usual-canvas-sorting-order-is-not-kept-when-setting-directly-from-script/323493)를 확인해보면 같은 sorting layer를 가진 여러 캔버스들은 하이어러키에 배치된 순서대로 렌더링 되지 않는다고 보고있습니다.
          - [유니티 코드 몽키](https://unitycodemonkey.com/tutorial_text_contents_tinymce.php?v=5_BwFB-1dAo)에서도 이 점을 강조하고 있습니다. *"Avoid Non-Deterministic Behavior. If multiple objects have the same Sorting Layer and Sorting Order, Unity cannot guarantee which one renders on top."*
    3. z값은 무시된다! why? -> Overlay는 3D 공간을 아예 거치지 않고 모니터 픽셀에 UI를 직접 인쇄해 버리는 방식이기 때문.
그림을 보게 되면, 네개의 오버레이와 각각 이미지가 들어있습니다. 첫번째는 Sort가 0인 이미지로 가운데에 위치해있습니다.

<img width="711" height="662" alt="overlay 설명" src="https://github.com/user-attachments/assets/2661f26f-329b-471b-978d-a258314aa9ec" />


그림을 보게 되면, Sort0인 그림 두 개는 동일한 sort oredr에 있음에도 하이라키상 위에 존재하는 그림이 앞에 위치하고 있습니다(가운데 지훈클롭스와 가장 뒤 지훈클롭스). 그리고 sort order가 1이면서 동시에 하이라키 가장 위에 있는 회피중인 지훈클롭스가 가장 위에 표시되고 있습니다. 이를 하이라키상 뒤, 그러니깐 Overlay_Sort0의 위로 옮겨도 동일하게 앞에 위치합니다. 반면 카메라 캔버스의 경우 Sort Order를 아무리 올려도, zorder를 아무리 올려도 언제나 뒤에 위치하게 됩니다.

<img width="385" height="358" alt="overlay 설명2" src="https://github.com/user-attachments/assets/8ae40776-c8c3-4312-a3fc-6fb7226f851f" />


반면 이렇게 동일한 Overlay Canvas에 위치하고 있을 때는 하이라키 위치가 중요합니다. 앞에 나와있는 지훈클롭스가 하이라키상 아래 위치하고 있습니다. 그리고 z 값은 앞에 위치한 지훈 클롭스가 더 작지만, 전혀 영향을 받지 않습니다.

2. Screen Space - Camera
 - 특정 카메라를 지정하고, 그 카메라에 Plane Distance만큼 떨어진 허공에 UI 캔버스를 띄워놓는 방식입니다.
 - 이때, UI 요소들도 3D 월드 상의 실제 좌표(World Position)를 가지게 됩니다. 따라서 마우스 위치(Screen)와 UI 위치(World)를 계산할 때 반드시 UI 카메라를 기준으로 좌표 변환을 해줘야 합니다.

3. World Space
 - UI가 3D 게임 월드 안의 일반 오브젝트처럼 존재합니다. 이건 앞서 설명한 언리얼 게임에서도 사용한 방식입니다.

#### 3D 월드와 2D UI의 관계 및 좌표 작동 방식

마우스로 클릭하는 모니터 화면은 기본적으로 2D(Screen Space, Pixel)이고, 각 유닛들이 실질적으로 존재하는 곳은 3D(World Space) 월드로 설정했습니다. 그럼 이 두 좌표를, 픽셀 단위의 Screen Space를 World Space와 일치시키기 위해서 다음 작업을 진행했습니다.

1. 3D 월드 플레이어 위치를 스크린 픽셀로 변환

```csharp
// 1. 3D 월드의 플레이어 위치를 스크린 픽셀로 변환
Vector2 screenStartPos = _mainCamera.WorldToScreenPoint(transform.position);
```

- 메인 카메라(`_mainCamera`)가 바라보는 3D 공간의 플레이어 위치(x,y,z)를 모니터 화면 픽셀 좌표(x,y)로 바꿉니다. 이를 통해 해당 유닛의 위치를 *마우스 좌표*와 같은 단위로 비교할 수 있게 변환시켰습니다.

2. 플레이어와 마우스의 위치를 모두 UI World Space로 변환

```csharp
// 2. 플레이어와 마우스의 위치를 모두 캔버스 공간(UI World Space)으로 변환
RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvasRect, screenStartPos, _uiCamera, out var uiStartWorldPos);
RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvasRect, eventData.position, _uiCamera, out var uiEndWorldPos);
```

3. 좌표의 z축을 캔버스와 동일하게 고정, 깊이 왜곡 차단
```csharp
uiStartWorldPos.z = _canvasRect.position.z;
uiEndWorldPos.z = _canvasRect.position.z;
```

- 이제 위치를 바꾸었으니, 타겟팅 화살표를 그려야합니다. 여기에서, 이 프로젝트 진행하며 '이걸 꼭 해야해? 그냥 씨 때려 칠까...' 를 많이 고민했던 부분입니다. 더럽게 어렵고, 처음에는 슬더슬2처럼 쭈왁 꺾이는 이쁜 화살표랑 선 구현하고 싶었는데 일직선으로 타협봤습니다.

### [`Combat/TargetingArrowController.cs`](./Combat/TargetingArrowController.cs)
- 선을 만드는건 쉽게 생각했습니다.
  1. 마우스 클릭을 한다. 이때, 마우스 포인터를 변경한다. (림버스 인게임처럼 둥근 모양으로 교체)
  2. 클릭한 부분에 `시작점`을 남겨두고 거기에서부터 선을 생성한다. `끝점`은 언제나 마우스 포인터로 한다.
- 다만, 이런 방법은 사용할 수 없었습니다. 
  - 마우스 커서 이미지를 `Cursor.SetCursor`로 바꿀수는 있지만, 이렇게 해버리면, 만약에 화살표 형식의 머리라면, 머리가 항상 시작점을 등지고 타겟 방향으로 마우스 커서 방향을 회전시키려면 시스템 커서로는 불가능했습니다. 그러니깐 원래 생각했던 방법은 웹이나 2D 엔진에서나 사용할 방법이지, 유니티에서 사용할 방법은 아니었습니다.
  - 유니티에서는... 선 긋기 전용 툴 따위는 없었습니다. 캔버스 UI 자체는 텍스트나 이미지를 띄우는 기능은 있지만 'A지점에서 B점까지 선을 그려라'라는 기본 컴포넌트 같은게 없었습니다... 그래서 이걸 만약에 UI 캔버스 안에서만 해결하려고 한다? 그럼 얇고 긴 네모난 Image를 하나 만든 다음, 스크립트로 시작점과 끝점 사이의 거리를 계산해서 Image의 길이를 늘리고, 각도를 계산해서 RectTransform을 회전시키는 복잡한 수학을 매 프레임 돌려야 합니다. (직선은 어찌어찌 해도, 나중에 곡선을 넣고 싶어지면 아예 불가능해집니다.) 사실 곡선을 이렇게 하려다가 실패해서 떄려쳤습니다.
- 그럼 불가능한가? 그만두어야하나? 어림도 없지. 찾고 또 찾던 중 `LineRenderer`를 찾았습니다. LineRenderer는 3D 입체 공간에서 사용하는 3D 선 긋기 툴입니다. 다만 이걸 사용하면 이제 본격적인 문제가 발생합니다. 선을 아무리 그어보려고 작동시켜도 이게 당길수록(시작점에서 멀 수록) 선이 얇아지고, 가까울수록 커지고. 이상해지기 시작했습니다. 찾아보니 당연하게도...
  - 캔버스(UI): 납작한 2D 평면 캔버스. 원근감 따위 없음.
  - LineRenderer: 3D 입체 공간. 앞, 뒤, 원근감이 존재.
- 이러다보니깐, 원근감이 없는 캔버스에 원근감이 있는 3D 선을 그으려고 시도하니깐 시작점과 끝점 계산에는 계속 오차가 생기고, 선의 한쪽 끝이 카메라 쪽으로 튀어 나오거나 뒤로 파고들어 UI처럼 납작하고 일정한 굵기의 선은 못그리고 3D 공간에서 앞뒤로 기울어지며 있으면 안되는 이상한 원근감이 생겨버립니다. 그럼 이걸 어떻게 해야하나? Z값을 캔버스 Z값과 똑같이 맞춰서 평행하게 다림질을 쫙쫙 해버렸습니다.
- TargetingArrowController는 3D 컴포넌트인 LineRenderer를 억지로 2D UI 처럼 보이게 만드는 컨트롤러입니다. 그럼 이를 바탕으로 TargetingArrowController에 대해 잠시 설명하겠습니다.

- `Awake`: LineRenderer에게 로컬 좌표가 아닌 변환시킨 3D 월드 좌표(UIWorldPos)를 그대로 사용할 것을 선언합니다.

- `Activate`: 드래그가 시작되었다면 LineRenderer가 부착된 프리팹을 활성화 시키고 선을 그릴 준비를 합니다. 이때, 필드 존에 있는 Collider를 그리기 시작해 '필드에 놓고 싶다면 이리로 오시오' 알려줍니다.

```csharp
        public void Activate()
        {
            gameObject.SetActive(true);
            if (drawColliderGL != null) drawColliderGL.showInPlayMode = true;
        }
```

- `DrawArrow`: 그럼 시작점과 끝점 좌표를 받았으니, Z축을 평평하고 일정하게 만든 좌표이므로, 별다른 공정을 거치지 않고 ArrowHead(헬다이버 개발사 아님)를 끝점 위치(마우스 위치로 계속 갱신)로 보내면서 시작점과 반대 방향을 보도록 회전시킵니다.

- 이 과정에서 마우스를 멀리 당길수록 3D 라인 렌더러이기 때문에 점선 텍스처가 무슨 여름철 엿가락마냥 쭈우우우욱 늘어나서 이쁘지 않게 되므로 마우스랑 시작점 사이의 실제 픽셀 거리를 계산해서 그만큼 점선 개수를 늘려줍니다.

```csharp
        public void DrawArrow(Vector3 startUIWorldPos, Vector3 endUIWorldPos)
        {
            if (lineRenderer == null) return;

            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.SetPosition(0, startUIWorldPos);
            lineRenderer.SetPosition(1, endUIWorldPos);

            if (arrowHead != null)
            {
                arrowHead.position = endUIWorldPos;
                arrowHead.localScale = Vector3.one * headScale;

                Vector3 dir = (endUIWorldPos - startUIWorldPos).normalized;
                if (dir != Vector3.zero)
                {
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    arrowHead.rotation = (_uiCamera != null ? _uiCamera.transform.rotation : Quaternion.identity) *
                                         Quaternion.Euler(0, 0, angle - 90f);
                }
            }

            // 타일링을 위해 캔버스 좌표를 픽셀 거리로 다시 변환하여 계산
            if (_lineMaterial != null && tilePixelLength > 0f)
            {
                Vector2 screenStart =
                    _uiCamera != null ? _uiCamera.WorldToScreenPoint(startUIWorldPos) : startUIWorldPos;
                Vector2 screenEnd = _uiCamera != null ? _uiCamera.WorldToScreenPoint(endUIWorldPos) : endUIWorldPos;
                float screenDistance = Vector2.Distance(screenStart, screenEnd);
                _lineMaterial.mainTextureScale = new Vector2(screenDistance / tilePixelLength, 1f);
            }
        }
```

    이 부분은 AI의 도움을 특히 많이 받았습니다. 특히 이 3D 라인 렌더러와 2D 캔버스 사이의 매칭 방법과 텍스쳐 타일링 해결 방법을 해결하는데 많은 도움을 받았습니다.

- 순서를 정리해보면 다음과 같이 정리됩니다.
  1. 3D -> 2D 픽셀: 메인 카메라(`_mainCamera`)를 기준으로, 3D 월드에 서 있는 유닛의 위치를 스크릿 픽셀 좌표(마우스와 같은 단위)로 변환합니다.
  2. 2D픽셀 -> UI 3D 평면: 유닛의 픽셀 좌표와 마우스의 픽셀 좌표를 모두 UI 카메라(`_uiCamera`) 기준의 캔버스 3D 좌표로 변환하여, 두 점을 같은 캔버스 위로 불러옵니다.
  3. Z축 평탄화: LineRenderer 특유의 원근감(구께 왜곡)을 차단하기 위해, 변환된 두 점의 Z값을 캔버스 Z값을 강제 고정합니다.
  4. 그리기: 완벽하게 평평해진 두 좌표를 `_arrowController.DrawArrow`에 넘기고, 여기서 LineRenderer가 점을 잇고, 화살표 머리가 회전하며 텍스처 타일링이 계산됩니다.
  5. UI 상호작용 체크: 무거운 물리 엔진(`Physics.Raycast`) 대신 `RectTransformUtility.RectangleContainsScreenPoint`라는 가벼운 수학 공식으로 현재 마우스가 판매 구역(SellZone) 위에 있는지 판별하여 UI 이펙트를 켭니다. 그러면 SellZone이 입을 쫘악 벌리고 유닛을 먹어치울지 말지 결정합니다.
- 여기서 OnEndDrag가 호출되면 다음과 같이 작동합니다.
  1. 반전술식 - 원상 복구: 타겟팅 화살표를 다시 숨기고, 감춰뒀던 마우스 커서를 화면에 나타나게합니다. 거기에 판매 미리보기도 꺼버립니다.
  2. 판매 구역이라면 판매합니다.
  3. 핸드 구역이라면, 유니티의 UI 이벤트 시스템(`eventData.hovered`)을 활용해 마우스 포인트 아래에 겹쳐진 UI 태그들을 검사합니다. 여기서 "HandZone"을 발견하면, 핸드 매니저에게 유닛을 넘겨주고 필드에서 유닛을 지웁니다.

그럼 반대로, 핸드에서 필드로 이동할때는 어떻게 되느냐? 바로 다음 코드에서 볼 수 있습니다.

### [`Entity/HandSlotUI.cs`](./Entity/HandSlotUI.cs)

이제 3D 공간에 존재하는 유닛을 UI 평면으로 끌고왔으니, 반대도 해봐야합니다. 조립은 해체의 역순, 해체는 조립의 역순이므로 상대적으로 쉽게(?) 구현했습니다.

이 핸드슬롯의 핵심은 

- `OnBeginDrag`: `_canvasGroup.blocksRaycasts = false;`를 시전해줍니다. 이는 드래그를 시작하는 순간 슬로 UI 자체가 마우스 포인터를 가려버리면 마우ㅜ스가 필드나 판매 구역을 인식할 수 없습니다. 필드에서 핸드나 셀존으로 할때는 문제가 없었는게, 선만 생성했지 마우스 위치에 미리보기 같은걸 생성하지 않았기에 여기서만 구현해두었습니다. 물론 추후에 미리보기용 이미지나 잡힌 캐릭터가 바둥바둥 거리는 귀여운(?) 애니메이션 이미지 같은걸 넣게 된다면 거기서도 이 레이케스팅 해제를 추가해야합니다.

- `OnDrag`: 여기에선 앞보다 로직이 확 줄어들었습니다. `Player.cs`에서는 시작점이 3D 공간에 있기 때문에 픽셀로 바꾸고 이걸 다시 캔버스로 바꿔야했지만 여긴 시작점 자체가 UI요소에 있으므로 이 과정이 생략됩니다. 그러니 마우스 위치(끝점) 하나만 캔버스 공간으로 보내주고, Z축 다림질 역시 시작점의 Z값 (`lineStartPos.position.z`)에 맞춰버리면 이쁘게 나옵니다. 아 이쁘다.

```csharp
// 시작점 변환 과정이 사라짐!
RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvasRect, eventData.position, _uiCamera, out var uiEndWorldPos);
uiEndWorldPos.z = lineStartPos.position.z;

arrowController.DrawArrow(lineStartPos.position, uiEndWorldPos);
```

- `OnEndDrag`: 여기가 핵심입니다. 유저가 핸드에 있던 유닛을 놓았을 때, 판매 vs 필드 배치를 결정하게 됩니다. 이는 UI에서 3D 월드로 raycast 빔을 쭈와아악 발사해서 해결했습니다.

```csharp
// UI(2D 픽셀)에서 3D 월드로 물리 레이저 발사!
Ray worldRay = _mainCamera.ScreenPointToRay(eventData.position);
if (Physics.Raycast(worldRay, out var hit) && hit.collider.CompareTag("FieldZone"))
{
    if (_isInCombat) { Debug.Log($"[HandSlotUI] 전투 중에는 필드에 기물을 넣을 수 없는 데수웅"); return; }
    // 필드 배치 실행
}
```

  - 만약 판매 구역이다? 그럼 앞에서처럼 간단하게 `RectangleContainsScreenPoint`로 판매합니다.
  - 필드에 놓고싶다?????
    - `_mainCamera.ScreenPointToRay(eventData.position)`: 메인 카메라야...지금 마우스 픽셀 위치를 관통(헉 도너츠)해서 3D 월드를 향해 레이저 빔을 쏴라아앗!!! 아직 멀었냐 피콜로!!!!
    - 그러면 발사된 마관광살포, worldRay가 3D 월드를 향해 날아가다가 미리 3D에 배치해둔 Field Collider에 부딪히면 hit에 정보를 담습니다.(`out var hit`). 그러면 거기서 태그 비교를 해서 필드 존이네? 하면 필드 배치를 수행합니다. 물론 여기서 전투중이거나 필드가 꽉차면 거부합니다.

## Chapter 4. 스킬 실행 전략 (Strategy 패턴)
> 코인 토스 및 스킬 실행

### [`Logic/CoinActionLogic.cs`](./Logic/CoinActionLogic.cs)
> Custom CoinAction을 위한 추상 클래스

- SO 형태의 추상 클래스입니다. 림버스 전투를 보면, 각 코인이 고유의 애니메이션과 공격 로직을 가지고 있습니다. 어떤건 다단히트기도 하고, 어떤건 묵직한 한방(아아 뫼르소...)을 내기도 합니다. 그런데 if문 무한 중첩으로 구현하면... 생각만해도 아찔해져서 SO 형태로, '끼워넣기'가 가능하게 구현해보았습니다. `BattleManager`(아래에서 더 자세히 다룹니다!!!)에서는 결론적으로, 코인 앞뒷면을 판정하고 해당 코인의 총 데미지를 전달합니다. 가령, 합이 끝나서 각 코인마다 데미지를 주어야 하는 시점이 오면, 해당 코인을 토스하고 -> 앞뒷면 가중치 계산하고 -> 캐릭터 스탯이랑 스킬 데미지 계산해서 -> 총 데미지를 여기로 전달합니다. 그러면 이제 그 총 데미지를 다단히트로 할건지, 아니면 묵직한 한방을 때릴껀지, 이러면서 개쩌는 애니메이션을 출력할건지 등을 결정하는 곳이 바로 이 CoinActionLogic입니다.
- 당연히 혼자 만들고있는 입장에서, 모든 캐릭터의 공격 로직을 customLogic으로 만들어서 사용할수는 없기에 보험도 들어두었습니다.
```csharp
                else
                {
                    var hits = coin.hitCount > 0 ? coin.hitCount : 1;
                    var damagePerHit = totalDamage / hits;

                    for (var i = 0; i < hits; i++)
                    {
                        targetChar.PlayCharacterSfx(coin.hitSound);
                        targetChar.TakeDamage(damagePerHit);

                        float currentDelay = coin.hitDelay > 0f ? coin.hitDelay : 0.5f; // 기본값 설정

                        if (coin.customHitDelays != null && i < coin.customHitDelays.Length)
                        {
                            currentDelay = coin.customHitDelays[i];
                        }

                        if (isFocus) yield return new WaitForSecondsRealtime(currentDelay);
                        else yield return new WaitForSeconds(currentDelay);

                        if (targetChar.Hp <= 0) break;
                    }
                }
```
- 별도의 hitCount 변수도 두었기에, 코인 데이터(`coinData`) 인스펙터에 설정해둔 hitCount에 맞게 다단 히트를 정상적으로 수행하긴 합니다. 이 else 블록 자체를 StandardHitLogic의 마이너 카피 버전으로 만들었습니다. 이걸로 SO가 없어도, 거기에 다단 히트를 하고 싶어도 사용할 수 있게 해☆결 했습니다.

이 코인 액션 로직 자체는 아래에서 설명할 `StandardHitLogic`과 `EscalatingHitLogic`을 위한 추상클래스로만 생각하면 됩니다!

### [`Core/CoinResolver.cs`](./Core/CoinResolver.cs)
> 스킬 코인의 상태 조회·파괴·토스를 담당하는 순수 헬퍼 

전투의 흐름 자체는 `BattleManager`가 담당하지만, 개별적인 스킬은 이 `CoinResolver`가 담당합니다. 그런데 생각을 해보면, static은 메모리에 하나만 존재하는데, 여러 곳에서 동시에 코인 상태를 조회하고 파괴하고 토스하면 꼬이지 않을까? 라는 생각을 할 수 있습니다.

사실 이 클래스는 원래 `BattleManager`에 통합되어 있었습니다. 그리고 그 이전에는 단순히 '캐릭터가 코인을 돌리는 주체지니깐, 캐릭터가 가지고 있으면 되지 않을까?'라고 생각하여 `Character`에 구현했습니다. 그렇게 구현하다가 꼬이고 꼬여서 결국 차라리 `BattleManager`에서 전투 자체를 총괄 시키자, 라는 생각으로 `BattleManager`에 구현했습니다. 다만 그러다보니, 이 `BattleManager` 코드만 700~800줄이 되어버리고 한번 복습하려고 혹은 로직 고치려고 볼때마다 어질어질했습니다. 그래서 과감하게 코인 로직은 분리했습니다. 단순히 코인은 이곳저곳에서도 다 쓰는거고, 코인마다 개별적으로 접근이 어디서든 가능해야하니깐 static으로 하자! 해서 이렇게 구현했습니다.

**결과적으로는, 운이 좋았습니다. 다만 운이 좋은것에 그치지 않고 제대로 복습하고 이해하고자 다시금 정리해보았습니다.**

가장 중요한 핵심은 `CoinResolver` State를 가지지 않는 순수 헬퍼 클래스입니다. 즉, 이 코인이 뭔지, 뭔 역할을 하는지 진짜 1도 관심 없습니다. 만약 필드가 존재한다면 당연히 이야기는 다를겁니다. 뭐 가령, `public static int currentPower;`같은게 내부에 있다면 어디서든 접근 가능할테니 교착 상태를 막기 위해서라도 세마포어나 뮤텍스를 도입해서 동시성 프로그래밍을 해야 할 것입니다. (전공 OS 수업을 듣기를 잘 했지만, 여기서 안쉽게도(?) 그 실력을 뽐낼 기회는 없어졌습니다. 휴.)
여기에선 그냥 계산기 역할만 합니다. CoinResolvere는 단순 공식 계산기로 쓰이는 것이지 뭐 다른걸 하는건 아닙니다. 한번 내부를 자세히 보겠습니다.

```csharp
public static IEnumerator TossSlotCoinsRoutine(ActionSlot slot, float totalDuration, Action<int> onComplete, bool isFocus)
{
    var skill = slot.SelectedSkill; // 넘겨받은 slot에서 데이터를 꺼냄
    // ...
    totalPower += isFront ? coin.frontValue : coin.backValue; // 지역 변수(Local Variable)에 저장
    // ...
}
```

여기서 쓰이는 `skill`, `totalPower`, `delayPerCoin` 같은 변수들은 클래스의 변수가 아니라 지역 변수(Local Variable)입니다. 그러니 당연하게도, CA 지식을 조금만 꺼내보면 이것들이 메모리 스택에 새롭게 만들어져 쌓이고, 당연히 함수마다 다른 스택 공간을 사용하니 퍼니 발렌타인의 D4C라도 오지 않는 이상, 서로 간섭할 수 없습니다.

거기에 유니티 코루틴 작동 방식도 이를 더 안전하게 만들어줍니다. 코루틴은 진짜 멀티 스레드가 아니라 메인 스레드 하나에서 아주 빠르게 번갈아가며 Time-Slicing 하게 실행되는 구조입니다. 즉, 엔진 레벨에서도 진짜 완전히 동시에 완벽하게 엄청난 확률로 코드가 동시에 겹쳐서 실행되는 일은 없으므로, 사실 코루틴 안에서 `static` 함수를 마구마구마구마구마구 불러다 써도 데이터 결함은 없을겁니다.

다시 본론으로 돌아와서 `CoinResolver`의 역할은 간단합니다. 파괴 안된 코인을 세어주거나, 코인을 모.분 해버리거나 코인 하나를 굴리거나, 코인이 여러개일 경우 각 코인을 돌리는 로직을 돌리거나 그리고 마지막으로 코인이 뒤집힐 확률을 구하거나입니다. 마지막에 있는 `CalculateSlotProbability`는 간단하게 `y = ax + b` 꼴의 함수를 이용해서 정신력 비례 앞면 노출 확률을 설정한겁니다. 그 아래에는 만일 앞면 혹은 뒷면을 강제해야 하는 경우 강제할 수 있도록 추가 함수도 구현해두었습니다. (여기선 사용 안합니다!)

### [`Logic/StandardHitLogic.cs`](./Logic/StandardHitLogic.cs)
> 단순 구현체 1

가장 기본이 되는 타격 로직입니다. 넘겨받은 `totalDamage`를 코인의 타수(`hitCount`)만큼 균등하게 나눕니다. (이 `totalDamage`도 `totalDamage`를 두고 균등하게 나눈 뒤, Random으로 구분해서 데미지를 분산시키는 등 너무 균등하지 않게 확장도 염두에 두고 있습니다.) 또한 **넉백**이 존재합니다. 여기서 일전에 말한 무게가 사용됩니다.

```csharp
// 밀려날 방향 계산 (공격자 -> 방어자 방향)
var pushDirection = (target.transform.position - attacker.transform.position).normalized;
```
  - `target.position - attacker.position`: 유니티(벡터 수학)에서 `목표점(B) - 시작점(A)`을 하면 A에서 B를 향하는 벡터가 만들어집니다. 즉, 공격자에서 방어자를 관통하여 등 뒤로 뻗어나가는 밀려날 방향을 구합니다. (관통 추가뎀)
  - `.normalized`: 그럼 이렇게 구했으니, 벡터에서 길이가 1인 방향 벡터만 추출합니다.

```csharp
if (pushDirection == Vector3.zero) pushDirection = Vector3.forward;
```
  - 그런데 만일, 아주아주 드물게, 솔직히 이럴 순 없겠지만... 공격자와 방어자의 X, Y, Z 좌표가 0.00001의 오차도 없이 완전히 똑같을 수 있습니다. 이때 뺄셈을 하면 방향이 Vector3.zero(0, 0, 0)가 되어버립니다.
  - 방향이 0인 상태로 다음 계산을 진행하면 캐릭터가 아예 사라지거나 에러가 날 수 있기 때문에, "만약 겹쳐있으면 일단 앞쪽(`Vector3.forward`)으로 밀어라!" 라고 예외 처리를 해두었습니다.

그럼 이렇게 방향 벡터를 구했으니, Force를 구해야합니다.
```csharp
                // 살아있을 때만 밀어내기 및 피격 모션 적용
                if (target.Hp > 0)
                {
                    target.ExecuteAttackMotion("Hit");
                    if (!target.IsImmovable)
                    {
                        var targetPos = target.transform.position + pushDirection * pushbackDistancePerHit;
                        target.transform.position = targetPos;
                    }
                }
```
  - 죽은자는 말이 없듯, 그리고 티베깅은 벤 사유이므로 타겟이 살아있을때만 밀어내기 및 피격 모션을 적용합니다. for루프, 그러니깐 타격이 들어갈 때마다 실제로 대상을 이동시킵니다.
    - `pushDirection * pushbackDistancePerHit`: 길이가 1인 방향 벡터에, 인스펙터에서 설정한 밀려날 거리(`pushbackDistancePerHit` = 0.5f 등)를 곱합니다. 그러면 길이가 1인 방향 벡터에 해당 force만큼 힘이 가해지므로 그 방향만큼 벡터가 생성됩니다.
    - `target.transform.position + ...`: 현재 방어자의 위치에 그 0.5m짜리 화살표를 더해서 최종 목적지(targetPos)를 구합니다.
    - `target.transform.position = targetPos`: 방어자를 최종 목적지로 즉시 순간이동시킵니다.
  - 여기에서는 즉시 순간이동으로 구현했는데, 이는 타격이 들어가는 프레임에 맞춰 좌표가 끊기듯 뒤로 찔끔찔끔 밀려나게 됩니다. 이는 플레이어의 눈에 캐릭터가 강한 타격을 맞고 몸을 가누지 못해 뒤로 비틀거리는(Stagger) 아주 **찰진 타격감**을 구현해보았습니다.
  - 물론, 부딪히자마자 멀리 쭈우욱 밀려나는 큰 넉백은 `BattleManager`의 `CalculateKnockback`이 부드러운 코루틴으로 담당하고, 여기 `StandardHitLogic`에서는 타격 순간의 '미세한 덜컥거림'을 담당하도록 완벽하게 역할을 분리했습니다.
  - 그리고 당연히, 대상이 움직일 수 없는 환상체라면 움직이면 안되므로 `target.IsImmovable` 여부를 검토합니다.

### [`Logic/EscalatingHitLogic.cs`](./Logic/EscalatingHitLogic.cs)
> 단순 구현체 2

- 여긴 뒤로 갈수록 대미지가 강해지는 로직을 구현했습니다. 대미지 분배 (가중치)를 통해 타수가 진행될수록 대미지 배율이 증가합니다. 예를 들어 3타 스킬이라면 가중치 총합은 6 (1+2+3)이 됩니다. 1타는 전체 대미지의 1/6, 2타는 2/6, 마지막 3타는 3/6(절반)의 묵직한 대미지를 꽂아 넣습니다.

### [`Logic/SpearAttackLogic.cs`](./Logic/SpearAttackLogic.cs)
> 투사체 기반 복합 구현제 (전체 조율)

- 여기서는 Attacker의 Rarity에 따라 소환할 창의 개수를 결정합니다. 그러면 `SpawnSpears`를 통해 창들을 머리 위(`yOffset`) 주변 반경(`radius`)에 원형으로 소환합니다. 그리고 이 창들을 한 번에 쏘지 않고 fireDelay 간격을 두고 순차적으로(따다닥) 발사시킵니다. 발사음과 피격음도 순차적으로 다른 소리(`sequentialAttackSounds`)를 내도록 세팅할 수 있습니다.

### [`Logic/SpearProjectile.cs`](./Logic/SpearProjectile.cs)
> 투사체 기반 복합 구현체 (개별 동작)

- 창들이 스폰되면 공격자 주위를 빙글빙글 돌다가 `Fire` 명령이 떨어지면 타임스케일을 무시(`Time.unscaledDeltaTime`)하고 날아갑니다. 이는 스킬 3은 주변 슬로모션 적용 + 주변 인물들 opacity 조절이 진행되므로 이 투사체는 반드시 타임 스케일을 무시해야 똑바로 작동합니다. 아니면 느려진 시간 속에서 함께 느려지므로.......
- 이렇게 타겟이 닿는 순간 콜백 함수(`_onHitCallback`)를 호출하여 타겟에게 대미지를 입힙니다.

<img width="800" height="333" alt="spear" src="https://github.com/user-attachments/assets/b39af735-9114-4f5f-86d3-f03fbf83ac64" />

| 구분 | SpearAttackLogic | SpearProjectile |
| :--- | :--- | :--- |
| **타입** | ScriptableObject (CoinActionLogic 상속) | MonoBehaviour |
| **존재 형태** | 에셋 1개 (씬에 없음, 인스펙터 데이터) | 씬 오브젝트 N개 (창 1개당 1개) |
| **역할** | 공격 1회의 시나리오 지휘 | 창 1개의 자기 행동 |
| **상태** | 실행마다 인자로만 받음 (per-execution stateless) | 자체 상태머신 보유 (Spawning → Orbiting → Firing) |
| **수명** | 코인 액션이 끝나면 역할 종료 | Fire 후 타겟 명중 시 Destroy(self) |

## Chapter 5. 전투 코어
> 핵심 전투 로직

- 캐릭터 설계, 코인 로직 설계 그리고 공격 애니메이션까지 끝났다면 이제 실제로 핵심 전투 로직이 어떻게 돌아가는지 설명하겠습니다. 우선, 다음 다이어그램을 통해 전투가 어떤 방식으로 진행되는지 간단하게 설명하겠습니다.

<img width="1903" height="6029" alt="전투 다이어그램" src="https://github.com/user-attachments/assets/21e9da0a-40e1-418f-a05c-a9a9c394c5b3" />

1. 전투 개시
   1. 전투 시작 버튼 클릭
   2. 전투가 가능한 상태라면 (필드에 아군 유닛이 있다면) 전투 시작
2. 타겟 결정
   1. 생존 캐릭터 전원 속도 굴림
   2. 캐릭터마다 개별 ActionSlot 생성
   3. 적 슬롯이 플레이어 슬롯을 랜덤 지목
   4. 플레이어 슬롯을 속도 내림차순 정렬
   5. 플레이어 슬롯이 합 상대 잠금 - 자기보다 느러가나 같고 아직 안 잡힌 적이 있으면 상호 링크(합!), 없으면 아무나 일방 지목
   6. 결과를 BattleMatchup으로 변환
3. 전투 실행
   1. 전투 상태로 전환. 교전마다 `ProcessBattleSequence`를 동시에 실행
   2. 교전 1건마다 코인 리셋 → 사망 조기 종료 체크 → `ViewType`으로 `isFocus` 판정(포커스면 슬로모션·반투명) → `MatchType`으로 분기.
      1. 합: 양쪽 코인이 남은 동안 반복: 충돌 지점 계산 → 접근 → 합 모션 → 양측 코인 토스 → 위력 비교 → 진 쪽 코인 1개 파괴 + SP 증감 → 넉백. 한쪽 코인이 바닥나면 남은 쪽이 `ExecuteMultiHitAttack`.
      2. 일방 줘팸: 접근 → `ExecuteMultiHitAttack` → 타겟 넉백.
   3. 코인을 순서대로: `CoinResolver.TossSingleCoin`으로 위력 누적 → 공격음 → 공격 모션(대시 여부) → vfx → `customLogic` 있으면 위임 / 없으면 기본 타격 루프(TakeDamage 반복).

### [`Core/TargetingResolver.cs`](./Core/TargetingResolver.cs)
> 속도 기반 타겟 매칭 알고리즘(합/일방공격 판정)

이번 턴에 누가 누구를 때릴지(헉), 그리고 해당 공격이 합(Clash!)인지 일방 공격인지 결정합니다.

`ResolveTurn`: 총 6개의 페이즈로 나뉘어 실행되는 `TargetingResolver`의 핵심 메서드입니다.

1. 속도 결정
   - 모든 플레이어와 적들이 `RollBattleSpeed()`를 호출하여 이번 턴의 자신의 속도 값을 결정합니다.
2. 슬롯 생성
   - `GenerateSlots`을 이용해 `playerSlots`과 `enemySlots`을 생성합니다.
3. 적들의 타겟팅
   - 림버스에서 적들의 공격을 먼저 화살표로 알려주듯(실제 인게임에서는 공격 우선순위나 다른 로직이 사용되겠지만, 여기에서는 간단하게 랜덤으로 구현했습니다.) 살아있는 플레이어 슬롯 중 아무나 랜덤으로 골라서 `TargetSlot`으로 설정합니다.
4. 플레이어 정렬
   - `SortSlotsBySpeed`를 호출해 플레이어 슬롯들을 가장 빠른 순서대로 정렬합니다. 인게임과 마찬가지로 빠른 플레이어일수록 합 가로채기가 가능합니다.
5. 플레이어 타겟팅 및 합 가로채기
   - 림버스 전투의 꽃, 합 가로채기를 생각해보았습니다. 다음과 같이 진행됩니다.
     1. 정렬된 플레이어 슬롯(`pSlot`)을 하나씩 꺼냅니다.
     2. 적 슬롯(`eSlot`)들을 탐색하며 다음 조건을 만족하는 가로채기 후보를(`candidateSlot`) 찾습니다.
        1. `eSlot.Owner.Speed <= pSlot.Owner.Speed`: 속도가 적과 빠르거나 같은 경우
        2. `playerSlots.All(p => p.TargetSlot != eSlot)`: 아직 다른 아군이 해당 적을 타겟으로 잡지 않은 경우
            => 속도가 빠른 아군이 반드시 '먼저' 가로채기를 진행하고, 속도가 느리다면 일방 줘팸만 가능!
     3. 가로챌 수 있는 적이 있다면 그 적을 타겟으로 삼아 서로를 바라보게 만듭니다(`pSlot.TargetSlot = selectedEnemySlot; selectedEnemySlot.TargetSlot = pSlot;`). 가로챌 적이 없다면 남은 적 중 아무나 랜덤으로 공격합니다.
6. BattleMatchup 확정
   - 일전에 구현해두었던 BattleMatchup을 본격적으로 사용해야합니다. 이제 일종의, 림버스로 따지면 화살표가 다 그어졌으니 결과를 분류해서 `finalBattles` 리스트에 담습니다.
   - 합: 플레이어와 적이 서로를 타겟으로 삼고 있다면(`defenderSlot.TargetSlot == pSlot`), 두 슬롯을 묶어 `BattleMatchType.Clash`로 판정합니다.
   - 일방 줘팸 - 플레이어: 서로 바라보지 않는다면 플레이어의 일방 공격으로 판정합니다.
   - 일방 줘팸 - 적: 적들 중에서도 합을 뺏기지 않고(누구의 타겟도 되지 않고) 원래 목표를 유지한 녀석들은 적의 일방 공격으로 판정하여 리스트에 밀어 넣습니다.

코드를 통해서 하나하나 톺아보겠습니다.

```csharp
var finalBattles = new List<BattleMatchup>();
```
우선 최종적으로 완성시킬 `BattleMatchup` 리스트를 만들었습니다. 이제 이 리스트에 `BattleMatchup`을 그득그득 채워넣을 것입니다.

```csharp
            foreach (var character in players)
                if (character != null && character.Hp > 0)
                    character.RollBattleSpeed();
            foreach (var character in enemies)
                if (character != null && character.Hp > 0)
                    character.RollBattleSpeed();
```
여기에서 속도굴림을 시작합니다. 굴린다 라는 말에 어감이 참 좋습니다. DnD 느낌도 나고.

```csharp
            var playerSlots = GenerateSlots(players);
            var enemySlots = GenerateSlots(enemies);
```
이제 슬롯을 생성합니다.

```csharp
            if (playerSlots.Count == 0 || enemySlots.Count == 0) return finalBattles;

            foreach (var enemy in enemySlots)
            {
                enemy.TargetSlot = playerSlots[Random.Range(0, playerSlots.Count)];
            }
```
만약에 어? 사람이 읎어? 하면 그냥 빈 리스트를 보냅니다. 그럼 빈찬합을 받은 순욱처럼 게임은 게임 오버 메뉴를 띄울것입니다. 이 부분을 무사히 통과했다면, 적 먼저 플레이어를 상대로 타겟팅합니다.

```csharp
            SortSlotsBySpeed(playerSlots);
```
그럼 이제 플레이어의 본격적인 '자동 전투'를 위해 슬롯을 속도 기준으로 정렬해줍니다.

```csharp
            var processedSlots = new HashSet<ActionSlot>();

            foreach (var slot in allSlots)
            {
                // 이미 합 진행 했으면 패스. 여기에 추후 흐트러짐 상태면 패스 로직을 넣어도 됨!
                if (processedSlots.Contains(slot)) continue;

                var targetSlot = slot.TargetSlot;
                var isFocus = slot.SelectedSkill.isFocusSkill || targetSlot.SelectedSkill.isFocusSkill;
                var viewType = isFocus ? CameraViewType.FocusTarget : CameraViewType.Normal;

                if (targetSlot.TargetSlot == slot)
                {
                    finalBattles.Add(new BattleMatchup
                    {
                        AttackerSlot = slot,
                        DefenderSlot = targetSlot,
                        MatchType = BattleMatchType.Clash,
                        ViewType = viewType
                    });

                    processedSlots.Add(slot);
                    processedSlots.Add(targetSlot);
                }
                else
                {
                    finalBattles.Add(new BattleMatchup
                    {
                        AttackerSlot = slot,
                        DefenderSlot = targetSlot,
                        MatchType = BattleMatchType.OneSided,
                        ViewType = viewType
                    });

                    processedSlots.Add(slot);
                }
            }
```
이제 이렇게 만들어진 내부 링크를 바탕으로 합과 일방공격을 속도 순서대로 진행하기 위해 BattleMatchup 리스트에 이를 하나씩 등록합니다.(여기에서 `processedSlots`을 `HashSet<ActionSlot>`으로 사용한 이유는, `List.Contains(slot)`은 리스트의 처음부터 끝까지 데이터를 찾기 때문에 O(N)의 시간이 걸립니다. 하ㅓ지만 `HashSet.Contains(slot)`은 해시 알고리즘으로 데이터를 찾으므로 O(1)의 시간이 걸립니다. (와!) 물론... 슬롯이 실상 인게임에서도 많아봤자 닝-겐이 체감할 수 없는 수준의 차이만 생기겠지만 추후 100 vs 100 전투가 된다면? 이건 분명 체감이 있을겁니다. 슬롯 100 vs 100개의 전투? 가슴이 웅장해집니다.)
그럼 여기에서 최종적으로 전달받은 BattleMatchup 리스트가 BattleManager에 전달되어 실제 전투를 진행하게 됩니다.

`GenerateSlots`: 전투 시작 전, 각 캐릭터가 이번 턴에 몇 번 행동할지, 그리고 어떤 스킬을 쓸지 정해줍니다. 방어 코드를 한번 거치고 난 뒤, 캐릭터의 `ActionSlotCount`에 접근하여 해당 개수만큼 슬롯을 생성, 스킬을 적용합니다. 이때 캐릭터 스킬의 각 확률에 기반해서 슬롯을 생성합니다.

```csharp
        private List<ActionSlot> GenerateSlots(List<Character> characters, bool isPlayer)
        {
            var slots = new List<ActionSlot>();
            foreach (var character in characters)
            {
                if (character == null || character.Hp <= 0) continue;

                for (int i = 0; i < character.ActionSlotCount; i++)
                {
                    slots.Add(new ActionSlot
                    {
                        Owner = character,
                        SelectedSkill = character.GetRandomSkillByProbability(),
                    });
                }
            }

            return slots;
        }
```

`SortSlotsBySpeed`: 플레이어 슬롯들을 속도 순서로 정렬합니다. ResolveTurn에서 편하게 사용하기 위해 만들었습니다. 만약에 속도가 같다면 캐릭터의 최대 속도(maxSpeed)가 더 높은 쪽에 우선권을 주었습니다. 이렇게 신경쓴 이유는, **림버스에서도 이따금 캐릭터의 스킬 슬롯이 2개 이상 연달아 진행될 때 어떤 스킬은 먼저 진행되고 어떤 스킬은 나중에 진행되는게 규칙성이 보일랑말랑해서 한번 탐구해보고자 디테일하게 구현해보았습니다.**

### [`Combat/BattleActionController.cs`](./Combat/BattleActionController.cs)
> 충돌 예측·넉백 물리 계산

허공에 주먹질을 하고 무기를 휘두르기 위해서는 무공의 경지가 고강해서, 완숙한 절정 이상의 경지에 올라 내공을 실어 날리거나 무기의 길이가 매우매우매우 길어야합니다. 아직 수감자들의 무공 수위가 그정도에 오르지 못했으므로, 그리고 아무리 긴 무기가 있다고 하더라도 일단 붙어야 하므로, 충돌을 예측하고 부딪혔을 때 넉백이 있어야합니다. 그래야, 게임이 더 찰져질것입니다.

기본적으로 생각한건 다음과 같습니다.
1. 각 캐릭터에게는 Speed와 Weight 값이 존재한다.
2. 두 개의 타겟팅 되는 캐릭터와 적 사이의 거리가 존재한다.
3. 그렇다면 가속도를 구하고, 두 캐릭터가 만나는 지점을 미리 계산한다.
4. 만나는 지점까지 두 캐릭터를 각각 이동시킨다.
5. 만나게 된다면, 해당 점을 기준으로 두 캐릭터가 자신의 속도와 무게에 비례해서 (작용 반작용) 튕겨나간다.
6. 이를 반복하며, 되돌아오는 시간을 기하급수적으로 줄이며 합이 점점 빠르게 진행하게 한다.

이렇게 생각할 수 있습니다.
*그럼 그냥 이렇게 하지 말고, 두 캐릭터 동시에 개별 속도로 보내고 Collider로 부딪히면 그거 연산해서 튕겨나가고 하면 되는거 아님???*
물론, 이럴 수 있습니다. 다만 그러면 *물리 계산은 조상님이 해줌?*

유니티에서 콜라이더 기반 연산은 메모리를 많이 잡아먹습니다. 그리고 물리엔진 특유의 버그가 발생하면, 그러니깐 두 물체가 지나치게 빠르게 부딪히거나, 극적인 연출을 위해 엄청나게 가까워져야하거나, 부딪힌 상태에서 컷신으로 특정 대사 연출을 하는 동안 대기를 한다면 결국 -> Colldier를 켜두자니 계속 쭈인님 부딪혔어요오 보내고, 끄고 키자니 이건 여간 귀찮은 오버헤드가 아닐 수 없습니다....

그럼 코드랑 함께 설명하겠습니다. 우선, 구조체로 다음과 같이 선언했습니다.

```csharp
    // 충돌(합)에 참여하는 한쪽의 운동 정보
    public struct ClashMover
    {
        public Transform Transform;
        public float CurrentSpeed;
        public float AddedSpeed;
        public float Weight;
        public float MaxSpeed;
        public float Range;

        public bool IsImmovable;
    }

    // 두 물체가 만나는 시점·위치·최종 속도 계산 결과
    public struct MeetingResult
    {
        public float TimeToMeet;
        public Vector3 MeetPosA;
        public Vector3 MeetPosB;
        public Vector3 FinalVelA;
        public Vector3 FinalVelB;
    }

    // 충돌 후 두 물체가 튕겨날 목표 지점·반동 속도 계산 결과
    public struct KnockbackResult
    {
        public Vector3 TargetA;
        public Vector3 TargetB;
        public float ReboundA;
        public float ReboundB;
    }
```
각각 다음의 정보를 가지고 있는 구조체입니다.
   - 충돌 객체 정보(`ClashMover`): 이동할 캐릭터의 Transform, 속도, 그리고 무게를 기록
   - 충돌 결과 예측(`MeetingResult`): 언제(TimeToMeet), 어디서(MeetPos), 얼마의 속도(FinalVel)로 부딪히는지 정보를 기록
   - 반작용 결과(`KnockbackResult`): 어디까지 튕겨나가고(Target), 다음 돌진을 위한 반동 속도(Rebound)는 얼마인지 정보를 기록

1. `CalculateMeetingData`: 가속도 및 교차점 계산. 가속도를 구해서 만나는 지점을 예측합니다.
```csharp
public MeetingResult CalculateMeetingData(ClashMover a, ClashMover b)
{
    // ... (방향(dirA, dirB) 구하는 공식 생략) ...

    var startSpeedA = Mathf.Min(a.CurrentSpeed + a.AddedSpeed, a.MaxSpeed);
    
    // 가속도(a) = 힘(F) / 질량(m) 가속은 곧 힘 힘은 곧 가속도 되니 F=ma다!
    // 여기서 AddedSpeed가 캐릭터가 내는 '힘'이고, Weight가 '질량'입니다.
    // 무거운 캐릭터일수록 가속도(accelA)가 낮게 나옵니다.
    var accelA = dirA * (a.AddedSpeed / a.Weight); 

    // 외부 함수(IntersectionCalculator)를 통해 두 등가속도 운동의 궤적이 만나는 시간(t)과 위치를 구합니다.
    var willMeet = IntersectionCalculator.CalculateIntersection(...);

    if (willMeet)
    {
        // 나중 속도(v) = 처음 속도(v0) + 가속도(a) * 시간(t)
        // 충돌하는 바로 그 순간의 최종 속도(FinalVel)을 계산합니다.
        result.FinalVelA = Vector3.ClampMagnitude(velA + accelA * timeToMeet, a.MaxSpeed);
    }
    else
    {
        // (예외 처리: 평행선이거나 수학적으로 안 만나면 강제로 중간 지점에서 만나게 함)
    }

    // 캐릭터 이미지가 완전히 겹치지 않고 무기가 닿는 거리(Range)에서 멈추도록 위치를 빼줍니다.
    result.MeetPosA = meetingPoint - dirA * a.Range;
    return result;
}
```

2. `ExecuteMovement`: 실제 이동 처리. 만나는 지점까지 캐릭터 이동
```csharp
public IEnumerator ExecuteMovement(Transform target, Vector3 destination, float duration, bool useUnscaledTime = false)
{
    var startPos = target.position;
    // Y축을 startPos.y로 고정하여 캐릭터가 땅을 파고들거나 하늘로 솟구치지 않게 합니다. 잘못하면 백룸으로 이동합니다.
    var endPos = new Vector3(destination.x, startPos.y, destination.z); 
    var elapsedTime = 0f;

    while (elapsedTime < duration)
    {
        elapsedTime += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        // 사인 곡선 가속가속으로 제곱 곡선보다는 부드럽게 가속
        var rawT = elapsedTime / duration;
        var t = 1f - Mathf.Cos(rawT * Mathf.PI * 0.5f);
        target.position = Vector3.Lerp(startPos, endPos, t);
        yield return null;
    }
    target.position = endPos;
}
```

3. `CalculateKnockback`: 작용-반작용 및 튕겨나감. 만난 점을 기준으로 속도와 무게에 비례하여 튕겨나갑니다.

이때, 사실 진짜 작용 반작용이라면, 두 물체가 부딪히는 순간 양쪽이 받는 힘(충격량)은 완벽하게 똑같아야합니다.

그래서 결국 가속도 공식`(a = F/m)`에 의해, 속도와 상관없이 무조건 가벼운(Weight가 낮은) 쪽이 더 멀리 튕겨 나가야 합니다. 그런데 이러면 재미 없습니다. 내가 합을 이겼는데 왜 내가 더 멀리 튕겨나가야 하는 느낌이 들어야하지?

물론 여기서 그럼 무게 개념을 빼버리면 되겠지만, 뭔가 묵직한 맛도 좀 있으면 좋기도 하고, 여기서는 완벽하게 똑같은 시스템을 구현하기 보다는, 제가 어떤 생각을 가지고 해석해서 구현했는지 보여드리는게 더 좋을 것 같았습니다. 따라서 다음과 같이 바꾸어보았습니다.

1. 가속할 때 (현실 물리 적용): BattleActionController에서 가속도(accelA)를 구할 때는 Weight로 나눠줍니다. 즉, 무거운 유닛은 굼뜨게 출발하고, 가벼운 유닛은 미친 듯이 가속합니다.
2. 부딪히고 튕겨날 때 (게임 물리 적용): 넉백 거리를 구하는 CalculateKnockback에서는 Weight를 아예 공식에서 빼버렸습니다! 오직 부딪히는 순간의 최종 속도(Speed)만을 기준으로 밀려나는 거리를 결정합니다.

그런데 림버스를 보면, 특정 환상체들은 움직이지 않습니다. (카르밀라, 초롱같은 친구들)

```csharp
        public KnockbackResult CalculateKnockback(ClashMover a, ClashMover b, MeetingResult meeting,
            float baseKnockbackDistance, float knockbackMultiplier)
        {
            var speedA = meeting.FinalVelA.magnitude;
            var speedB = meeting.FinalVelB.magnitude;
            var totalSpeed = speedA + speedB;

            var distanceRatioA = totalSpeed > Mathf.Epsilon ? speedB / totalSpeed : 0.5f;
            var distanceRatioB = totalSpeed > Mathf.Epsilon ? speedA / totalSpeed : 0.5f;

            var speedRatioA = totalSpeed > Mathf.Epsilon ? speedA / totalSpeed : 0.5f;
            var speedRatioB = totalSpeed > Mathf.Epsilon ? speedB / totalSpeed : 0.5f;

            var dirA = ResolveKnockbackDir(meeting.FinalVelA, a.Transform.position, b.Transform.position,
                Vector3.back);
            var dirB = ResolveKnockbackDir(meeting.FinalVelB, b.Transform.position, a.Transform.position,
                Vector3.forward);

            var distA = baseKnockbackDistance * distanceRatioA;
            var distB = baseKnockbackDistance * distanceRatioB;

            return new KnockbackResult
            {
                TargetA = a.Transform.position + new Vector3(dirA.x * distA, 0f, dirA.z * distA),
                TargetB = b.Transform.position + new Vector3(dirB.x * distB, 0f, dirB.z * distB),

                ReboundA = speedA * speedRatioA * knockbackMultiplier,
                ReboundB = speedB * speedRatioB * knockbackMultiplier
            };
        }
```

해서 다음에 바로 설명할 `BattleManager`에서 `ExecuteClashSequence`의 `while`루프에서 이 구조체들을 가져다 쓰면서 다음 로직이 작동합니다.
   1. CalculateMeetingData로 만나는 지점 계산.
   2. ExecuteMovement로 충돌.
   3. CalculateKnockback으로 튕겨나갈 위치(TargetA)와 다음 반동 속도(ReboundA) 반환.
   4. 다시 ExecuteMovement로 뒤로 튕겨 나감.
   5. BattleManager에서 ReboundA를 다음 루프의 CurrentSpeed로 덮어씌움.
   6. BattleManager에서 currentKnockbackDuration에 0.4f (DecayFactor) 등을 곱해 시간을 깎아버림.
결과: 루프를 반복할수록 거리는 좁아지고, 시간은 짧아지며, 속도는 빨라져서 림버스 합처럼 "챙! 챙! 챙채채챙!!" 하고 미친 듯이 합을 겨루게 됩니다.

### [`Core/BattleManager.cs`](./Core/BattleManager.cs)
> 전투 시퀀스 코루틴 전체 총괄 + 포커스 슬로모션 연출

이 프로젝트의 백미인, `BattleManager`입니다. `BattleManager`는 `TargetingResolver`에서 최종적으로 나온 `BattleMatchup`을 받아와서 실질적인 전투 코루틴들을 처리합니다.

턴 1회의 생명주기를 코드와 함께 보겠습니다.

1. `ExecuteTurn()` → `TurnCoroutine()`
   - 교전이 N개면 N개의 코루틴을 병렬로 실행합니다.
   - 그러면 `_activeClusters` 카운터로 전부 끝냈는지 판별합니다. (이때, 각 `ProcessBattleSequence`가 끝날 때 `--` 해줍니다.)
   - 이때 카메라는 시작할 때는 `ShowAll`로 이동하고, 활성된 전투 유닛들 목록(`_currentActiveFighters`)을 카메라 타겟으로 삼습니다.

```csharp
foreach (var battle in turnBattles)
    StartCoroutine(ProcessBattleSequence(battle));   // 1. 교전마다 코루틴을 동시에 띄움

while (_activeClusters > 0) yield return null;        // 2. 전부 끝날 때까지 대기

SetBattleState(CheckResult); OnTurnEnded?.Invoke();   // 3. 결산 신호
```

2. `ProcessBattleSequence()` — 교전 1건 처리
   1. 코인 리셋 - `ResetCoins`로 양측 코인 상태 초기화
   2. 상호 배제 - `while (attackerChar.IsEngaged || defenderChar.IsEngaged) yield` → 같은 캐릭터가 두 교전에 동시에 끌려가지 않도록.
   3. 조기 종료 - 전투 전 이미 사망했으면 정리하고 `yield break`.
   4. 포커스 판정 - `isFocus = battle.ViewType == FocusTarget`. 포커스면 `Time.timeScale = 0.05`, 당사자 외 캐릭터 반투명·애니메이션 정지.
   5. 분기 - `MatchType`에 따라 `ExecuteClashSequence`(합) 또는 `ExecuteOneSidedSequence`(일방).
   6. 포커스 해제 - 1.5초 여운 후 `timeScale` 복구, 불투명도 원복.
   7. 원위치 복귀 - `ReturnToPosition`으로 시작 좌표로.
   8. 정리 - `IsEngaged` 해제, `_activeClusters--`, 카메라 타겟 갱신.

```csharp
if (isFocus)
{
    _isFocusSequenceActive = true;   // 포커스는 한 번에 하나만 (뮤텍스)
    Time.timeScale = 0.05f;          // 시간을 거의 멈춰 슬로모션
    // ... 당사자만 선명하게, 나머지 캐릭터는 반투명 처리 ...
}
```

3. `ExecuteClashSequence()` - 합!

코인이 양쪽 다 남은 동안은 계속해서 반복합니다.

   1. `ClashMover` 구조체 빌드 → `actionController.CalculateMeetingData()` — 가속하며 접근할 때 만나는 지점·시간 예측. 여기서 앞서 설명한 `actionController`의 내부 로직들이 사용됩니다.
   2. 양측을 만남 지점까지 `ExecuteMovement`로 동시 이동 (`approachA`/`approachD` 둘 다 대기).
   3. 합 모션 재생 → `CoinResolver.TossSlotCoinsRoutine` 2개를 동시에 띄우고 `WaitUntil`로 둘 다 끝나길 대기.
   4. 위력 비교 → 진 쪽 `CoinResolver.BreakSlotCoin` + 양측 ChangeSp.
   5. `CalculateKnockback` → 넉백 위치로 이동.
   6. 다음 합을 위해 속도 증폭(`speedProgressionFactor`), 넉백 거리·시간 감쇠.

이 루프가 종료되면, 코인이 남은 쪽이 `ApproachForAttack` + `ExecuteMultiHitAttack` 남은 코인 개수만큼 줘팹니다.

```csharp
StartCoroutine(CoinResolver.TossSlotCoinsRoutine(attackerSlot, dur, r => { powerA = r; doneA = true; }, isFocus));
StartCoroutine(CoinResolver.TossSlotCoinsRoutine(targetSlot,   dur, r => { powerD = r; doneD = true; }, isFocus));
yield return new WaitUntil(() => doneA && doneD);   // 양측 토스가 끝나야 위력 비교
```

4. `ExecuteOneSidedSequence()` — 일방 공격
   - 접근 → ExecuteMultiHitAttack → 타겟 넉백.

5. `ExecuteMultiHitAttack()` — 실제 타격
   - `CoinResolver.TossSingleCoin`으로 위력 누적 → 공격음 → 공격 모션(대시 여부) → vfx → `coin.customLogic`이 있으면 위임(`StandardHit`/`EscalatingHit`/`SpearAttack`), 없으면 내장 기본 타격 루프(`TakeDamage` 반복).

```csharp
if (coin.customLogic != null)
    yield return StartCoroutine(coin.customLogic.Execute(...));  // 특수 스킬 → SO에 위임
else
    { /* 내장 기본 타격 루프 */ }
```

이렇게 구성되어 있습니다. 이벤트 기반으로 다른 시스템과 효율적으로 소통할 수 있게 설계하엿고, 코루틴 팬아웃/조인 + 콜백 동기화로 복잡한 병렬 연출을 제어하고 있습니다. 그리고 애초에 코인이나 물리 로직을 분리해서 객체지향의 핵심인 단일책임 원칙을 고수하고자 노력했습니다.

다만... 여전히 이 BattleManager는 너무 크고, 사실 개별적인 일방공격과 합은 분리하는게 더 좋습니다. 그리고 무엇보다 `_activeClusters` 같은 상태들을 코루틴이 공유중인데 이는 분명히... 코루틴 수동 카운팅이 되므로 실수의 여지가 너무나 많습니다. 이는 추후에 async/await나 구조화된 동시성으로 수정할 계획입니다.

이것을 구현하는 과정에서 코루틴의 팬아웃(Fan-out)과 조인(Join)에 대해 알게되었습니다.
   - 팬아웃(Fan-out) = 하나의 지점에서 여러 작업을 한꺼번에 퍼뜨려 띄우는 것 (1 → N 병렬).
   - 조인(Join) = 그 N개가 전부 끝날 때까지 기다렸다가 다음으로 넘어가는 것 (N → 1 합류).

이 BattleManager에서는 이 기법을 배운것만으로도, 충분히 가치있고 너무 재미있는 시간이었습니다.

## Chapter 6. 카메라 연출
> 실제 림버스에서 타격감과 연출에 중요한 부분의 한 축인, 엄청 공들여서 만든 것 같은 카메라 시스템을 미약하게나마 따라해보았습니다.

림버스 전투 카메라가 특히 분석하기 재미있었던 점이, 3D 월드에서의 2.5D 표현 방식이 좋았기 때문입니다. 2.5D 게임을 하나 취미로 만들고 있던 중에, 이런 기법은 어디서 배울까 고민을 많이 했습니다. 아쉽게도 림버스가 먼저 떠오른건 아니었습니다. ![`Tails Noir Preludes`](https://store.steampowered.com/app/2020030/Tails_Noir_Preludes/) 언리얼 엔진을 공부하면서 여러 관련 영상을 보던 중, 찾게된 게임입니다. 2.5D 방식이면서 3D를 부드럽고 아름답게 표현한 게임으로, 그림자, 광원까지 정말 보는 맛이 있는 게임이었습니다.
이 방식을 먼저 숙지하고(아쉽게도 언리얼 엔진은 미숙해서 언리얼에서 구현은 하지 못했습니다.) 림버스를 관찰하자(거울던전 돌리기 X 9999) 점점 하나씩 보이기 시작했습니다.
그리고 이를 바탕으로, 간략하게나마 카메라 시스템을 구현해보았습니다.

접근한 요소는 크게 세가지입니다.

1. 평소 카메라의 셋팅은 어떻게 되는가? -> 모든 캐릭터와 적이 카메라에 담겨야함.
2. 전투중 카메라 셋팅은 어떻게 되는가? -> 당연히 모든 캐릭터와 적이 카메라에 담겨야 하지만, 최대 줌 아웃 거리는 존재함.
3. 전투중 카메라 셋팅, 3스킬은 어떻게 되는가? -> 공격자와 피격자를 중심으로 담아내고, 주변 타임 스케일은 느리게(자-워르도)만든 다음, 두 캐릭터의 연출에 집중한다.

이 세가지 요소 구현에 초점을 두었습니다.

### [`Cameras/CameraTracker.cs`](./Cameras/CameraTracker.cs)
> 타겟 Bounds·중심점 추적

카메라 트래커는 오직 중심점(`CurrentCenter`)과 화면 크기(`CurrentTargetSize`)만 계산하는 두뇌 역할만 담당합니다. 그래서 이 스크립트는 특히, 다른 스크립트에서 값을 가져다가 쓰기 전에 로딩시킬 필요가 있었습니다.
`[DefaultExecutionOrder(-100)]`은 이를 위해서 사용했습니다. 스크립트의 실행 순서를 맨 앞으로 당기는 명령어로, 다른 스크립트들이 반드시 의존해야 하는 스크립트에 사용합니다.

고민이 든 것이, '그럼 캐릭터들을 전부 담아내야 하는데 그 경계는 어떻게 정할까?' 였습니다. 콜라이더를 생성하자니 이건 좀 아닌 것 같고. 그렇게 찾다 찾다 나온 방법이 유니티의 `Bounds(경계 상자) 구조체`였습니다.

```csharp
bounds.Encapsulate(t.position);
var maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
CurrentTargetSize = maxSize > 0.1f ? maxSize : 5f;
```

`Encapsulate`를 호출할 때마다 타겟들을 모두 포함하는 투명한 상자가 커지며, 이 상자의 가장 긴 길이(`maxSize`)를 구해 카메라 컨트롤러에게 "이 정도 크기니까 화면을 이만큼 줌아웃해!"라고 지시(`CurrentTargetSize`)합니다. 어차피 이 투명 상자는 가장 큰걸로 하나만 있으면 되는거니까요.

그리고 여기에서 중심점만 바라봅니다. 앞선 `BattleManager`에서 `actionController.CalculateMeetingData` 구한 좌표를 이곳에 전달합니다. 그러면 '두 캐릭터 사이의 충돌 지점'을 카메라가 중심점으로 이동하게 됩니다. 여기에 추가적인 줌인, 줌아웃을 추가한다면 더 역동감있는, 림버스 카메라 연출이 될 겁니다. (와!)

즉, 이 `CameraTracker`는 카메라를 움직이지 않고, 중심점과 크기만 게산하는 계산기입니다.

### [`Cameras/CameraController.cs`](./Cameras/CameraController.cs)
> FOV 기반 동적 줌 거리 계산

Tracker가 계산을 했다면, `CameraController`가 이름값을 할 차례입니다. 

```csharp
            var distanceForHeight = targetSize * config.padding / (2.0f * tanHalfFov);
            var distanceForWidth = distanceForHeight / _cam.aspect;
```

1. 카메라에서 거리 d만큼 떨어진 지점에서 화면에 보이는 세로 높이는 2 · d · tan(FOV/2)입니다.
2. 그러니 크기 H인 대상을 다 담으려면 d = H / (2·tan(FOV/2)). 여기에 아무래도 딱 붙으면 보기 싫으니 padding을 곱해서 여유를 줍니다.
3. 이제 H를 구했으니 W를 구합니다. 유니티의 _cam.aspect는 W*H입니다. 그리고 기본적으로 제공해줍니다. 그러니 여기에 H를 곱해주기만하면, 복잡하게 atan 역산할 필요 없이 값이 나옵니다.

<img width="1920" height="1080" alt="높이계산" src="https://github.com/user-attachments/assets/9f193372-1382-4d31-8ec7-5b2f7dcf3934" />

<img width="1920" height="1080" alt="넓이 계산" src="https://github.com/user-attachments/assets/f87ad2be-f88c-45ce-ae42-ee21324ccf1a" />

이렇게 나온 값중 최대값을 찾습니다.
```csharp
            var requiredDistance = Mathf.Max(distanceForHeight, distanceForWidth);
            requiredDistance = Mathf.Clamp(requiredDistance, minZoomDistance, maxZoomDistance);
```

- distanceForHeight = 대상이 화면을 세로로 정확히 꽉 채우는 거리.
- distanceForWidth = 대상이 화면을 가로로 정확히 꽉 채우는 거리.

카메라는 이제 이 두 개의 값 중, 더 큰 값을 새로운 d 값으로 설정하여 모든 유닛들이 카메라에 전부 나오게 셋팅합니다. 위아래까지 계산한 이유는 아무래도 카르밀라와 같은 환상체도 그렇고 대부분의 환상체가 위아래로도 크므로, 이렇게 설정했습니다. 또한, 만일 카메라의 줌인 줌아웃 최소와 최대 거리를 넘어가거나 당겨지면 안되므로, `requiredDistance = Mathf.Clamp(requiredDistance, minZoomDistance, maxZoomDistance);`를 통해 최소 최대값을 조절해주었습니다.

해서 이를 통해 위치는 `Vector3.SmoothDamp`, 회전은 `Quaternion.Slerp`으로 부드럽게 이동하고, 둘 다 `Time.unscaledDeltaTime` 사용합니다. 이는 당연하게도 포커스 슬로모션(`timeScale 0.05`) 중에도 카메라는 정상 속도로 움직여야 연출이 맛갈나기 때문입니다. 

### [`Cameras/CameraMathVisualizer.cs`](./Cameras/CameraMathVisualizer.cs)
> 에디터 기즈모 디버깅 툴

[ExecuteInEditMode] + OnDrawGizmos — 플레이하지 않고도 씬 뷰에서 카메라 수학을 눈으로 보는 도구입니다. 각 진영 중심점(구), Bounds(와이어큐브), 카메라 절두체(frustum) 라인을 그려줍니다. 이건 앞선 `CameraController` 식을 공부할 때 사용했습니다. 아무래도 숫자로 계산하는것도 좋지만 실제 눈으로 보이는 것 만큼 좋은건 없으니 이렇게 짜보았습니다.

### [`Cameras/Billboard.cs`](./Cameras/Billboard.cs)
> 2D 스프라이트가 3D 월드에 존재하기 위해 반드시반드시반드시 필요한 빌보드 기법

2D 스프라이트나 UI 캔버스는 수학적으로 두께가 0인 완벽한 평면입니다. 해서 만약에 3D 공간에서 카메라가 호오오옥시나 캐릭터의 옆으로 이동하거나 회전했을 때 이 평면이 카메라를 따라 돌지 않으면 너무나 얇은 그들의 실체를 마주하게 됩니다(무섭따). 사실, 언제나 앞면만 보여주니깐 큰 문제는 없지만 만에 하나 라는 경우도 있으니 부착해두었습니다.

그래서 캐릭터의 회전값(`transform.rotation`)을 메인 카메라의 회전값(`_mainCamera.transform.rotation`)과 완벽하게 동일하게 맞춰치켜 세움으로써, 카메라가 3D 공간의 어디로 이동하든 항상 스프라이트의 정면 이미지만을 렌더링하게 만들었습니다.

다만 처음에는 `transform.LookAt(_mainCamera.transform.position)`을 사용했는데 이게... 화면 가장자리로 밀려난 캐릭터가 짜부가 되어버렸습니다. 이는 찾아보니 다음과 같았습니다.
 - LookAt의 문제점: 카메라 렌즈의 '중심점'을 향해 각도를 틀어버립니다. 만약 캐릭터가 화면 구석에 있다면, 카메라 렌즈를 쳐다보기 위해 몸을 비스듬하게 틀게 되고, 이로 인해 2D 원본 이미지의 비율이 투시(Perspective)에 의해 찌그러집니다.
그래서, 카메라 렌즈 면과 완벽하게 평행(Parallel)하도록 `transform.rotation = _mainCamera.transform.rotation;`로 구현했습니다. 여기에 추가적으로, 카메라 실행 순서에서 잘못하면 드르르르르르륵 덜덜덜덜덜덜덜덜 거리므로... 당연히 `LateUpdate`를 사용했습니다.

## Chapter 7. 플레이 필드 & 경제 시스템
> 인게임상의 플레이 필드(보드)와 경제 시스템을 관리

### [`Shop/PlayerManager.cs`]()
> 코스트 시스템 관리

현재 코스트, 시작 코스트, 코스트 판매를 담당합니다.

### [`Shop/ShopManager.cs`]()
> 상점 리롤·구매·업그레이드

상점 새로고침, 구입, 상점 레벨 기반으로 레어도에 기반한 확률 유닛 제공을 담당합니다. 

### [`Board/HandManager.cs`]()
> 핸드 관리

최대 10개의 핸드만을 가질 수 있게 구성했습니다. 각각 유닛 데이터를 가지고 있으며 HandLayout 하위에 프리팹으로 handSlot 유닛들을 생성한 뒤, 정렬합니다.

### [`Board/FieldManager.cs`]()
> 필드 관리

필드를 관리합니다. 필드는 다음 사진처럼 구성되어있습니다.

<img width="1501" height="717" alt="Field" src="https://github.com/user-attachments/assets/827d1943-a800-4f6f-82ec-03f45b014d34" />

일전에 `TargetingArrow`를 설명할때 핵심 로직은 설명했으니, 가볍게 설명하겠습니다. 필드는 기본적으로 적이 소환될 공간, 플레이어가 소환될 공간으로 나뉩니다. 각각의 공간에 '어디에' 소환될지는 필드에 사전 준비해두었습니다.
사진에 보이는 커다란 사각형은 FieldZone 태그가 부착된 콜라이더입니다. 앞에서 설명했 듯, 이곳에 레이케스팅이 되면 필드로 유닛을 배치(`AddUnitToField`)합니다.
배치는 간단합니다. 만일 배치가 가능하다면, 외부의 드래그 앤 드롭 스크립트에서 레이캐스트(`Raycast`)를 통해 알아낸 마우스의 최종 위치를 `dropPosition`으로 넘겨주면, 필드 매니저는 2D 평면 기준(`Vector2.Distance`)으로 그 위치와 가장 가까운 '빈' 슬롯을 수학적으로 찾아냅니다.

```csharp
private Transform GetClosestEmptySlot(Vector3 position)
{
    // ...
    foreach (var slot in playerSlots)
    {
        if (_slotOccupancy[slot] != null) continue; // 이미 누가 있으면 패스!

        var slotPos2D = new Vector2(slot.position.x, slot.position.y);
        var dist = Vector2.Distance(dropPos2D, slotPos2D); // 드롭한 위치와 슬롯의 거리 계산!

        if (dist < minDistance) // 가장 거리가 짧은(가까운) 슬롯을 갱신!
        {
            minDistance = dist;
            closestSlot = slot;
        }
    }
    return closestSlot;
}
```

왜 이렇게 구현 했느냐? UX 때문입니다. 사실 속도가 랜덤으로 정해지므로 별 의미는 없지만, 림버스 카메라 워킹을 100% 구현한건 아니기 때문에 이 게임의 카메라 구도는 솔직히 좀 구립니다. 사실 그걸 떠나서, 이건 앞에 두고 이건 뒤에두고 하는 방식으로 '플레이어가 마치 이러한 요소 하나하나'를 무의식적으로 변수로 인식하게 만들어 몰입도를 높이고 싶었습니다.

그리고 이렇게 배치된 플레이어를 단순히 리스트로만 관리하지는 않았습니다.
```private readonly Dictionary<Transform, Player> _slotOccupancy = new();``` 
딕셔너리를 활용하여 슬롯(Transform)을 Key로, 플레이어(Player)를 Value로 연결해서 특정 슬롯이 비어있는 상황을 체크할때는 리스트 전부를 뒤지지 않게 처리했습니다.

거기에 더해, 매 턴이나 매 프레임마다 `GetAllUnits()`를 호출할 때 `new List<UnitData>()`를 계속 생성하면 유니티의 GC에 과부하가 걸려 게임에 렉이 발생합니다. 실제로 이 부분을 프로파일링으로 찾았을 때, 문제가 컸습니다. 그래서, 클래스 멤버로 캐싱 리스트를 만들어두고 `.Clear()`로 재활용하는 방식으로 리스트를 캐싱했습니다.

이것도 한번 테스트해봤습니다.

```csharp
using System.Collections.Generic;
using Scenes.Battle.Data;
using UnityEngine;
using UnityEngine.Profiling;

public class ListProfilingTest : MonoBehaviour
{
    private List<UnitData> _cachedList = new List<UnitData>();
    private List<UnitData> _dummyDataList = new List<UnitData>();

    [Header("테스트 설정")]
    [Tooltip("한 프레임당 함수를 몇 번 반복할 것인가?")]
    public int loopCount = 10000; 

    private void Start()
    {
        // 테스트를 위한 더미 데이터 10개 생성
        for (int i = 0; i < 10; i++)
        {
            _dummyDataList.Add(ScriptableObject.CreateInstance<UnitData>());
        }
    }

    private void Update()
    {
        // 매번 new List를 생성 (GC 유발)
        Profiler.BeginSample("1. Bad_NewList_Test"); // 프로파일러에 표시될 이름
        for (int i = 0; i < loopCount; i++)
        {
            BadGetAllUnits();
        }
        Profiler.EndSample();

        // 캐싱된 리스트를 재활용 (GC 방지)
        Profiler.BeginSample("2. Good_CachedList_Test"); // 프로파일러에 표시될 이름
        for (int i = 0; i < loopCount; i++)
        {
            GoodGetAllUnits();
        }
        Profiler.EndSample();
    }

    // 매번 메모리 할당
    private List<UnitData> BadGetAllUnits()
    {
        var list = new List<UnitData>();
        foreach (var data in _dummyDataList)
        {
            list.Add(data);
        }
        return list;
    }

    // 캐싱 및 Clear
    private List<UnitData> GoodGetAllUnits()
    {
        _cachedList.Clear();
        foreach (var data in _dummyDataList)
        {
            _cachedList.Add(data);
        }
        return _cachedList;
    }
}
```

<img width="2283" height="332" alt="Bad" src="https://github.com/user-attachments/assets/40303dc6-3fa6-4989-89c0-5c017a3f6565" />

<img width="2284" height="334" alt="Good" src="https://github.com/user-attachments/assets/90f526f9-6688-4515-a746-8bf233c31124" />


여기에서는 다음 항목을 볼 수 있습니다.
   1. GC Alloc: 이번 프레임에 Heap 메모리에 생성된 Garbage의 양을 뜻합니다.
      1. List: 3.4MB
      2. Dict: 320B
   2. Time ms: 해당 함수 처리를 위해 CPU가 소모한 순수 시간을 뜻합니다.
      1. List: 11.13
      2. Dict: 7.24

이렇게 사실 이렇게까지 프레임마다 10000씩 반복은 하지 않겠지만, 이렇게 하나하나 잡을 수 있는 요소는 잡아야 최적화가 된다고 생각하고, 진행해보았습니다.

### [`Board/MergeManager.cs`](./Board/MergeManager.cs)
> 3개 합성 + 전투 중 큐 처리

오토체스 류 게임의 특징을 그대로 살리고자 노력했습니다.
1. 전투중이 아니라면, 유닛을 구매했을 때 필드와 핸드를 분석해서 합칠 수 있는 것들은 합친다,.
2. 전투중이라면, 전투가 끝나는 즉시 합체된다.

`CheckMerge`: 합칠 수 있는지 검토하는 메서드입니다. 다음 순서로 작동됩니다.
   1. 핸드와 필드를 쭉 탐색하며 `GetMatchingUnits` 함수를 통해 동일한 성급과 아이디의 유닛을 탐색합니다. 
   2. 이게 만약 3장 이상이라면, 거기에 Idle 상태라면 바로 합쳐버립니다. 만약 전투중이라면, Queue에 등록해두고, 전투가 끝나면 바로 합성 가능하게 대기시킵니다.
      - 그런데 만약에, 이 합성시키려는 본체를 팔아버렸다? 그럼 전투가 끝나고 진행되는 `ProcessQueue`에서 `queuedUnit == null` 검사를 통해 막아버립니다.
   3. 그럼 ExecuteMerge가 연쇄적으로 작용하며, 내부에서 CreateUpgradedUnit를 다시 호출하고 -> 여기서는 CheckMerge를 다시 호출해서 연쇄적으로 합성이 일어나는 개쩌는 **재귀**를 구현했습니다. (찢었다. 내가 해냄.)

## Chapter 8. UI 계층
> 앞선 로직들이 실제로 연결된 UI
> UI는 인스펙터 할당 그리고 프리팹 생성 및 조율이므로 별도의 설명은 첨부하지 않겠습니다.

[전투 영상](https://youtu.be/t6fYwb53-BY)

### [`UI/CardUI.cs`](./UI/CardUI.cs)
> 상점에 나타나는 카드 UI

### [`UI/ShopUI.cs`](./UI/ShopUI.cs)
> 상점 UI

### [`UI/HandSlotUI.cs`](./UI/HandSlotUI.cs)
> 구매한 유닛들이 보관되는 HandSlot UI

### [`UI/SellZoneUI.cs`](./UI/SellZoneUI.cs)
> 판매존 UI

### [`UI/TopPanelUI.cs`](./UI/TopPanelUI.cs)
> 스테이지 정보, 배틀 시작 버튼이 위치한 TopPanel UI

### [`UI/ToolTipUI.cs`](./UI/ToolTipUI.cs)
> 마우스 호버시 나타날 ToolTipUI

### [`UI/GameOverUI.cs`](./UI/GameOverUI.cs)
> 게임 종료를 관리하는 UI


## Chapter 9. 마무리
> 최종적으로 마무리합니다. (마참내!)

### [`Controller/BattleSceneUIController.cs`](./Controller/BattleSceneUIController.cs)
> 씬 연출 및 페이드

- 앞선 씬들과 마찬자기로 단순히 씬의 진입과 퇴장 연출만을 관리합니다.

### [`Core/BattleDirector.cs`](./Core/BattleDirector.cs)
> 게임 루프 관리

마지막으로 `BattleDirector`는 게임을 총괄합니다. 메인 게임 루프를 관리하여 게임의 모든 흐름을 관리합니다.

1. 메인 게임 루프 (Game Loop) 통제
우선 크게, 스폰 -> 전투 -> 결산 -> 정비의 흐름을 이벤트 기반으로 제어하고있습니다.
   - 이벤트 구독: OnEnable과 OnDisable에서 battleManager.OnTurnEnded 이벤트를 구독/해제하여 메모리 누수를 방지하고 있습니다.
   - 역순 순회(Reverse for-loop)를 통한 안전한 삭제: CheckTurnResult()에서 죽은 캐릭터를 리스트에서 제거할 때 `for (var i = players.Count - 1; i >= 0; i--)`처럼 뒤에서부터 순회하는 것은 리스트 조작 시 발생할 수 있는 인덱스 꼬임(Index Out of Range) 버그를 방지하고 있습니다.
   - 상태 복구 로직: 승리하여 다음 라운드로 넘어갈 때, 생존한 기물들에게 `SetupUnit`을 다시 호출하여 체력과 스탯을 꽉 채워주고 `ResetStateAndPosition()`으로 제자리로 돌려보내는 정비 시스템을 구현했습니다. 실제 림버스에서는 물론, 버프도 디버프도 그리고 체력과 SP도 유지하지만 여기에선 어디까지나 비슷하게 구현하는 것을 목표로 했기에, 그리고 오토체스류 게임의 특징을 살리기 위해 이렇게 구현했습니다.
2. 오토배틀러 경제 시스템 (이자 & 수입)
게임 밸런스를 정말 어렵게 잡아놨기에, 이자는 필수입니다.
   - 최신 C# 스위치 식(Switch Expression): `> 50 => 5`와 같이 가독성 높은 패턴 매칭을 사용하여 "보유 골드의 10%를 이자로 주되, 최대 5골드까지만 지급한다"는 TFT(롤토체스) 스타일의 이자 공식을 처리했습니다.
   - 턴 종료 보상: 이자와 함께 기본 수입(+5)을 지급하고, 상점을 무료로 한 번 리롤(`shopManager.RerollShop()`)해주며, 전투 중 밀려있던 합성 대기열(`mergeManager.ProcessQueue()`)을 처리합니다.
3. 전투 보상 및 페널티 (SP & 킬 보상)
림버스와 마찬가지로, 상대를 죽이면 SP가 증가되게 설정했습니다. 물론 당연히 아군이 죽어도 깎입니다. 거기에 오토체스라면 당연히 상대를 잡았으면 코스트가 증가해야하므로, 보상도 넣어두었습니다.
4. 플레이어가 아무것도 할 수 없어 게임이 멈춰버리는 현상(소프트락)을 예방하기위해 안전장치를 해두었습니다.
단순히 "필드에 유닛이 없으면 패배"로 처리하지 않습니다. 
   1. 필드에 유닛이 없고,
   2. 핸드에도 낼 유닛이 없고, 
   3. 상점에서 가장 싼 유닛을 살 돈조차 없을 때(currentCost < minUnitCost) 비로소 "더 이상 소생 불가능한 상태"로 판단하여 게임 오버를 트리거합니다. `CanStartCombat`의 3중 체크를 여기서 처리합니다.
