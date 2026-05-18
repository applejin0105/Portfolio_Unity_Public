using System;
using Scenes.Battle.Data;
using UnityEngine;

namespace Scenes.Battle.Shop
{
    public class PlayerManager : MonoBehaviour
    {
        [Header("Economy Data")]
        [SerializeField] private int currentCost;
        [SerializeField] private int startCost = 3;

        public Action<int> OnCostChanged;

        public int CurrentCost => currentCost;
        public int TotalCostSpent { get; private set; }
        public int TotalCostEarned { get; private set; }

        #region Economy Control

        private void Awake()
        {
            currentCost = startCost;
            OnCostChanged?.Invoke(currentCost);
        }

        public void ChangeCost(int cost, out bool costChanged)
        {
            var tmp = currentCost + cost;

            if (tmp < 0)
            {
                costChanged = false;
                return;
            }

            // 통계 누적
            if (cost > 0) TotalCostEarned += cost;
            else if (cost < 0) TotalCostSpent += Mathf.Abs(cost);

            currentCost = tmp;
            costChanged = true;

            OnCostChanged?.Invoke(currentCost);
        }

        public void SellUnit(UnitData targetUnit)
        {
            if (targetUnit.unitState != UnitState.Field && targetUnit.unitState != UnitState.Hand) return;

            var sellCost = targetUnit.cost / 2;
            ChangeCost(sellCost, out var isChanged);
        }

        #endregion

        public void ResetManager()
        {
            currentCost = startCost;
            TotalCostEarned = 0;
            TotalCostSpent = 0;

            OnCostChanged?.Invoke(currentCost);
        }
    }
}