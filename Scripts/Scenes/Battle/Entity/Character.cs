using System.Collections.Generic;
using Core.Data.Enums;
using Core.Managers;
using Scenes.Battle.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Scenes.Battle.Entity
{
    public class Character : MonoBehaviour
    {
        public string Name { get; private set; }
        public int MinSpeed { get; private set; } = 1;
        public int MaxSpeed { get; private set; } = 10;
        public int Hp { get; private set; }
        public int Sp { get; private set; }
        public int Speed { get; private set; }
        public int Weight { get; private set; }
        public int Strength { get; private set; }
        public float CombatRange { get; private set; } = 1.5f;
        public List<Skill> AvailableSkills { get; private set; } = new();
        public bool IsImmovable { get; private set; }


        // HomePosition은 필드에 캐릭터 놓을때 결정되는 요소이므로 private 금지
        public Vector3 HomePosition { get; set; }

        public UnitData currentUnitData;
        public int ActionSlotCount { get; private set; }
        public bool IsEngaged { get; set; }

        // 이 프리팹의 자식 오브젝트 참조 → 인스펙터에서 직접 연결
        [field: SerializeField] public GameObject CameraTarget { get; private set; }

        [Header("Animation & Rendering")]
        public SpriteRenderer spriteRenderer;

        [Header("Info UI")]
        [SerializeField] public GameObject characterInfoUI;
        [SerializeField] public GameObject[] starImage = new GameObject[3];
        [SerializeField] public TextMeshProUGUI hp;
        [SerializeField] public TextMeshProUGUI sp;

        [Header("Skill & Coin UI")]
        [SerializeField] public GameObject skillUI;
        [SerializeField] public Image skillImage;
        [SerializeField] public GameObject[] coinsImage = new GameObject[5];
        [SerializeField] public TextMeshProUGUI totalValue;
        private CanvasGroup _skillCanvasGroup;

        public enum CoinUIState { Ready, Front, Back, Broken }

        [Header("Coin UI Settings")]
        [SerializeField] private Sprite coinReadySprite;
        [SerializeField] private Sprite coinFrontSprite;
        [SerializeField] private Sprite coinBackSprite;
        [SerializeField] private Sprite coinBrokenSprite;

        [Header("Audio & Sounds")]
        [SerializeField] private AudioSource characterAudioSource;
        [SerializeField] private BattleSoundType deathSound = BattleSoundType.Willhelm;
        [SerializeField] private BattleSoundType coinBreakSound = BattleSoundType.CoinBreak;

        public BattleSoundType DeathSound => deathSound;
        public BattleSoundType CoinBreakSound => coinBreakSound;

        private bool _isDead;

        protected Animator Anim;
        public int StarLevel { get; private set; } = 1;
        private int _activeCoinLayoutCount;

        protected virtual void Awake()
        {
            Anim = GetComponent<Animator>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            if (characterAudioSource == null) characterAudioSource = GetComponent<AudioSource>();
            if (characterAudioSource != null)
            {
                characterAudioSource.spatialBlend = 0.5f; // 3D 사운드 강제 적용
                characterAudioSource.spread = 150f; // 좌우 패닝 완화
            }

            if (skillUI != null)
            {
                _skillCanvasGroup = skillUI.GetComponent<CanvasGroup>();
                if (_skillCanvasGroup == null)
                    _skillCanvasGroup = skillUI.AddComponent<CanvasGroup>();
            }
        }

        public void PlayCharacterSfx(SfxSoundType sfxType)
        {
            if (sfxType == SfxSoundType.None || characterAudioSource == null) return;

            if (SoundManager.Instance.GetSfx(sfxType, out var clip))
            {
                characterAudioSource.PlayOneShot(clip);
            }
        }

        public void PlayCharacterSfx(BattleSoundType sfxType)
        {
            if (sfxType == BattleSoundType.None || characterAudioSource == null) return;

            if (SoundManager.Instance.GetBattleSound(sfxType, out var clip))
            {
                characterAudioSource.PlayOneShot(clip);
            }
        }

        public void TakeDamage(int damage, BattleSoundType hitSound)
        {
            if (_isDead) return;

            PlayCharacterSfx(hitSound); // 피격음: 대미지가 들어오는 바로 이 순간 재생

            Hp -= damage;
            if (Hp <= 0)
            {
                Hp = 0;
                _isDead = true;
                PlayCharacterSfx(DeathSound);

                // 사망 시 시각적 식별을 위해 스프라이트를 회색으로 처리
                if (spriteRenderer != null) spriteRenderer.color = Color.gray;

                HideSkillUI(); // 사망 시 남은 코인 UI 즉시 제거
            }
        }

        #region Initialization & Data Binding

        public void SetupUnit(UnitData data, int starLevel = 1)
        {
            currentUnitData = data;
            StarLevel = starLevel;

            if (Anim != null && currentUnitData.overrideController != null)
                Anim.runtimeAnimatorController = currentUnitData.overrideController;

            ApplyUnitData();
        }

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
            IsImmovable = currentUnitData.baseStats.isImmovable || currentBonus.addedStats.isImmovable;

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

        public void UpgradeStar()
        {
            if (StarLevel >= 3) return;
            StarLevel++;
            ApplyUnitData();
            Debug.Log($"[{Name}] {StarLevel}성으로 업그레이드 완료!");
        }

        #endregion

        #region Battle Mechanics

        public void RollBattleSpeed()
        {
            Speed = Random.Range(MinSpeed, MaxSpeed + 1);
        }

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

        public void ResetCoins(Skill skill)
        {
            if (skill.coins == null) return;

            for (int i = 0; i < skill.coins.Count; i++)
            {
                // 1. 데이터 초기화
                var coin = skill.coins[i];
                coin.isBroken = false;
                skill.coins[i] = coin;

                // 2. UI 초기화 (기존의 UpdateCoinUI 메서드 활용)
                // CoinUIState.Normal 혹은 기본 대기 상태로 변경
                UpdateCoinUI(i, CoinUIState.Ready);
            }
        }

        public virtual void ChangeSp(int amount)
        {
            Sp = Mathf.Clamp(Sp + amount, -45, 45);
        }

        #endregion

        #region Actions & Animation

        public void SetMoving(bool isMoving)
        {
            if (Anim != null) Anim.SetBool("IsMoving", isMoving);
        }

        public virtual void ExecuteAttackMotion(string triggerName)
        {
            if (Anim != null && !string.IsNullOrEmpty(triggerName)) Anim.SetTrigger(triggerName);
        }

        public virtual void ExecuteClashMotion(string clashTrigger)
        {
            ExecuteAttackMotion(clashTrigger);
        }

        public virtual void Attack()
        {
        }

        public virtual void Action()
        {
        }

        public virtual void Defend()
        {
        }

        public virtual void Avoid()
        {
        }

        public virtual void CounterAttack()
        {
        }

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

        #endregion

        #region UI Rendering

        public virtual void SetInfoUI()
        {
            if (hp != null) hp.text = $"{Hp}";
            if (sp != null) sp.text = $"{Sp}";
            if (starImage is { Length: >= 3 }) starImage[StarLevel - 1].SetActive(true);
        }

        public virtual void ShowInfoUI()
        {
            if (characterInfoUI != null) characterInfoUI.SetActive(true);
        }

        public virtual void HideInfoUI()
        {
            if (characterInfoUI != null) characterInfoUI.SetActive(false);
        }

        public virtual void ShowSkillUI(Skill activeSkill)
        {
            if (skillUI != null) skillUI.SetActive(true);

            UpdateTotalValueText(activeSkill.basicValue);

            if (skillImage != null)
            {
                skillImage.sprite = activeSkill.skillIcon;
                skillImage.color = activeSkill.skillColor;
            }

            _activeCoinLayoutCount = activeSkill.coins?.Count ?? 0;

            // 1. 코인 개수에 맞는 전용 레이아웃만 활성화하고 나머지는 비활성화
            for (var i = 0; i < coinsImage.Length; i++)
            {
                if (coinsImage[i] != null)
                {
                    // i는 0부터 시작하므로 개수와 맞추려면 i + 1과 비교
                    coinsImage[i].SetActive((i + 1) == _activeCoinLayoutCount);
                }
            }

            if (_activeCoinLayoutCount == 0) return;

            // 2. 활성화된 레이아웃 내부의 개별 코인 상태 초기화
            for (var i = 0; i < _activeCoinLayoutCount; i++)
            {
                var isBroken = activeSkill.coins[i].isBroken;
                UpdateCoinUI(i, isBroken ? CoinUIState.Broken : CoinUIState.Ready);
            }
        }

        public virtual void HideSkillUI()
        {
            if (skillUI != null) skillUI.SetActive(false);
            UpdateTotalValueText(0); // 텍스트 초기화
            _activeCoinLayoutCount = 0;
        }

        public virtual void UpdateCoinUI(int coinIndex, CoinUIState state)
        {
            if (_activeCoinLayoutCount <= 0 || _activeCoinLayoutCount > coinsImage.Length) return;
            if (coinIndex < 0 || coinIndex >= _activeCoinLayoutCount) return;

            // 현재 켜져 있는 레이아웃 게임오브젝트 가져오기
            var activeLayout = coinsImage[_activeCoinLayoutCount - 1];
            if (activeLayout == null) return;

            // 해당 레이아웃의 자식(Child) 객체 중 coinIndex번째 객체의 Image 컴포넌트 접근
            if (coinIndex >= activeLayout.transform.childCount) return;

            var img = activeLayout.transform.GetChild(coinIndex).GetComponent<Image>();
            if (img == null) return;

            switch (state)
            {
                case CoinUIState.Ready:
                    img.sprite = coinReadySprite;
                    break;
                case CoinUIState.Front:
                    img.sprite = coinFrontSprite;
                    break;
                case CoinUIState.Back:
                    img.sprite = coinBackSprite;
                    break;
                case CoinUIState.Broken:
                    img.sprite = coinBrokenSprite;
                    break;
            }
        }

        public void UpdateTotalValueText(int value)
        {
            if (totalValue != null)
            {
                totalValue.text = value.ToString();
            }
        }

        #endregion

        public void SetOpacity(float alpha)
        {
            if (spriteRenderer != null)
            {
                var color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }
        }

        public void SetSkillUIOpacity(float alpha)
        {
            if (_skillCanvasGroup != null)
            {
                _skillCanvasGroup.alpha = alpha;
            }
        }

        public void SetAnimatorUnscaledTime(bool isUnscaled)
        {
            if (Anim != null)
            {
                Anim.updateMode = isUnscaled ? AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.aquamarine;
            Gizmos.DrawWireSphere(transform.position, CombatRange);
        }
    }
}