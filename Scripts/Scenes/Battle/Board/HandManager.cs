using System;
using System.Collections.Generic;
using Scenes.Battle.Data;
using Scenes.Battle.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scenes.Battle.Board
{
    public class HandManager : MonoBehaviour
    {
        [Header("Hand Data")]
        [SerializeField] private int maxHand = 10;
        [SerializeField] private List<UnitData> handUnits;
        public int HandCount => handUnits.Count;

        [Header("UI References")]
        [SerializeField] private Transform handSlotParent;
        [SerializeField] private HandSlotUI[] handSlots;
        public Action OnSlotChanged;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // 씬이 리셋될 때마다 무조건 실행되는 초기화 로직
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (handUnits == null) handUnits = new List<UnitData>();
            else handUnits.Clear();

            if (handSlotParent != null)
            {
                handSlots = handSlotParent.GetComponentsInChildren<HandSlotUI>(true);
                foreach (var slot in handSlots)
                {
                    slot.ClearSlot(); // 씬 진입 시 모든 슬롯 강제 숨김
                }
            }

            SyncHandSlots();
        }

        private void Awake()
        {
            // 유닛 리스트 강제 초기화
            if (handUnits == null) handUnits = new List<UnitData>();
            else handUnits.Clear();

            // 부모 오브젝트(handSlotParent)의 자식에 있는 진짜 슬롯들을 찾아서 강제 할당
            if (handSlotParent != null)
            {
                handSlots = handSlotParent.GetComponentsInChildren<HandSlotUI>(true);
                Debug.Log($"[HandManager] {handSlotParent.name} 아래에서 {handSlots.Length}개의 슬롯을 강제 갱신했습니다.");
            }
            else
            {
                Debug.LogError("[HandManager] handSlotParent가 비어있습니다! 인스펙터에서 Hand_Layout을 연결해주세요.");
            }
        }

        #region UI Synchronization

        private void SyncHandSlots()
        {
            if (handSlots == null || handSlots.Length == 0)
            {
                Debug.LogError("[HandManager] handSlots 배열 자체가 비어있거나 연결이 풀렸습니다!");
                return;
            }

            for (var i = 0; i < handSlots.Length; i++)
            {
                // [체크 1] 슬롯 객체가 유실(파괴)되었는지 확인
                if (handSlots[i] == null)
                {
                    Debug.LogError($"[HandManager] {i}번째 HandSlotUI 참조가 Missing 상태입니다!");
                    continue;
                }

                if (i < handUnits.Count)
                {
                    Debug.Log($"[HandManager] {i}번째 슬롯에 '{handUnits[i].unitName}' 데이터 전송 시도.");
                    handSlots[i].SetSlot(handUnits[i]);
                }
                else
                {
                    handSlots[i].ClearSlot();
                }
            }

            OnSlotChanged?.Invoke();
        }

        #endregion

        #region Core Logic

        public void AddToHand(UnitData unit, out bool isSuccess)
        {
            if (handUnits.Count >= maxHand)
            {
                isSuccess = false;
                return;
            }

            var clonedUnit = Instantiate(unit);
            clonedUnit.name = unit.unitName;
            clonedUnit.unitState = UnitState.Hand;

            handUnits.Add(clonedUnit);
            isSuccess = true;

            SyncHandSlots();
        }

        public void RemoveFromHand(UnitData card)
        {
            if (handUnits.Remove(card)) SyncHandSlots();
        }

        public List<UnitData> GetAllUnits()
        {
            return handUnits;
        }

        #endregion

        public void ResetManager()
        {
            handUnits.Clear();
            SyncHandSlots();
        }
    }
}