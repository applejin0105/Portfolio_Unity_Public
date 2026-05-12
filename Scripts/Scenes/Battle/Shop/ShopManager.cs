using System.Collections.Generic;
using Scenes.Battle.Board;
using Scenes.Battle.Data;
using Scenes.Battle.UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scenes.Battle.Shop
{
    public class ShopManager : MonoBehaviour
    {
        [Header("Managers & Data")]
        [SerializeField] private PlayerManager playerManager;
        [SerializeField] private MergeManager mergeManager;
        [SerializeField] private HandManager handManager;
        [SerializeField] private UnitDatabase unitDatabase;

        [Header("Shop Status")]
        [SerializeField] private int currentStoreStep;
        [SerializeField] private int currentShopUpgradeCost;
        [SerializeField] private int shopRerollCost;

        public int ShopRerollCost => shopRerollCost;

        [Header("Shop Progression Settings")]
        [SerializeField] private List<int> shopUpgradeCost;
        [SerializeField] private List<ShopLevelProbability> shopProbabilities;

        [Header("UI References")]
        [SerializeField] private ShopUI shopUI;

        public int CurrentStoreUpgradeCost => currentShopUpgradeCost;

        private void Awake()
        {
            if (shopUpgradeCost != null)
                currentShopUpgradeCost = shopUpgradeCost[0];
        }

        public void Start()
        {
            RerollShop();
        }

        #region Shop Reroll & Generation

        public void OnClickReroll()
        {
            RerollShop(false);
        }

        public void RerollShop(bool isFree = true)
        {
            if (!isFree)
            {
                if (playerManager.CurrentCost < shopRerollCost) return;

                playerManager.ChangeCost(-shopRerollCost, out var isChanged);
                if (!isChanged) return;
            }

            if (shopUI != null) shopUI.RefreshSlots();
        }

        public UnitData GenerateRandomUnitForSlot()
        {
            var rolledRarity = RollRarity(currentStoreStep);
            return unitDatabase.GetRandomUnitByRarity(rolledRarity);
        }

        private UnitRarity RollRarity(int level)
        {
            var currentProb = shopProbabilities.Find(p => p.shopLevel == level);

            if (currentProb.rarityChances == null || currentProb.rarityChances.Count == 0)
                return UnitRarity.Common;

            var roll = Random.Range(0f, 100f);
            var cumulative = 0f;

            foreach (var rc in currentProb.rarityChances)
            {
                cumulative += rc.chance;
                if (roll <= cumulative) return rc.rarity;
            }

            return UnitRarity.Common;
        }

        public int GetMinCost()
        {
            return shopUI.GetMinCost();
        }

        #endregion

        #region Shop Interactions (Buy & Upgrade)

        public void TryBuyUnit(CardUI clickedCard, out bool isSuc)
        {
            Debug.Log("[ShopManager] TryBuyUnit 진입 완료");

            var data = clickedCard.AssignedData;

            if (playerManager.CurrentCost < data.cost)
            {
                isSuc = false;
                return;
            }

            handManager.AddToHand(data, out var isSuccess);

            if (!isSuccess)
            {
                Debug.LogWarning("[ShopManager] 핸드가 꽉 차있습니다.");
                isSuc = false;
                return;
            }

            playerManager.ChangeCost(-data.cost, out var costChanged);
            clickedCard.PlayPurchaseEffect();
            mergeManager.CheckMerge(data);
            isSuc = true;
        }

        public void UpgradeShop()
        {
            if (currentStoreStep >= shopUpgradeCost.Count) return;

            if (playerManager.CurrentCost < currentShopUpgradeCost) return;

            playerManager.ChangeCost(-currentShopUpgradeCost, out var isChanged);

            if (isChanged)
            {
                currentStoreStep++;

                // 설정된 최대 레벨을 초과하지 않는 선에서 다음 레벨업 비용 갱신
                if (shopUpgradeCost != null && currentStoreStep < shopUpgradeCost.Count)
                    currentShopUpgradeCost = shopUpgradeCost[currentStoreStep];

                Debug.Log($"[ShopManager] 상점 레벨업 완료. 현재 레벨: {currentStoreStep}");
            }
        }

        #endregion

        public void ResetShop()
        {
            // 상점 레벨 초기화
            currentStoreStep = 0;

            // 상점 업그레이드 비용 초기화
            if (shopUpgradeCost != null && shopUpgradeCost.Count > 0)
            {
                currentShopUpgradeCost = shopUpgradeCost[0];
            }

            // 상점 기물 목록을 무료 리롤을 통해 강제 새로고침
            RerollShop(true);

            Debug.Log("[ShopManager] 상점 하드 리셋 완료.");
        }
    }
}