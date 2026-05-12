using System.Collections;
using Core.Data.Enums;
using Core.Events;
using Core.Interfaces;
using Core.Managers;
using Scenes.PlayedGames.Controller;
using UnityEngine;

namespace Scenes.PlayedGames.Manager
{
    public class PlayedGamesSceneManager : MonoBehaviour, ISceneLifecycle
    {
        [Header("Scene Setup")]
        [SerializeField] private string currentScene = "06_PlayedGames";
        [SerializeField] private BGMSoundType sceneBGM = BGMSoundType.Main;

        [Header("Dependencies")]
        [Tooltip("이 씬의 UI를 담당하는 컨트롤러")]
        [SerializeField] private PlayedGamesUIController uiController;

        private void Start()
        {
            if (GameSettingManager.GetDeviceMode()) StartCoroutine(SingleLoadOn());
        }


        private void OnEnable()
        {
            GlobalSceneEvents.OnSceneSlideStarted += HandleSlideStarted;
            GlobalSceneEvents.OnSceneSlideCompleted += HandleSlideCompleted;
        }

        private void OnDisable()
        {
            GlobalSceneEvents.OnSceneSlideStarted -= HandleSlideStarted;
            GlobalSceneEvents.OnSceneSlideCompleted -= HandleSlideCompleted;
        }

        private void HandleSlideStarted(string fromScene, string toScene)
        {
            if (fromScene == currentScene)
                // 슬라이드 이동 시 Preload 방식 퇴장
                ExitScenePreload();
            else if (toScene == currentScene)
                // 도착할 씬이 현재 씬이면 Preload 방식 초기화
                ResetScenePreload();
        }

        private void HandleSlideCompleted(string arrivedScene)
        {
            if (arrivedScene == currentScene)
                // 슬라이드 완료 시 Preload 방식 입장
                EnterScenePreload();
        }

        #region Single Load

        private IEnumerator SingleLoadOn()
        {
            ResetSceneSingleLoad(); // 초기화 먼저 실행
            yield return null;
            EnterSceneSingleLoad(); // 페이드 인 등 입장 연출 실행
            Debug.Log($"[{currentScene}] SingleLoadOn: 단일 로드 씬 입장 완료");
        }

        private void SingleLoadOff()
        {
            ExitSceneSingleLoad(); // 페이드 아웃 등 퇴장 연출 실행
            Debug.Log($"[{currentScene}] SingleLoadOff: 단일 로드 씬 퇴장 완료");
        }

        public void ResetSceneSingleLoad()
        {
            if (uiController != null) uiController.ResetUISingleLoad();

            Debug.Log($"[{currentScene}] ResetSceneSingleLoad: 씬 단일 로드 초기화 완료");
        }

        public void EnterSceneSingleLoad()
        {
            SoundManager.Instance.PlayBgm(sceneBGM);

            if (uiController != null) uiController.PlayEnterUISingleLoad();

            Debug.Log($"[{currentScene}] EnterSceneSingleLoad: 씬 단일 로드 입장 완료");
        }

        public void ExitSceneSingleLoad()
        {
            if (uiController != null) uiController.PlayExitUISingleLoad();

            Debug.Log($"[{currentScene}] ExitSceneSingleLoad: 씬 단일 로드 퇴장");
        }

        #endregion

        #region Preload (프리로드)

        public void ResetScenePreload()
        {
            if (uiController != null) uiController.ResetUIPreload();

            Debug.Log($"[{currentScene}] ResetScenePreload: 씬 프리로드 초기화 완료");
        }

        public void EnterScenePreload()
        {
            SoundManager.Instance.PlayBgm(sceneBGM);

            if (uiController != null) uiController.PlayEnterUIPreload();

            Debug.Log($"[{currentScene}] EnterScenePreload: 씬 프리로드 입장 완료");
        }

        public void ExitScenePreload()
        {
            if (uiController != null) uiController.PlayExitUIPreload();

            Debug.Log($"[{currentScene}] ExitScenePreload: 씬 프리로드 퇴장");
        }

        #endregion
    }
}