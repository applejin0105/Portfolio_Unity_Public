using Components.Effects.UI;
using Components.Effects.UI.Types;
using Scenes.Battle.Entity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Scenes.Battle.UI
{
    public class SellZoneUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI pricePreviewText;
        [SerializeField] private GameObject sellCostObj;

        [Header("Effects")]
        [SerializeField] private UIMoveEffect moveEffectUp;
        [SerializeField] private UIMoveEffect moveEffectDown;

        [SerializeField] private UIHoverSwingEffect textHoverEffect;

        private MoveConfig _moveConfig;

        private void Awake()
        {
            pricePreviewText.gameObject.SetActive(false);
        }

        public void SetDraggedUnitCost(int sellPrice)
        {
            pricePreviewText.text = $"+{sellPrice}";
        }

        public void ToggleSellPreview(bool isVisible)
        {
            pricePreviewText.gameObject.SetActive(isVisible);
        }

        private void SetMoveConfig(bool isEnter)
        {
            // Enter일 때 43.5 -> 12.0, Exit일 때 12.0 -> 43.5 로드
            float startY = isEnter ? 12.0f : 43.5f;
            float endY = isEnter ? 43.5f : 12.0f;

            _moveConfig = moveEffectUp.DefaultConfig;
            _moveConfig.position.start = new Vector3(0, startY, 0);
            _moveConfig.position.end = new Vector3(0, endY, 0);
            moveEffectUp.SetProperty(_moveConfig);

            _moveConfig = moveEffectDown.DefaultConfig;
            _moveConfig.position.start = new Vector3(0, -startY, 0);
            _moveConfig.position.end = new Vector3(0, -endY, 0);
            moveEffectDown.SetProperty(_moveConfig);
        }

        #region Pointer Events

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!eventData.dragging || eventData.pointerDrag == null) return;

            var isFromHand = eventData.pointerDrag.GetComponent<HandSlotUI>() != null;
            var isFromField = eventData.pointerDrag.GetComponent<Player>() != null;

            if (isFromHand || isFromField)
            {
                SetMoveConfig(true);
                moveEffectUp.PlayOverrideEffect();
                moveEffectDown.PlayOverrideEffect();
                if (sellCostObj.activeInHierarchy) textHoverEffect.PlayOverrideEffect();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!eventData.dragging || eventData.pointerDrag == null) return;

            var isFromHand = eventData.pointerDrag.GetComponent<HandSlotUI>() != null;
            var isFromField = eventData.pointerDrag.GetComponent<Player>() != null;

            if (isFromHand || isFromField)
            {
                SetMoveConfig(false);
                moveEffectUp.PlayOverrideEffect();
                moveEffectDown.PlayOverrideEffect();
                textHoverEffect.Stop();
            }
        }

        #endregion
    }
}