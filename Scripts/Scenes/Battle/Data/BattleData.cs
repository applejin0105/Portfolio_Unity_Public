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

    public struct BattleMatchup
    {
        public ActionSlot AttackerSlot;
        public ActionSlot DefenderSlot;
        public BattleMatchType MatchType;
        public CameraViewType ViewType;
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