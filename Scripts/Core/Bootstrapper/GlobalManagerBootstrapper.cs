using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Bootstrapper
{
    public static class GlobalManagerBootstrapper
    {
        public static Camera GlobalMainCamera { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InitializeGlobalManagers()
        {
            var prefabObject = Resources.Load<GameObject>("prefab/Bootstrapper/MainCamera");

            if (GlobalMainCamera != null || Camera.main != null)
            {
                // 이미 카메라가 존재하면 건너뜀
            }
            else if (prefabObject != null)
            {
                var managerObj = Object.Instantiate(prefabObject);
                managerObj.name = "Main Camera";
                Object.DontDestroyOnLoad(managerObj);

                GlobalMainCamera = managerObj.GetComponent<Camera>();

                Debug.Log("<color=cyan>[Bootstrapper] Main Camera has been setup</color>");
            }
            else
            {
                Debug.LogError("[Bootstrapper] Resources 폴더에 'Main Camera' 프리팹이 존재하지 않습니다.");
            }

            prefabObject = Resources.Load<GameObject>("prefab/Bootstrapper/SoundManager");

            if (GameObject.Find("SoundManager") == null && prefabObject != null)
            {
                var managerObj = Object.Instantiate(prefabObject);
                managerObj.name = "SoundManager";
                Object.DontDestroyOnLoad(managerObj);
                Debug.Log("<color=cyan>[Bootstrapper] SoundManager has been setup</color>");
            }
            else if (prefabObject == null)
            {
                Debug.LogError("[Bootstrapper] Resources 폴더에 'SoundManager' 프리팹이 존재하지 않습니다.");
            }

            prefabObject = Resources.Load<GameObject>("prefab/Bootstrapper/CustomCursor");

            if (prefabObject != null)
            {
                var managerObj = Object.Instantiate(prefabObject);
                managerObj.name = "CustomCursor";
                Object.DontDestroyOnLoad(managerObj);
                Debug.Log("<color=cyan>[Bootstrapper] CustomCursor has been Setup</color>");
            }
            else
            {
                Debug.LogError("[Bootstrapper] Resources 폴더에 'CustomCursor' 프리팹이 존재하지 않습니다.");
            }

            prefabObject = Resources.Load<GameObject>("prefab/Bootstrapper/FixedAspectRatio");

            if (prefabObject != null)
            {
                var managerObj = Object.Instantiate(prefabObject);
                managerObj.name = "FixedAspectRatio";
                Object.DontDestroyOnLoad(managerObj);
                Debug.Log("<color=cyan>[Bootstrapper] FixedAspectRatio has been setup</color>");
            }
            else
            {
                Debug.LogError("[Bootstrapper] Resources 폴더에 'FixedAspectRatio' 프리팹이 존재하지 않습니다.");
            }

            prefabObject = Resources.Load<GameObject>("prefab/Bootstrapper/EventSystem");

            if (prefabObject != null)
            {
                var managerObj = Object.Instantiate(prefabObject);
                managerObj.name = "EventSystem";
                Object.DontDestroyOnLoad(managerObj);
                Debug.Log("<color=cyan>[Bootstrapper] EventSystem has been setup</color>");
            }
            else
            {
                Debug.LogError("[Bootstrapper] Resources 폴더에 'EventSystem' 프리팹이 존재하지 않습니다.");
            }

            prefabObject = Resources.Load<GameObject>("prefab/Bootstrapper/SceneSlideManager");

            if (prefabObject != null)
            {
                var managerObj = Object.Instantiate(prefabObject);
                managerObj.name = "SceneSlideManager";
                Object.DontDestroyOnLoad(managerObj);
                Debug.Log("<color=cyan>[Bootstrapper] SceneSlideManager has been setup</color>");
            }
            else
            {
                Debug.LogError("[Bootstrapper] Resources 폴더에 'SceneSlideManager' 프리팹이 존재하지 않습니다.");
            }
        }
    }
}