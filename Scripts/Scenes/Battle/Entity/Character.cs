using System;
using System.Collections.Generic;
using System.Linq;
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
        [Header("Identity & Core Stats")]
        [field: SerializeField] public int Index { get; set; }

        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public float CombatRange { get; set; } = 1.5f;
        [field: SerializeField] public int MinSpeed { get; set; } = 1;
        [field: SerializeField] public int MaxSpeed { get; set; } = 10;

        [Header("Battle Status")]
        [field: Min(0f)] [field: SerializeField]
        public int Hp { get; set; }

        [field: Range(-45, 45)] [field: SerializeField]
        public int Sp { get; set; }

        [field: SerializeField] public int Speed { get; set; }
        [field: SerializeField] public int Weight { get; set; }
        [field: SerializeField] public int Strength { get; set; }
        [field: SerializeField] public List<Skill> AvailableSkills { get; set; } = new();
        public UnitData currentUnitData;
        public int ActionSlotCount { get; private set; }
        public bool IsEngaged { get; set; }

        [Header("Animation & Rendering")]
        public SpriteRenderer spriteRenderer;
        [field: SerializeField] public GameObject CameraTarget { get; set; }

        [Header("Pose")]
        [field: SerializeField] public Vector3 HomePosition { get; set; }

        [Header("Info UI")]
        [field: SerializeField] public GameObject characterInfoUI;
        [field: SerializeField] public GameObject[] starImage = new GameObject[3];
        [field: SerializeField] public TextMeshProUGUI hp;
        [field: SerializeField] public TextMeshProUGUI sp;

        [Header("Skill & Coin UI")]
        [field: SerializeField] public GameObject skillUI;
        [field: SerializeField] public Image skillImage;
        [field: SerializeField] public GameObject[] coinsImage = new GameObject[5];
        [field: SerializeField] public TextMeshProUGUI totalValue;
        private CanvasGroup _skillCanvasGroup;

        public enum CoinUIState { Ready, Front, Back, Broken }

        [Header("Coin UI Settings")]
        [field: SerializeField] private Sprite coinReadySprite;
        [field: SerializeField] private Sprite coinFrontSprite;
        [field: SerializeField] private Sprite coinBackSprite;
        [field: SerializeField] private Sprite coinBrokenSprite;

        [Header("Audio & Sounds")]
        [SerializeField] private AudioSource characterAudioSource;
        [SerializeField] private BattleSoundType deathSound = BattleSoundType.Willhelm;
        [SerializeField] private BattleSoundType coinBreakSound = BattleSoundType.CoinBreak;

        public BattleSoundType DeathSound => deathSound;
        public BattleSoundType CoinBreakSound => coinBreakSound;

        private bool _isDead;

        private readonly float _probabilityMax = 0.95f;
        private readonly float _probabilityMin = 0.05f;
        private readonly float _spMax = 45.0f;
        private readonly float _spMin = -45.0f;
        private float _a, _b;
        protected Animator Anim;
        public int StarLevel { get; private set; } = 1;
        public List<Skill> CurrentSkills { get; set; }
        private int _activeCoinLayoutCount;

        protected virtual void Awake()
        {
            Anim = GetComponent<Animator>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            if (characterAudioSource == null) characterAudioSource = GetComponent<AudioSource>();
            if (characterAudioSource != null) characterAudioSource.spatialBlend = 1f; // 3D 사운드 강제 적용

            _a = (_probabilityMax - _probabilityMin) / (_spMax - _spMin);
            _b = _probabilityMin - _a * _spMin;
            CurrentSkills = new List<Skill>();

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

        public void TakeDamage(int damage)
        {
            if (_isDead) return;

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

        public virtual void Initialize(int[] input, int index, string characterName, int strength,
            List<Skill> currentSkill)
        {
            var breakIndex = new int[input.Length];
            Array.Copy(input, breakIndex, input.Length);
            Index = index;
            Name = characterName;
            Strength = strength;
            foreach (var skill in currentSkill)
            {
                CurrentSkills.Add(skill);
            }
        }

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

        public void SetSpeedRange(int min, int max)
        {
            MinSpeed = min;
            MaxSpeed = max;
        }

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

        public List<Coin> GetSurvivingCoins(Skill targetSkill)
        {
            var surviving = new List<Coin>();
            if (targetSkill.coins == null) return surviving;
            foreach (var coin in targetSkill.coins)
                if (!coin.isBroken)
                    surviving.Add(coin);

            return surviving;
        }

        public int GetActiveCoinCount(Skill targetSkill)
        {
            if (targetSkill.coins == null) return 0;
            var count = 0;
            foreach (var coin in targetSkill.coins)
                if (!coin.isBroken)
                    count++;

            return count;
        }

        public int TossSingleCoin(Coin coin)
        {
            var prob = CalculateProbability(Sp);
            return Random.value < prob ? coin.frontValue : coin.backValue;
        }

        public int TossCoins(Skill targetSkill)
        {
            if (targetSkill.coins == null) return targetSkill.basicValue;
            var totalPower = targetSkill.basicValue;
            foreach (var coin in targetSkill.coins)
            {
                if (coin.isBroken) continue;
                var prob = CalculateProbability(Sp);
                if (Random.value < prob) totalPower += coin.frontValue;
                else totalPower += coin.backValue;
            }

            return totalPower;
        }

        public List<int> GetMultiHitPowers(Skill targetSkill)
        {
            var powers = new List<int>();
            if (targetSkill.coins == null || targetSkill.coins.Count == 0)
            {
                powers.Add(targetSkill.basicValue);
                return powers;
            }

            var currentPower = targetSkill.basicValue;
            foreach (var coin in targetSkill.coins)
            {
                if (coin.isBroken) continue;
                var prob = CalculateProbability(Sp);
                if (Random.value < prob) currentPower += coin.frontValue;
                else currentPower += coin.backValue;
                powers.Add(currentPower);
            }

            return powers;
        }

        public void BreakCoin(Skill targetSkill)
        {
            if (targetSkill.coins == null) return;
            for (var i = targetSkill.coins.Count - 1; i >= 0; i--)
                if (!targetSkill.coins[i].isBroken)
                {
                    var coin = targetSkill.coins[i];
                    coin.isBroken = true;
                    targetSkill.coins[i] = coin;
                    break;
                }
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

        protected virtual float CalculateProbability(float x)
        {
            return Mathf.Clamp(_a * x + _b, _probabilityMin, _probabilityMax);
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

        public string GetValidAnimTrigger(Skill skill, Coin coin)
        {
            // 코인에 지정된 애니메이션 트리거가 있다면 사용
            if (!string.IsNullOrEmpty(coin.animTrigger)) return coin.animTrigger;

            // 비어있다면 스킬의 이름을 그대로 사용 (예: "Skill1")
            if (!string.IsNullOrEmpty(skill.skillName)) return skill.skillName;

            throw new NotImplementedException();
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
    }
}