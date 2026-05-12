using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scenes.Battle.Data
{
    public class UnitDatabase : MonoBehaviour
    {
        [Header("Master Data")]
        [SerializeField] private List<UnitData> masterUnitList;

        private readonly Dictionary<UnitRarity, List<UnitData>> _unitPoolByRarity = new();

        private void Awake()
        {
            InitializePool();
        }

        private void InitializePool()
        {
            _unitPoolByRarity.Clear();

            foreach (UnitRarity rarity in Enum.GetValues(typeof(UnitRarity)))
                _unitPoolByRarity[rarity] = new List<UnitData>();

            foreach (var unit in masterUnitList)
                if (unit != null)
                    _unitPoolByRarity[unit.rarity].Add(unit);
                else
                    Debug.LogWarning("[UnitDatabase] MasterUnitList에 할당되지 않은 Null 데이터가 존재합니다.");
        }

        public UnitData GetRandomUnitByRarity(UnitRarity targetRarity)
        {
            if (!_unitPoolByRarity.TryGetValue(targetRarity, out var targetPool) || targetPool.Count == 0) return null;

            var randomIndex = Random.Range(0, targetPool.Count);
            return targetPool[randomIndex];
        }
    }
}