using System;
using System.Collections;
using Core.Bootstrapper;
using Core.Data.Enums;
using Core.Data.Models;
using Core.Events;
using Core.Interfaces;
using Core.Managers;
using Scenes.Main.Controller;
using UnityEngine;

namespace Scenes.Main.Manager
{
    public class MainSceneManager : MonoBehaviour, ISceneLifecycle
    {
        [Header("Scene Setup")]
        [SerializeField] private string currentScene = "03_Main";
        [SerializeField] private BGMSoundType sceneBGM = BGMSoundType.Main;

        [Header("Dependencies")]
        [Tooltip("이 씬의 UI를 담당하는 컨트롤러")]
        [SerializeField] private MainSceneUIController uiController;

        private void Awake()
        {
            if (!GlobalManagerBootstrapper.GlobalMainCamera.gameObject.activeSelf)
            {
                GlobalManagerBootstrapper.GlobalMainCamera.gameObject.SetActive(true);
            }
        }

        private void Start()
        {
            if (GameSettingManager.GetDeviceMode())
            {
                StartCoroutine(SingleLoadOn());
            }
            else
            {
                var targetScene = string.IsNullOrEmpty(SceneTransitData.TargetSceneName)
                    ? "03_Main"
                    : SceneTransitData.TargetSceneName;

                if (currentScene == targetScene)
                    // 최초 진입 시 자연스러운 페이드인 연출을 위해 SingleLoad 방식을 빌려옴
                    StartCoroutine(InitialPreloadEnterRoutine());
                else
                    // 화면 밖에 대기하는 나머지 씬들은 초기화만 수행 (투명도 1로 세팅 후 보이지 않는 곳에 대기)
                    ResetScenePreload();
            }
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

        private IEnumerator InitialPreloadEnterRoutine()
        {
            // 투명도를 0으로 만들고 UI를 초기화 (페이드인 준비)
            ResetSceneSingleLoad();

            // 로딩 씬의 페이드아웃(닫기) 연출과 자연스럽게 교차되도록 약간 대기
            yield return new WaitForSeconds(0.3f);

            // 페이드 인이 포함된 입장 연출 실행
            EnterSceneSingleLoad();
            Debug.Log($"[{currentScene}] InitialPreloadEnterRoutine: 최초 프리로드 씬 입장 완료 (페이드인 적용)");
        }

        private void HandleSlideStarted(string fromScene, string toScene)
        {
            if (toScene == currentScene)
            {
                ResetScenePreload();
                EnterScenePreload();
            }
        }

        private void HandleSlideCompleted(string arrivedScene)
        {
        }

        #region Single Load

        private IEnumerator SingleLoadOn()
        {
            ResetSceneSingleLoad();
            yield return null;
            EnterSceneSingleLoad();
            Debug.Log($"[{currentScene}] SingleLoadOn: 단일 로드 씬 입장 완료");
        }

        private void SingleLoadOff()
        {
            ExitSceneSingleLoad();
            Debug.Log($"[{currentScene}] SingleLoadOff: 단일 로드 씬 퇴장 완료");
        }

        public void ResetSceneSingleLoad()
        {
            if (uiController != null) uiController.ResetUISingleLoad();
            Debug.Log($"[{currentScene}] ResetSceneSingleLoad: 단일 로드 씬 초기화 완료");
        }

        public void EnterSceneSingleLoad()
        {
            SoundManager.Instance.PlayBgm(sceneBGM);
            if (uiController != null) uiController.PlayEnterUISingleLoad();
            Debug.Log($"[{currentScene}] EnterSceneSingleLoad: 단일 로드 씬 입장 완료");
        }

        public void ExitSceneSingleLoad()
        {
            if (uiController != null)
                StartCoroutine(uiController.PlayExitUISingleLoad());

            Debug.Log($"[{currentScene}] ExitSceneSingleLoad: 단일 로드 씬 퇴장");
        }

        #endregion

        #region Preload

        public void ResetScenePreload()
        {
            if (uiController != null) uiController.ResetUIPreload();
            Debug.Log($"[{currentScene}] ResetScenePreload: 프리로드 씬 초기화 완료");
        }

        public void EnterScenePreload()
        {
            SoundManager.Instance.PlayBgm(sceneBGM);
            if (uiController != null) uiController.PlayEnterUIPreload();
            Debug.Log($"[{currentScene}] EnterScenePreload: 프리로드 씬 입장 완료");
        }

        public void ExitScenePreload()
        {
            if (uiController != null) uiController.PlayExitUIPreload();

            Debug.Log($"[{currentScene}] ExitScenePreload: 프리로드 씬 퇴장. 진행 중인 연출 정지");
        }

        #endregion
    }
}