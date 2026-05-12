using Components.Common.Buttons.Core;
using Scenes.Battle.Board;
using Scenes.Battle.Core;
using Scenes.Battle.Data;
using Scenes.Battle.Shop;
using TMPro;
using UnityEngine;

namespace Scenes.Battle.UI
{
    public class TopPanelUI : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private BattleDirector battleDirector;
        [SerializeField] private PlayerManager playerManager;
        [SerializeField] private HandManager handManager;
        [SerializeField] private ShopManager shopManager;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI stageCount;
        [SerializeField] private CompoundButton battleStartButton;

        private void Awake()
        {
            if (battleManager == null) Debug.LogError("[TopPanelUI] battleManager is null");
            if (battleDirector == null) Debug.LogError("[TopPanelUI] battleDirector is null");
        }

        private void Start()
        {
            UpdateUI();
        }

        private void OnEnable()
        {
            if (battleStartButton != null) battleStartButton.onClickEvent.AddListener(OnClickBattleStartButton);
            if (battleManager != null) battleManager.OnTurnEnded += UpdateUI;
        }

        private void OnDisable()
        {
            if (battleStartButton != null) battleStartButton.onClickEvent.RemoveListener(OnClickBattleStartButton);
            if (battleManager != null) battleManager.OnTurnEnded -= UpdateUI;
        }

        private void UpdateUI()
        {
            if (battleDirector != null && battleDirector.currentStageData != null)
                stageCount.text = $"Stage {battleDirector.currentStageData.stageIndex}";
        }

        private void OnClickBattleStartButton()
        {
            int currentCost = playerManager.CurrentCost;
            int minUnitCost = shopManager.GetMinCost();
            int handUnitCount = handManager.HandCount;

            if (!battleDirector.CanStartCombat(currentCost, minUnitCost, handUnitCount))
            {
                return;
            }

            battleDirector.StartCombatPhase();
        }
    }
}