using Core.Extensions;
using UnityEngine;

namespace Scenes.Battle.Combat
{
    public class TargetingArrowController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Transform arrowHead;

        [Header("Visual Settings")]
        [SerializeField] private float lineWidth = 0.05f;
        [SerializeField] private float headScale = 1.0f;
        [SerializeField] private float tilePixelLength = 100f;

        [SerializeField] private DrawColliderGL drawColliderGL;

        private Material _lineMaterial;
        private Camera _uiCamera;

        private void Awake()
        {
            var canvas = GetComponentInParent<Canvas>();
            _uiCamera = canvas != null ? canvas.worldCamera : Camera.main;

            if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();

            if (lineRenderer != null)
            {
                lineRenderer.textureMode = LineTextureMode.Stretch;
                lineRenderer.positionCount = 2;
                lineRenderer.useWorldSpace = true; // 반드시 월드 스페이스 사용
                _lineMaterial = lineRenderer.material;
            }

            Deactivate();
        }

        public void Activate()
        {
            gameObject.SetActive(true);
            if (drawColliderGL != null) drawColliderGL.showInPlayMode = true;
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
            if (drawColliderGL != null) drawColliderGL.showInPlayMode = false;
        }

        public void DrawArrow(Vector3 startUIWorldPos, Vector3 endUIWorldPos)
        {
            if (lineRenderer == null) return;

            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.SetPosition(0, startUIWorldPos);
            lineRenderer.SetPosition(1, endUIWorldPos);

            if (arrowHead != null)
            {
                arrowHead.position = endUIWorldPos;
                arrowHead.localScale = Vector3.one * headScale;

                Vector3 dir = (endUIWorldPos - startUIWorldPos).normalized;
                if (dir != Vector3.zero)
                {
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    arrowHead.rotation = (_uiCamera != null ? _uiCamera.transform.rotation : Quaternion.identity) *
                                         Quaternion.Euler(0, 0, angle - 90f);
                }
            }

            // 타일링을 위해 캔버스 좌표를 픽셀 거리로 다시 변환하여 계산
            if (_lineMaterial != null && tilePixelLength > 0f)
            {
                Vector2 screenStart =
                    _uiCamera != null ? _uiCamera.WorldToScreenPoint(startUIWorldPos) : startUIWorldPos;
                Vector2 screenEnd = _uiCamera != null ? _uiCamera.WorldToScreenPoint(endUIWorldPos) : endUIWorldPos;
                float screenDistance = Vector2.Distance(screenStart, screenEnd);
                _lineMaterial.mainTextureScale = new Vector2(screenDistance / tilePixelLength, 1f);
            }
        }
    }
}