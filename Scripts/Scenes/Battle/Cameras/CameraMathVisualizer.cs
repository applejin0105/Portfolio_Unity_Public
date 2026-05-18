using System;
using System.Collections.Generic;
using Scenes.Battle.Data;
using UnityEngine;

namespace Scenes.Battle.Cameras
{
    [Serializable]
    public struct DynamicViewConfig
    {
        public float padding;
        public Vector3 positionOffset;
        public Vector3 rotationOffset;
    }

    [Serializable]
    public struct StaticViewConfig
    {
        public Vector3 position;
        public Vector3 rotation;
        public float fieldOfView;
    }

    [ExecuteInEditMode]
    public class CameraMathVisualizer : MonoBehaviour
    {
        [Header("Targets")]
        public List<Transform> friendlies = new();
        public List<Transform> enemies = new();

        [Header("View State")]
        public CameraViewType currentViewType = CameraViewType.Normal;
        public float smoothTime = 0.1f;

        [Header("Dynamic View Settings (Normal, ShowAll, FocusTarget)")]
        public DynamicViewConfig normalConfig = new() { padding = 1.1f };
        public DynamicViewConfig showAllConfig = new() { padding = 1.5f };
        public DynamicViewConfig focusTargetConfig = new() { padding = 1.0f };

        [Header("Static View Settings (Start, End)")]
        public StaticViewConfig startConfig = new() { fieldOfView = 60f };
        public StaticViewConfig endConfig = new() { fieldOfView = 60f };

        [Header("Calculated Data (Read Only)")]
        public Vector3 friendlyCenter;
        public Vector3 enemyCenter;
        public Vector3 boundsCenter;
        public float boundsSize;
        public float currentDistance;
        public float requiredDistance;
        public float frustumHeight;
        public float frustumWidth;

        private Camera _cam;
        private Vector3 _velocity = Vector3.zero;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (_cam == null) return;
            if (friendlies.Count == 0 && enemies.Count == 0) return;

            UpdateCameraTracking();
            UpdateFrustumData();
        }

        /// <summary>
        ///     에디터 씬 뷰에 각 진영의 중심점, 최종 Bounds 영역 및 카메라 절두체를 렌더링합니다.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (_cam == null) return;
            if (friendlies.Count == 0 && enemies.Count == 0) return;

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(friendlyCenter, 0.3f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(enemyCenter, 0.3f);

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(friendlyCenter, enemyCenter);

            var bounds = new Bounds(friendlyCenter, Vector3.zero);
            if (enemies.Count > 0) bounds.Encapsulate(enemyCenter);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(bounds.center, bounds.size);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(bounds.center, boundsSize * 0.5f);

            Gizmos.color = Color.white;
            var camPos = transform.position;
            var camForward = transform.forward;
            var centerAtTarget = camPos + camForward * currentDistance;

            Gizmos.DrawWireCube(centerAtTarget, new Vector3(frustumWidth, frustumHeight, 0.01f));
            DrawFrustumLines(camPos, centerAtTarget, frustumWidth, frustumHeight);
        }

        /// <summary>
        ///     다중 타겟 리스트의 중심점을 계산하여 반환합니다.
        /// </summary>
        private Vector3 GetGroupCenter(List<Transform> targets)
        {
            if (targets == null || targets.Count == 0) return Vector3.zero;

            var bounds = new Bounds(targets[0].position, Vector3.zero);
            for (var i = 1; i < targets.Count; i++)
                if (targets[i] != null)
                    bounds.Encapsulate(targets[i].position);

            return bounds.center;
        }

        /// <summary>
        ///     설정된 CameraViewType에 따라 동적 추적 또는 정적 위치로 카메라를 이동시킵니다.
        /// </summary>
        private void UpdateCameraTracking()
        {
            switch (currentViewType)
            {
                case CameraViewType.Start:
                    ApplyStaticConfig(startConfig);
                    break;
                case CameraViewType.End:
                    ApplyStaticConfig(endConfig);
                    break;
                case CameraViewType.Normal:
                    ApplyDynamicConfig(normalConfig);
                    break;
                case CameraViewType.ShowAll:
                    ApplyDynamicConfig(showAllConfig);
                    break;
                case CameraViewType.FocusTarget:
                    ApplyDynamicConfig(focusTargetConfig);
                    break;
            }
        }

