using System.Collections.Generic;
using Scenes.Battle.Combat;
using Scenes.Battle.Data;
using Scenes.Battle.Entity;
using Scenes.Battle.Shop;
using Scenes.Battle.UI;
using UnityEngine;

namespace Scenes.Battle.Board
{
    public class FieldManager : MonoBehaviour
    {
        [Header("Prefabs & Hierarchy")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform playerParent;

        [Header("Dependencies (Player Inject)")]
        [SerializeField] private TargetingArrowController arrowController;
        [SerializeField] private HandManager handManager;
        [SerializeField] private PlayerManager playerManager;
        [SerializeField] private SellZoneUI sellZoneUI;
        [SerializeField] private ToolTipUI tooltipManager;

        [Header("Field Settings")]
        [SerializeField] private Transform[] playerSlots;

        private readonly List<Player> _activePlayers = new();
        private readonly Dictionary<Transform, Player> _slotOccupancy = new();
        private readonly List<UnitData> _cachedUnitList = new(); // GC 방지용 캐싱 리스트

        private void Awake()
        {
            foreach (var slot in playerSlots) _slotOccupancy[slot] = null;
        }

        #region Data Provider

        public List<UnitData> GetAllUnits()
        {
            _cachedUnitList.Clear();
            foreach (var player in _activePlayers)
                if (player != null && player.currentUnitData != null)
                    _cachedUnitList.Add(player.currentUnitData);

            return _cachedUnitList;
        }

        public List<Player> GetActivePlayers()
        {
            return _activePlayers;
        }

        public Vector3 GetUnitPosition(UnitData unit)
        {
            var target = _activePlayers.Find(p => p.currentUnitData == unit);
            return target != null ? target.transform.position : Vector3.zero;
        }

        #endregion

        #region Placement Logic

        public void AddUnitToField(UnitData unit, Vector3 dropPosition)
        {
            if (unit == null || playerPrefab == null) return;

            var targetSlot = GetClosestEmptySlot(dropPosition);

            if (targetSlot == null)
            {
                Debug.LogWarning("[FieldManager] 배치할 수 있는 빈 슬롯이 없습니다. 핸드로 카드를 반환합니다.");
                handManager.AddToHand(unit, out var isSuccess);
                if (!isSuccess) Debug.LogError("[FieldManager] 핸드 공간도 부족하여 기물이 소실되었습니다.");
                return;
            }

            var newPlayerObj = Instantiate(playerPrefab, targetSlot.position, Quaternion.identity, playerParent);
            var playerComponent = newPlayerObj.GetComponent<Player>();

            if (playerComponent != null)
            {
                playerComponent.HomePosition = targetSlot.position;
                unit.unitState = UnitState.Field;

                playerComponent.SetupUnit(unit, unit.star);
                playerComponent.SetDragDependencies(arrowController, handManager, playerManager, sellZoneUI,
                    tooltipManager, this);
                playerComponent.SetInfoUI();
                playerComponent.ShowInfoUI();

                _activePlayers.Add(playerComponent);
                _slotOccupancy[targetSlot] = playerComponent;
            }
        }

        public void RemoveFromField(UnitData unit)
        {
            if (unit == null) return;

            var targetPlayer = _activePlayers.Find(p => p.currentUnitData == unit);

            if (targetPlayer != null)
            {
                Transform occupiedSlot = null;
                foreach (var kvp in _slotOccupancy)
                    if (kvp.Value == targetPlayer)
                    {
                        occupiedSlot = kvp.Key;
                        break;
                    }

                if (occupiedSlot != null) _slotOccupancy[occupiedSlot] = null;

                _activePlayers.Remove(targetPlayer);
                Destroy(targetPlayer.gameObject);
            }
        }

        private Transform GetClosestEmptySlot(Vector3 position)
        {
            Transform closestSlot = null;
            var minDistance = float.MaxValue;
            var dropPos2D = new Vector2(position.x, position.y);

            foreach (var slot in playerSlots)
            {
                if (_slotOccupancy[slot] != null) continue;

                var slotPos2D = new Vector2(slot.position.x, slot.position.y);
                var dist = Vector2.Distance(dropPos2D, slotPos2D);

                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestSlot = slot;
                }
            }

            return closestSlot;
        }

        #endregion

        public void ResetField()
        {
            // 필드에 소환된 모든 플레이어 오브젝트 물리적 파괴
            foreach (var player in _activePlayers)
            {
                if (player != null && player.gameObject != null)
                {
                    Destroy(player.gameObject);
                }
            }

            // 관리 중인 리스트 초기화
            _activePlayers.Clear();
            _cachedUnitList.Clear();

            // 슬롯 점유 상태(딕셔너리) 초기화
            // Dictionary는 foreach 중 수정이 불가능하므로 Key 리스트를 따로 뽑아서 순회.
            var keys = new List<Transform>(_slotOccupancy.Keys);
            foreach (var key in keys)
            {
                _slotOccupancy[key] = null;
            }
        }
    }
}