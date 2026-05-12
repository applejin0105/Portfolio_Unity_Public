using System;
using Components.Common.Buttons.Core;
using Scenes.Battle.Shop;
using TMPro;
using UnityEngine;

namespace Scenes.Battle.UI
{
    public class ShopUI : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField] private PlayerManager playerManager;
        [SerializeField] private ShopManager shopManager;

        [Header("Shop Cards")]
        [Tooltip("상점 패널에 배치된 CardUI 프리팹들을 순서대로 할당하세요.")]
        [SerializeField] private CardUI[] shopCards;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI currentCost;
        [SerializeField] private TextMeshProUGUI upgradeCost;
        [SerializeField] private TextMeshProUGUI rerollCost;

        [Header("Buttons")]
        [SerializeField] private CompoundButton rerollButton;
        [SerializeField] private CompoundButton upgradeButton;

        private void Start()
        {
            UpdateUI(playerManager.CurrentCost);
        }

        private void OnEnable()
        {
            if (playerManager != null) playerManager.OnCostChanged += UpdateUI;
            if (rerollButton != null) rerollButton.onClickEvent.AddListener(OnRerollButtonClick);
            if (upgradeButton != null) upgradeButton.onClickEvent.AddListener(OnUpgradeButtonClick);
        }

        private void OnDisable()
        {
            if (playerManager != null) playerManager.OnCostChanged -= UpdateUI;
            if (rerollButton != null) rerollButton.onClickEvent.RemoveListener(OnRerollButtonClick);
            if (upgradeButton != null) upgradeButton.onClickEvent.RemoveListener(OnUpgradeButtonClick);
        }

        private void UpdateUI(int changeCost)
        {
            currentCost.text = changeCost.ToString();
            rerollCost.text = shopManager.ShopRerollCost.ToString();
            upgradeCost.text = shopManager.CurrentStoreUpgradeCost.ToString();
        }

        private void OnUpgradeButtonClick()
        {
            shopManager.UpgradeShop();
        }

        private void OnRerollButtonClick()
        {
            shopManager.OnClickReroll();
        }

        public int GetMinCost()
        {
            int min = Int32.MaxValue;

            foreach (var card in shopCards)
            {
                if (card.ReturnCardCost() < min)
                {
                    min = card.ReturnCardCost();
                }
            }

            return min;
        }

        // ShopManager의 RerollShop() 등에서 호출되어 화면을 갱신하는 역할
        public void RefreshSlots()
        {
            if (shopCards == null || shopManager == null) return;

            foreach (var card in shopCards)
            {
                if (card == null) continue;

                var newUnit = shopManager.GenerateRandomUnitForSlot();

                if (newUnit != null)
                {
                    // 데이터 할당 및 회전 연출 실행
                    card.PlayRerollEffect(newUnit, shopManager);
                }
            }
        }
    }
}