        /// <summary>
        ///     동적 추적 설정을 적용하여 타겟의 중심점 및 Bounds를 기반으로 카메라를 이동시킵니다.
        /// </summary>
        private void ApplyDynamicConfig(DynamicViewConfig config)
        {
            friendlyCenter = GetGroupCenter(friendlies);
            enemyCenter = GetGroupCenter(enemies);

            var bounds = new Bounds(friendlyCenter, Vector3.zero);
            if (enemies.Count > 0) bounds.Encapsulate(enemyCenter);

            boundsCenter = bounds.center;
            boundsSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

            if (boundsSize <= 0.1f) boundsSize = 5f;

            var halfFovRad = _cam.fieldOfView * 0.5f * Mathf.Deg2Rad;
            var tanHalfFov = Mathf.Tan(halfFovRad);

            var distanceForHeight = boundsSize * config.padding / (2.0f * tanHalfFov);
            var distanceForWidth = distanceForHeight / _cam.aspect;

            requiredDistance = Mathf.Max(distanceForHeight, distanceForWidth);

            var targetRotation = Quaternion.Euler(config.rotationOffset);
            var targetPosition = boundsCenter - targetRotation * Vector3.forward * requiredDistance +
                                 config.positionOffset;

            MoveCamera(targetPosition, targetRotation);
        }

        /// <summary>
        ///     고정된 정적 위치와 회전값, 시야각(FOV)을 카메라에 적용합니다.
        /// </summary>
        private void ApplyStaticConfig(StaticViewConfig config)
        {
            _cam.fieldOfView = config.fieldOfView;
            var targetRotation = Quaternion.Euler(config.rotation);
            MoveCamera(config.position, targetRotation);
        }

        /// <summary>
        ///     목표 좌표와 회전값으로 카메라 Transform을 갱신합니다.
        /// </summary>
        private void MoveCamera(Vector3 targetPosition, Quaternion targetRotation)
        {
            if (Application.isPlaying)
            {
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, smoothTime);
                transform.rotation =
                    Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * (1f / smoothTime));
            }
            else
            {
                transform.position = targetPosition;
                transform.rotation = targetRotation;
            }
        }

        /// <summary>
        ///     기즈모 렌더링을 위한 카메라 절두체(Frustum) 데이터를 갱신합니다.
        /// </summary>
        private void UpdateFrustumData()
        {
            var halfFovRad = _cam.fieldOfView * 0.5f * Mathf.Deg2Rad;
            var tanHalfFov = Mathf.Tan(halfFovRad);

            // 동적 뷰 타입일 때만 오프셋을 포함한 거리 계산 적용
            var referencePosition = boundsCenter;
            if (currentViewType == CameraViewType.Normal) referencePosition += normalConfig.positionOffset;
            else if (currentViewType == CameraViewType.ShowAll) referencePosition += showAllConfig.positionOffset;
            else if (currentViewType == CameraViewType.FocusTarget)
                referencePosition += focusTargetConfig.positionOffset;

            currentDistance = Vector3.Distance(transform.position, referencePosition);
            frustumHeight = 2.0f * currentDistance * tanHalfFov;
            frustumWidth = frustumHeight * _cam.aspect;
        }

        /// <summary>
        ///     카메라 위치와 목표 지점을 연결하는 절두체 외곽선을 그립니다.
        /// </summary>
        private void DrawFrustumLines(Vector3 camPos, Vector3 center, float width, float height)
        {
            var up = transform.up * height * 0.5f;
            var right = transform.right * width * 0.5f;

            var topRight = center + up + right;
            var topLeft = center + up - right;
            var bottomRight = center - up + right;
            var bottomLeft = center - up - right;

            Gizmos.DrawLine(camPos, topRight);
            Gizmos.DrawLine(camPos, topLeft);
            Gizmos.DrawLine(camPos, bottomRight);
            Gizmos.DrawLine(camPos, bottomLeft);
        }
    }
}