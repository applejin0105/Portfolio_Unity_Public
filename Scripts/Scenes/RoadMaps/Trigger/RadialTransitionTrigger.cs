using Scenes.RoadMaps.Manager;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Scenes.RoadMaps.Trigger
{
    [RequireComponent(typeof(RectTransform))]
    public class RadialTransitionTrigger : MonoBehaviour, IPointerClickHandler
    {
        [Header("Manager Reference")]
        [SerializeField] private RadialTransitionManager manager;

        [Header("Button Index")]
        [SerializeField] private int elementIndex;

        private RectTransform _rectTransform;

        private float _lastClickTime = 0f;
        private const float ClickCooldown = 0.1f;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Time.time - _lastClickTime < ClickCooldown) return;
            _lastClickTime = Time.time;

            if (manager != null) manager.OnElementClicked(elementIndex, _rectTransform);
        }
    }
}