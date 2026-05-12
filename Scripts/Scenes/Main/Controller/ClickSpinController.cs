using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Scenes.Main.Controller
{
    public class ClickSpinController : MonoBehaviour, IPointerClickHandler
    {
        [Header("개별적으로 회전할 대상 리스트")]
        [SerializeField] private List<RectTransform> targetRects;

        [Header("Spin Properties")]
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private int spinCount = 10;

        private readonly HashSet<RectTransform> _spinningRects = new();

        public void OnPointerClick(PointerEventData eventData)
        {
            // 컨트롤러 자체가 파괴되었거나 비활성 상태일 경우 실행 방지
            if (this == null || !gameObject.activeInHierarchy) return;

            // 리스트 내 파괴된 오브젝트 사전 정리
            targetRects.RemoveAll(rect => rect == null);

            foreach (var rect in targetRects)
                if (RectTransformUtility.RectangleContainsScreenPoint(rect, eventData.position,
                        eventData.pressEventCamera))
                    if (!_spinningRects.Contains(rect))
                        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                                rect,
                                eventData.position,
                                eventData.pressEventCamera,
                                out var localCursor))
                        {
                            var spinDirection = localCursor.x < 0 ? -1f : 1f;
                            StartCoroutine(SpinTargetRect(rect, spinDirection));
                        }
        }

        private IEnumerator SpinTargetRect(RectTransform target, float direction)
        {
            _spinningRects.Add(target);

            var elapsedTime = 0f;
            var startEuler = target.localEulerAngles;
            var startY = startEuler.y;
            var targetY = startY + 360f * spinCount * direction;

            while (elapsedTime < duration)
            {
                // 코루틴 실행 중 대상이 파괴되었는지 매 프레임 확인 (MissingReferenceException 해결)
                if (target == null) yield break;

                elapsedTime += Time.deltaTime;
                var t = Mathf.Clamp01(elapsedTime / duration);

                // 감속 효과 유지 (Cubic Ease Out)
                var easeOutT = 1f - Mathf.Pow(1f - t, 3f);

                // X, Z축은 0으로 감속 복귀, Y축은 목표 지점까지 감속 회전
                var currentX = Mathf.LerpAngle(startEuler.x, 0f, easeOutT);
                var currentZ = Mathf.LerpAngle(startEuler.z, 0f, easeOutT);
                var currentY = Mathf.Lerp(startY, targetY, easeOutT);

                target.localRotation = Quaternion.Euler(currentX, currentY, currentZ);

                yield return null;
            }

            // 최종 종료 시 정확한 원래 각도(0, 0, 0)로 고정
            if (target != null)
            {
                target.localRotation = Quaternion.identity;
                _spinningRects.Remove(target);
            }
        }
    }
}