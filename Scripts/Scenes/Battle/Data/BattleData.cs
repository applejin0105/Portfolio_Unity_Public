using System;
using System.Collections.Generic;
using Core.Data.Enums;
using Scenes.Battle.Entity;
using Scenes.Battle.Logic;
using UnityEngine;

namespace Scenes.Battle.Data
{
    #region Enums

    public enum SkillType { Attack, Defend, Avoid, Counter }

    public enum BattleMatchType { Clash, OneSided }

    public enum CameraViewType { Start, Normal, ShowAll, FocusTarget, End }

    public enum BattleState { Idle, Execution, CheckResult }

    #endregion

    #region Structs

    [Serializable]
    public struct DynamicViewConfig
    {
        public float padding;
        public Vector3 positionOffset;
        public Vector3 rotationOffset;
    }

    [Serializable]
    public struct StaticViewConfig
    {
        public Vector3 position;
        public Vector3 rotation;
        public float fieldOfView;
    }

    [Serializable]
    public struct Coin : IEquatable<Coin>
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
        public float attackDashDistance;
        public GameObject vfxPrefab;

        [Header("Sound Settings")]
        public BattleSoundType coinTossSound;
        public BattleSoundType attackSound;
        public BattleSoundType hitSound;

        [Header("Custom Logic")]
        public CoinActionLogic customLogic;

        public bool Equals(Coin other)
        {
            return index == other.index && frontValue == other.frontValue && backValue == other.backValue &&
                   isBroken == other.isBroken && animTrigger == other.animTrigger && hitCount == other.hitCount &&
                   hitDelay.Equals(other.hitDelay) && attackDashDistance.Equals(other.attackDashDistance) &&
                   Equals(vfxPrefab, other.vfxPrefab) && Equals(customLogic, other.customLogic);
        }

        public override bool Equals(object obj)
        {
            return obj is Coin other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(index);
            hashCode.Add(frontValue);
            hashCode.Add(backValue);
            hashCode.Add(isBroken);
            hashCode.Add(animTrigger);
            hashCode.Add(hitCount);
            hashCode.Add(hitDelay);
            hashCode.Add(attackDashDistance);
            hashCode.Add(vfxPrefab);
            hashCode.Add(customLogic);
            return hashCode.ToHashCode();
        }
    }

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

    [Serializable]
    public struct BattleStruct : IEquatable<BattleStruct>
    {
        public ActionSlot AttackerSlot;
        public ActionSlot DefenderSlot;
        public BattleMatchType matchType;
        public CameraViewType viewType;

        public bool Equals(BattleStruct other)
        {
            return EqualityComparer<ActionSlot>.Default.Equals(AttackerSlot, other.AttackerSlot) &&
                   EqualityComparer<ActionSlot>.Default.Equals(DefenderSlot, other.DefenderSlot) &&
                   matchType == other.matchType &&
                   viewType == other.viewType;
        }

        public override bool Equals(object obj)
        {
            return obj is BattleStruct other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AttackerSlot, DefenderSlot, (int)matchType, (int)viewType);
        }
    }

    #endregion

    #region Classes

    public class ActionSlot
    {
        public Character Owner { get; set; }
        public Skill SelectedSkill { get; set; }
        public ActionSlot TargetSlot { get; set; }
    }

    #endregion
}