using System;
using UnityEngine;

namespace Core.Extensions
{
    [Serializable]
    public struct CameraConfig
    {
        public bool isLocal;
        public Vector3 position;
        public Vector3 rotation;

        public bool isOrthographic;
        public float fieldOfView;
        public float orthographicSize;

        public CameraConfig(Vector3 position, Vector3 rotation, float fieldOfView, bool isLocal = false)
        {
            this.position = position;
            this.rotation = rotation;
            this.fieldOfView = fieldOfView;
            this.isLocal = isLocal;

            isOrthographic = false;
            orthographicSize = 5f;
        }

        // 두 Config 사이의 보간된 Config를 반환하는 유틸리티 메서드
        public static CameraConfig Lerp(CameraConfig a, CameraConfig b, float t)
        {
            var result = new CameraConfig();
            result.isLocal = b.isLocal;
            result.isOrthographic = b.isOrthographic;

            result.position = Vector3.Lerp(a.position, b.position, t);
            result.rotation = Quaternion.Lerp(Quaternion.Euler(a.rotation), Quaternion.Euler(b.rotation), t)
                .eulerAngles;
            result.fieldOfView = Mathf.Lerp(a.fieldOfView, b.fieldOfView, t);
            result.orthographicSize = Mathf.Lerp(a.orthographicSize, b.orthographicSize, t);

            return result;
        }
    }

    public static class CameraExtension
    {
        public static void ApplyConfig(this Camera camera, CameraConfig config)
        {
            if (camera == null) return;

            if (config.isLocal)
            {
                camera.transform.localPosition = config.position;
                camera.transform.localRotation = Quaternion.Euler(config.rotation);
            }
            else
            {
                camera.transform.position = config.position;
                camera.transform.rotation = Quaternion.Euler(config.rotation);
            }

            camera.orthographic = config.isOrthographic;
            if (config.isOrthographic)
                camera.orthographicSize = config.orthographicSize;
            else
                camera.fieldOfView = config.fieldOfView;
        }
    }
}