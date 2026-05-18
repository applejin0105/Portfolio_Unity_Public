using Scenes.Battle.Board;
using Scenes.Battle.Combat;
using Scenes.Battle.Core;
using Scenes.Battle.Shop;
using Scenes.Battle.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Scenes.Battle.Entity
{
    public class Player : Character, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler, IBeginDragHandler,
        IDragHandler, IEndDragHandler
    {
        [Header("Dependencies")]
        private TargetingArrowController _arrowController;
        private FieldManager _fieldManager;
        private HandManager _handManager;
        private Camera _mainCamera;
        private Camera _uiCamera;
        private PlayerManager _playerManager;
        private RectTransform _sellZoneRect;
        private RectTransform _canvasRect;
        private SellZoneUI _sellZoneUI;
        private ToolTipUI _tooltipManager;

        private bool _isInCombat = false;

        #region Event Subscriptions

        private void OnEnable()
        {
            BattleManager.OnCombatStateChanged += HandleCombatState;
        }

        private void OnDisable()
        {
            BattleManager.OnCombatStateChanged -= HandleCombatState;
        }

        private void HandleCombatState(bool isCombat)
        {
            _isInCombat = isCombat;
        }

        #endregion

        public void SetDragDependencies(TargetingArrowController arrow, HandManager hand, PlayerManager player,
            SellZoneUI sellUI, ToolTipUI tooltip, FieldManager field)
        {
            _arrowController = arrow;
            _handManager = hand;
            _playerManager = player;
            _sellZoneUI = sellUI;
            _tooltipManager = tooltip;
            _fieldManager = field;
            _mainCamera = Camera.main;

            if (_sellZoneUI != null)
            {
                _sellZoneRect = _sellZoneUI.GetComponent<RectTransform>();
                var canvas = _sellZoneUI.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    _canvasRect = canvas.GetComponent<RectTransform>();
                    _uiCamera = canvas.worldCamera;
                }
                else
                {
                    _uiCamera = Camera.main;
                }
            }
        }

        public override void Attack()
        {
            if (Anim != null) Anim.SetTrigger("Attack");
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Hp > 0 && !eventData.dragging && _tooltipManager != null)
            {
                _tooltipManager.SetTooltipData(currentUnitData);
                _tooltipManager.UpdatePosition(eventData.position);
            }
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (Hp > 0 && !eventData.dragging && _tooltipManager != null)
                _tooltipManager.UpdatePosition(eventData.position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_tooltipManager != null) _tooltipManager.HideTooltip();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (Hp <= 0) return;

            if (_isInCombat)
            {
                Debug.Log($"[Player] 전투 중에는 다메다메다메요다메나노용");
                return;
            }

            if (_tooltipManager != null) _tooltipManager.HideTooltip();
            Cursor.visible = false;
            _arrowController.Activate();
            if (_sellZoneUI != null) _sellZoneUI.SetDraggedUnitCost(currentUnitData.cost / 2);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Hp <= 0 || _canvasRect == null || _isInCombat) return;

            // 1. 3D 월드의 플레이어 위치를 스크린 픽셀로 변환
            Vector2 screenStartPos = _mainCamera.WorldToScreenPoint(transform.position);

            // 2. 플레이어와 마우스의 위치를 모두 캔버스 공간(UI World Space)으로 변환
            RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvasRect, screenStartPos, _uiCamera,
                out var uiStartWorldPos);
            RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvasRect, eventData.position, _uiCamera,
                out var uiEndWorldPos);

            // Z축을 캔버스와 동일하게 고정하여 깊이 왜곡 완벽 차단
            uiStartWorldPos.z = _canvasRect.position.z;
            uiEndWorldPos.z = _canvasRect.position.z;

            _arrowController.DrawArrow(uiStartWorldPos, uiEndWorldPos);

            bool isOverSellZone = false;
            if (_sellZoneRect != null)
                isOverSellZone =
                    RectTransformUtility.RectangleContainsScreenPoint(_sellZoneRect, eventData.position, _uiCamera);
            if (_sellZoneUI != null) _sellZoneUI.ToggleSellPreview(isOverSellZone);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (Hp <= 0 || _isInCombat) return;

            _arrowController.Deactivate();
            Cursor.visible = true;

            if (_sellZoneUI != null) _sellZoneUI.ToggleSellPreview(false);

            if (_sellZoneRect != null &&
                RectTransformUtility.RectangleContainsScreenPoint(_sellZoneRect, eventData.position, _uiCamera))
            {
                _playerManager.ChangeCost(currentUnitData.cost / 2, out _);
                _fieldManager.RemoveFromField(currentUnitData);
                return;
            }

            foreach (var hit in eventData.hovered)
            {
                if (hit.CompareTag("HandZone"))
                {
                    _handManager.AddToHand(currentUnitData, out var isSuccess);
                    if (isSuccess) _fieldManager.RemoveFromField(currentUnitData);
                    return;
                }
            }
        }
    }
}