using System.Collections;
using Scenes.Battle.Data;
using Components.Effects.Cameras.Types;
using UnityEngine;

namespace Scenes.Battle.Cameras
{
    public class CameraController : MonoBehaviour
    {
        public CameraTracker tracker;

        [Header("Effects")]
        public CameraMoveEffect moveEffect;

        [Header("Settings")]
        public float smoothTime = 0.1f;
        public float transitionDuration = 2.0f;

        [Header("Dynamic View Settings (Battle)")]
        public DynamicViewConfig normalWideConfig = new() { padding = 2.0f };
        public DynamicViewConfig focusTargetConfig = new() { padding = 1.0f };

        [Header("Static View Targets")]
        [Tooltip("씬 로드 직후의 아주 먼 시점 (Config[0])")]
        public CameraMoveConfig entryMoveConfig;

        [Tooltip("전투 대기 및 유닛 배치 시점의 표준 시점 (Config[1])")]
        public CameraMoveConfig idleViewConfig;

        [Header("Zoom Limits")]
        public float minZoomDistance = 8f;
        public float maxZoomDistance = 40f;

        private Camera _cam;
        private CameraViewType _currentViewType;
        private bool isDynamicTracking;
        private Vector3 velocity = Vector3.zero;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            if (moveEffect == null) moveEffect = GetComponent<CameraMoveEffect>();
        }

        private void LateUpdate()
        {
            if (!isDynamicTracking || tracker == null) return;

            switch (_currentViewType)
            {
                case CameraViewType.Normal:
                case CameraViewType.ShowAll:
                    ApplyDynamicConfig(normalWideConfig);
                    break;
                case CameraViewType.FocusTarget:
                    ApplyDynamicConfig(focusTargetConfig);
                    break;
            }
        }

        public void SetInitPosition()
        {
            isDynamicTracking = false;
            PlayStaticMoveEffect(idleViewConfig);
        }

        public void PlayCutscene(CameraViewType type)
        {
            isDynamicTracking = false;
            _currentViewType = type;

            switch (type)
            {
                case CameraViewType.Start:
                    PlayStaticMoveEffect(entryMoveConfig);
                    break;
                case CameraViewType.End:
                    PlayStaticMoveEffect(idleViewConfig);
                    break;
            }
        }

        public void SetDynamicTracking(CameraViewType type)
        {
            if (moveEffect != null) moveEffect.Stop(false);
            _currentViewType = type;
            isDynamicTracking = true;
        }

        private void ApplyDynamicConfig(DynamicViewConfig config)
        {
            var targetSize = tracker.CurrentTargetSize;
            var targetCenter = tracker.CurrentCenter;

            var halfFovRad = _cam.fieldOfView * 0.5f * Mathf.Deg2Rad;
            var tanHalfFov = Mathf.Tan(halfFovRad);

            var distanceForHeight = targetSize * config.padding / (2.0f * tanHalfFov);
            var distanceForWidth = distanceForHeight / _cam.aspect;

            var requiredDistance = Mathf.Max(distanceForHeight, distanceForWidth);
            requiredDistance = Mathf.Clamp(requiredDistance, minZoomDistance, maxZoomDistance);

            var targetRotation = Quaternion.Euler(config.rotationOffset);
            var targetPosition = targetCenter - targetRotation * Vector3.forward * requiredDistance +
                                 config.positionOffset;

            _cam.transform.position = Vector3.SmoothDamp(
                _cam.transform.position, targetPosition, ref velocity, smoothTime, Mathf.Infinity,
                Time.unscaledDeltaTime);

            _cam.transform.rotation = Quaternion.Slerp(
                _cam.transform.rotation, targetRotation, Time.unscaledDeltaTime * (1f / smoothTime));
        }

        private void PlayStaticMoveEffect(CameraMoveConfig targetConfig)
        {
            if (moveEffect == null) return;

            CameraMoveConfig currentToTargetConfig = targetConfig;

            // FOV End가 0으로 설정되어 있다면 현재 FOV를 유지하여 급격한 확대를 방지
            if (currentToTargetConfig.fov.end <= 0.1f)
            {
                currentToTargetConfig.fov.end = _cam.fieldOfView;
            }

            currentToTargetConfig.position.start = _cam.transform.localPosition;
            currentToTargetConfig.rotation.start = _cam.transform.localRotation.eulerAngles;
            currentToTargetConfig.fov.start = _cam.fieldOfView;

            moveEffect.SetProperty(currentToTargetConfig, transitionDuration);
            moveEffect.PlayOverrideEffect();
        }
    }
}
