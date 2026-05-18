using Components.Common.Buttons.Core;
using Scenes.Battle.Board;
using Scenes.Battle.Combat;
using Scenes.Battle.Core;
using Scenes.Battle.Data;
using Scenes.Battle.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Scenes.Battle.UI
{
    public class HandSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler,
        IPointerExitHandler, IPointerMoveHandler
    {
        [Header("Managers")]
        [SerializeField] private TargetingArrowController arrowController;
        [SerializeField] private FieldManager fieldManager;
        [SerializeField] private PlayerManager playerManager;
        [SerializeField] private HandManager handManager;
        [SerializeField] private ToolTipUI tooltipManager;

        [Header("UI Elements")]
        [SerializeField] private GameObject[] starImages = new GameObject[3];
        [SerializeField] private TextMeshProUGUI unitName;
        [SerializeField] private GameObject sellZone;
        [SerializeField] private CompoundButton compoundButton;
        [SerializeField] private RectTransform lineStartPos;

        private CanvasGroup _canvasGroup;
        private UnitData _currentData;
        private Camera _mainCamera;
        private Camera _uiCamera;
        private RectTransform _canvasRect; // 캔버스 기준점 추가
        private RectTransform _sellZoneRect;
        private SellZoneUI _sellZoneUI;

        private bool _isInCombat = false;

        #region Event Subscriptions

        private void OnEnable()
        {
            BattleManager.OnCombatStateChanged += HandleCombatState;
            SceneManager.sceneLoaded += RefreshCameras; // 씬 리셋 카메라 복구
        }

        private void OnDisable()
        {
            BattleManager.OnCombatStateChanged -= HandleCombatState;
            SceneManager.sceneLoaded -= RefreshCameras;
        }

        private void HandleCombatState(bool isCombat)
        {
            _isInCombat = isCombat;
        }

        #endregion

        private void RefreshCameras(Scene scene, LoadSceneMode mode)
        {
            _mainCamera = Camera.main;
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                _canvasRect = canvas.GetComponent<RectTransform>();
                _uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            }
        }

        private void Awake()
        {
            // CanvasGroup 능동적 탐색 (인스펙터가 풀려도 찾아옴)
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();

            // Text 탐색 (이름이 "UnitNameText" 등일 경우)
            if (unitName == null) unitName = GetComponentInChildren<TextMeshProUGUI>();

            _mainCamera = Camera.main;

            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                _canvasRect = canvas.GetComponent<RectTransform>();
                _uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            }
            else
            {
                _uiCamera = Camera.main;
            }

            _canvasGroup = GetComponent<CanvasGroup>();
            ClearSlot();
            if (sellZone != null)
            {
                _sellZoneUI = sellZone.GetComponent<SellZoneUI>();
                _sellZoneRect = sellZone.GetComponent<RectTransform>();
            }
        }

        public bool SetSlot(UnitData data)
        {
            _currentData = data;
            int star = _currentData.star - 1;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.blocksRaycasts = true;
            }

            if (TryGetComponent<UnityEngine.UI.Image>(out var img))
            {
                img.color = Color.white;
            }

            unitName.text = _currentData.unitName;
            foreach (var image in starImages)
                if (image != null)
                    image.SetActive(false);
            if (star >= 0 && star < starImages.Length)
                if (starImages[star] != null)
                    starImages[star].SetActive(true);

            return true;
        }

        public void ClearSlot()
        {
            _currentData = null;
            unitName.text = "";

            foreach (var image in starImages)
                if (image != null)
                    image.SetActive(false);

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_currentData == null || eventData.dragging) return;
            tooltipManager.SetTooltipData(_currentData);
            tooltipManager.UpdatePosition(eventData.position);
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (_currentData == null || eventData.dragging) return;
            tooltipManager.UpdatePosition(eventData.position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            tooltipManager.HideTooltip();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_currentData == null) return;

            tooltipManager.HideTooltip();
            _canvasGroup.blocksRaycasts = false;
            Cursor.visible = false;
            arrowController.Activate();
            if (_sellZoneUI != null) _sellZoneUI.SetDraggedUnitCost(_currentData.cost / 2);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_currentData == null || _canvasRect == null) return;

            // 마우스 픽셀 좌표를 캔버스 공간(UI World Space)으로 변환
            RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvasRect, eventData.position, _uiCamera,
                out var uiEndWorldPos);

            // Z축을 캔버스와 동일하게 고정하여 깊이 왜곡 완벽 차단
            uiEndWorldPos.z = lineStartPos.position.z;

            // 이미 UI 공간에 있는 슬롯 위치와 방금 변환한 마우스 위치를 연결
            arrowController.DrawArrow(lineStartPos.position, uiEndWorldPos);

            bool isOverSellZone = false;
            if (_sellZoneRect != null)
                isOverSellZone =
                    RectTransformUtility.RectangleContainsScreenPoint(_sellZoneRect, eventData.position, _uiCamera);
            if (_sellZoneUI != null) _sellZoneUI.ToggleSellPreview(isOverSellZone);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_currentData == null) return;

            arrowController.Deactivate();
            _canvasGroup.blocksRaycasts = true;
            Cursor.visible = true;

            if (_sellZoneUI != null) _sellZoneUI.ToggleSellPreview(false);

            if (_sellZoneRect != null &&
                RectTransformUtility.RectangleContainsScreenPoint(_sellZoneRect, eventData.position, _uiCamera))
            {
                playerManager.ChangeCost(_currentData.cost / 2, out _);
                handManager.RemoveFromHand(_currentData);
                return;
            }

            Ray worldRay = _mainCamera.ScreenPointToRay(eventData.position);
            if (Physics.Raycast(worldRay, out var hit) && hit.collider.CompareTag("FieldZone"))
            {
                if (_isInCombat)
                {
                    Debug.Log($"[HandSlotUI] 전투 중에는 필드에 기물을 넣을 수 없는 데수웅");
                    return;
                }

                fieldManager.AddUnitToField(_currentData, hit.point);
                handManager.RemoveFromHand(_currentData);
            }
        }
    }
}