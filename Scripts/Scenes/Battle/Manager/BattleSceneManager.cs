using Core.Bootstrapper;
using Core.Data.Enums;
using Core.Managers;
using Scenes.Battle.Controller;
using UnityEngine;

namespace Scenes.Battle.Manager
{
    public class BattleSceneManager : MonoBehaviour
    {
        [Header("Scene Setup")]
        [SerializeField] private string currentScene = "08_Battle";

        [Header("Dependencies")]
        [Tooltip("이 씬의 UI를 담당하는 컨트롤러")]
        [SerializeField] private BattleSceneUIController uiController;

        private void Awake()
        {
            if (GlobalManagerBootstrapper.GlobalMainCamera.gameObject.activeSelf)
            {
                GlobalManagerBootstrapper.GlobalMainCamera.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            ResetScene();
            EnterScene();
        }

        private void ResetScene()
        {
            if (uiController != null) uiController.ResetUILoad();

            Debug.Log($"[{currentScene}] ResetScene: 씬 로드 초기화 완료");
        }

        private void EnterScene()
        {
            SoundManager.Instance.AddBgmToQueue(BGMSoundType.Battle1, 0.3f);
            SoundManager.Instance.AddBgmToQueue(BGMSoundType.Battle2, 0.3f);
            SoundManager.Instance.AddBgmToQueue(BGMSoundType.Battle3, 0.3f);
            SoundManager.Instance.AddBgmToQueue(BGMSoundType.Battle4, 0.3f);
            SoundManager.Instance.AddBgmToQueue(BGMSoundType.Battle5, 0.3f);
            SoundManager.Instance.AddBgmToQueue(BGMSoundType.Battle6, 0.3f);

            SoundManager.Instance.PlayQueue(true);

            if (uiController != null) StartCoroutine(uiController.PlayEnterUILoad());

            Debug.Log($"[{currentScene}] EnterSceneSingleLoad: 씬 단일 로드 입장 완료");
        }

        private void ExitScene()
        {
            if (uiController != null) uiController.PlayExitUILoad();

            SoundManager.Instance.StopQueue();

            Debug.Log($"[{currentScene}] ExitSceneSingleLoad: 씬 로드 퇴장");
        }
    }
}