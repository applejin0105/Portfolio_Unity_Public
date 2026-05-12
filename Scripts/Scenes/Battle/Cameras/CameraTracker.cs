using System.Collections.Generic;
using UnityEngine;

namespace Scenes.Battle.Cameras
{
    public class CameraTracker : MonoBehaviour
    {
        private List<Transform> _trackingTargets = new();
        private Dictionary<Transform, Vector3> _focusPoints = new();

        public Vector3 CurrentCenter { get; private set; }
        public float CurrentTargetSize { get; private set; }

        private void LateUpdate()
        {
            if (_trackingTargets.Count == 0) return;

            Bounds bounds = new Bounds();
            bool isInitialized = false;

            // 1. 모든 타겟을 포함하는 영역(Bounds) 계산 -> 카메라 줌아웃 크기(Size) 결정용
            foreach (var t in _trackingTargets)
            {
                if (t == null) continue;
                if (!isInitialized)
                {
                    bounds = new Bounds(t.position, Vector3.zero);
                    isInitialized = true;
                }
                else
                {
                    bounds.Encapsulate(t.position);
                }
            }

            if (!isInitialized) return;

            var maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            CurrentTargetSize = maxSize > 0.1f ? maxSize : 5f;

            // 2. 중심점(Center) 계산 -> 포커스 포인트가 있으면 그 평균, 없으면 타겟 위치들의 평균
            if (_focusPoints.Count > 0)
            {
                Vector3 sum = Vector3.zero;
                int validCount = 0;

                foreach (var kvp in _focusPoints)
                {
                    // 현재 트래킹 중인 타겟의 포커스 포인트만 유효값으로 계산
                    if (kvp.Key != null && _trackingTargets.Contains(kvp.Key))
                    {
                        sum += kvp.Value;
                        validCount++;
                    }
                }

                CurrentCenter = validCount > 0 ? sum / validCount : bounds.center;
            }
            else
            {
                CurrentCenter = bounds.center;
            }
        }

        public void SetTargets(List<Transform> targets)
        {
            _trackingTargets.Clear();
            if (targets != null) _trackingTargets.AddRange(targets);
        }

        // 외부 참조(BattleDirector 등)와의 하위 호환성 유지용 오버로딩
        public void SetTargets(List<Transform> newFriendlies, List<Transform> newEnemies)
        {
            _trackingTargets.Clear();
            if (newFriendlies != null) _trackingTargets.AddRange(newFriendlies);
            if (newEnemies != null) _trackingTargets.AddRange(newEnemies);
        }

        public void SetTargets(Transform player, Transform enemy)
        {
            _trackingTargets.Clear();
            if (player != null) _trackingTargets.Add(player);
            if (enemy != null) _trackingTargets.Add(enemy);
        }

        // 특정 캐릭터(주로 공격자)가 바라봐야 할 예측 교차점을 등록
        public void SetFocusPoint(Transform key, Vector3 point)
        {
            if (key != null) _focusPoints[key] = point;
        }

        public void RemoveFocusPoint(Transform key)
        {
            if (key != null) _focusPoints.Remove(key);
        }

        public void ClearFocusPoints()
        {
            _focusPoints.Clear();
        }
    }
}