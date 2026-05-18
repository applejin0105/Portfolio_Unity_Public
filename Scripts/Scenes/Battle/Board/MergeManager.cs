using System.Collections.Generic;
using System.Linq;
using Scenes.Battle.Core;
using Scenes.Battle.Data;
using UnityEngine;

namespace Scenes.Battle.Board
{
    public class MergeManager : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField] private FieldManager fieldManager;
        [SerializeField] private HandManager handManager;
        [SerializeField] private BattleManager battleManager;

        private Queue<UnitData> _mergeQueue = new();

        private void Awake()
        {
            if (_mergeQueue == null) _mergeQueue = new Queue<UnitData>();
        }

        #region Merge Check & Queue Processing

        public void CheckMerge(UnitData targetUnit)
        {
            if (targetUnit.star >= 3) return;

            var handMatches = GetMatchingUnits(handManager.GetAllUnits(), targetUnit);
            var fieldMatches = GetMatchingUnits(fieldManager.GetAllUnits(), targetUnit);

            var totalCount = handMatches.Count + fieldMatches.Count;
            if (totalCount < 3) return;

            var isCombatActive = battleManager.CurrentState != BattleState.Idle;

            if (isCombatActive)
            {
                if (handMatches.Count >= 3)
                {
                    Debug.Log("[Merge] 전투 중 대기열 무시: 핸드 내부 합성 진행.");
                    ExecuteMerge(targetUnit, handMatches.Take(3).ToList(), new List<UnitData>());
                }
                else
                {
                    if (!_mergeQueue.Contains(targetUnit))
                    {
                        Debug.Log("[Merge] 전투 중 필드 기물 포함됨: 대기열 추가.");
                        _mergeQueue.Enqueue(targetUnit);
                    }
                }
            }
            else
            {
                ExecuteMerge(targetUnit, handMatches, fieldMatches);
            }
        }

        public void ProcessQueue()
        {
            if (_mergeQueue.Count == 0) return;

            Debug.Log("[Merge] 전투 종료. 대기열에 보관된 합성 프로세스를 실행합니다.");

            var initialCount = _mergeQueue.Count;
            for (var i = 0; i < initialCount; i++)
            {
                var queuedUnit = _mergeQueue.Dequeue();

                if (queuedUnit == null)
                {
                    Debug.Log("[Merge] 대기열의 유닛이 이미 판매되어 파괴되었습니다. 합성을 취소합니다.");
                    continue;
                }

                CheckMerge(queuedUnit);
            }
        }

        private List<UnitData> GetMatchingUnits(List<UnitData> sourceList, UnitData target)
        {
            if (sourceList == null) return new List<UnitData>();
            return sourceList.Where(u => u.unitName == target.unitName && u.star == target.star).ToList();
        }

        #endregion

        #region Merge Execution

        private void ExecuteMerge(UnitData targetUnit, List<UnitData> handMatches, List<UnitData> fieldMatches)
        {
            var required = 3;
            var isMergedOnField = fieldMatches.Count > 0;
            var fieldSpawnPosition = Vector3.zero;

            var handRemoveCount = Mathf.Min(handMatches.Count, required);
            for (var i = 0; i < handRemoveCount; i++)
            {
                handManager.RemoveFromHand(handMatches[i]);
                Destroy(handMatches[i]);
            }

            required -= handRemoveCount;

            if (required > 0)
            {
                var fieldRemoveCount = Mathf.Min(fieldMatches.Count, required);
                for (var i = 0; i < fieldRemoveCount; i++)
                {
                    if (i == 0) fieldSpawnPosition = fieldManager.GetUnitPosition(fieldMatches[i]);
                    fieldManager.RemoveFromField(fieldMatches[i]);
                    Destroy(fieldMatches[i]);
                }
            }

            CreateUpgradedUnit(targetUnit, isMergedOnField, fieldSpawnPosition);
        }

        private void CreateUpgradedUnit(UnitData baseUnit, bool isMergedOnField, Vector3 spawnPosition)
        {
            var upgradedUnit = Instantiate(baseUnit);
            upgradedUnit.name = baseUnit.unitName;
            upgradedUnit.star = baseUnit.star + 1;

            if (isMergedOnField)
                fieldManager.AddUnitToField(upgradedUnit, spawnPosition);
            else
                handManager.AddToHand(upgradedUnit, out _);

            CheckMerge(upgradedUnit);
        }

        #endregion
    }
}