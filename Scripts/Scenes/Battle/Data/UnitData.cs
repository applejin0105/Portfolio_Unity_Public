using System;
using UnityEngine;

namespace Scenes.Battle.Data
{
    #region Enums

    public enum UnitState { Hand, Field }

    public enum UnitRarity { Common, Uncommon, Rare, Epic, Legendary }

    #endregion

    #region Structs

    [Serializable]
    public struct UnitStats
    {
        public int hp;
        public int sp;
        public int minSpeed;
        public int maxSpeed;
        public int weight;
        public int strength;
        public bool isImmovable;
    }

    [Serializable]
    public struct SkillBonus
    {
        public int addedBasicValue;
        public int addedFrontValue;
        public int addedBackValue;
    }

    [Serializable]
    public struct StarBonus
    {
        [Header("스탯 증가량")]
        public UnitStats addedStats;

        [Header("스킬/코인 위력 증가량")]
        public SkillBonus addedSkillBonus;

        [Header("액션 슬롯 증가량")]
        public int addedActionSlotCount;
    }

    #endregion

    #region ScriptableObjects

    [CreateAssetMenu(fileName = "NewUnitData", menuName = "Battle/UnitData")]
    public class UnitData : ScriptableObject
    {
        [Header("상점 및 기본 정보")]
        public int id;
        public string unitName;
        public Sprite profileSprite;
        public int cost;
        public int star;
        public int actionSlotCount;
        public UnitRarity rarity;
        public UnitState unitState;

        [Header("애니메이션 제어기")]
        public AnimatorOverrideController overrideController;

        [Header("기본 스탯 (1성 기준)")]
        public UnitStats baseStats;

        [Header("전투 사거리 (성급 무관 고정값)")]
        [Tooltip("타겟에 닿기 전 멈추는 거리. 캐릭터 크기에 맞춰 조절.")]
        public float combatRange = 1.5f;

        [Header("스킬 세팅 (해당 성급 도달 시 해금)")]
        [Tooltip("턴 시작 시 활성화된 스킬들의 Probability(가중치)를 합산하여 1개를 무작위 선택합니다.")]
        public Skill skillStar1;
        public Skill skillStar2;
        public Skill skillStar3;

        [Header("성급별 추가 능력치 (Index 0 = 1성, 1 = 2성, 2 = 3성)")]
        public StarBonus[] starBonuses = new StarBonus[3];
    }

    #endregion
}