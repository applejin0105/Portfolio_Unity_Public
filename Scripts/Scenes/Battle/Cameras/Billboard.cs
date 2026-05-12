using UnityEngine;

namespace Scenes.Battle.Cameras
{
    public class Billboard : MonoBehaviour
    {
        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (_mainCamera != null) transform.rotation = _mainCamera.transform.rotation;
        }
    }
}