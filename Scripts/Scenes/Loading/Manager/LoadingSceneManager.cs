using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Components.Effects.UI;
using Components.Effects.UI.Types;
using Core.Attributes;
using Core.Data.Models;
using Core.Managers;
using Scenes.Loading.Controller;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace Scenes.Loading.Manager
{
    public class LoadingSceneManager : MonoBehaviour
    {
        [SerializeField] private LoadingUIController loadingUIController;

        [Header("Scene Settings")]
        public string mainSceneName = "03_Main";
        public string[] preloadSceneNames =
            { "03_Main", "04_BasicInfo", "05_Projects", "06_PlayedGames", "07_RoadMaps" };

        public int ramThresholdMb = 6000;

        [Header("UI Settings")]
        public TextMeshProUGUI progressText;
        public List<ProgressLightNode> progressNodes;

        [SerializeField] private bool forceLoadMode;
        [ShowIf("forceLoadMode == true")]
        [SerializeField] private bool isSingleMode;

        // [핵심 추가] 에디터 에러 방지 및 안전한 관리를 위한 캐싱 리스트
        private readonly List<RectTransform> _allLoadedPanels = new();
        private readonly Vector2 _posBottom = new(0, -1080);

        private readonly Vector2 _posCenter = new(0, 0);
        private readonly Vector2 _posLeft = new(-1920, 0);
        private readonly Vector2 _posRight = new(1920, 0);
        private readonly Vector2 _posTop = new(0, 1080);

        private readonly Dictionary<string, RectTransform> _scenePanels = new();

        private int _currentLitCount;
        private bool _isUIAnimationComplete;

        private AsyncOperation _mainSceneAsync;
        private int _targetLitCount;

        private void Awake()
        {
            _currentLitCount = 0;
            _targetLitCount = 0;
        }

        private IEnumerator Start()
        {
            yield return StartCoroutine(loadingUIController.LoadingOpenEffect());

            StartCoroutine(ProcessLightQueue());
            StartCoroutine(MasterLoadingProcess());
        }

        private IEnumerator ProcessLightQueue()
        {
            _isUIAnimationComplete = false;

            if (progressNodes == null || progressNodes.Count == 0)
            {
                _isUIAnimationComplete = false;
                yield break;
            }

            var standardDuration = progressNodes[0].baseLight.Duration;

            while (_currentLitCount < progressNodes.Count)
            {
                var backlog = _targetLitCount - _currentLitCount;

                if (backlog > 0)
                {
                    var currentNode = progressNodes[_currentLitCount];
                    var dynamicDuration = standardDuration / backlog;

                    currentNode.SetDurationAndPlay(dynamicDuration);
                    _currentLitCount++;

                    if (progressText != null)
                    {
                        var percent = (float)_currentLitCount / progressNodes.Count * 100f;
                        progressText.text = $"{percent:F2}%";
                    }

                    yield return new WaitForSeconds(dynamicDuration);
                }
                else
                {
                    yield return null;
                }
            }

            _isUIAnimationComplete = true;
        }

        private void ProfilingMemory(int totalRam)
        {
            var usedRamUnity = Profiler.GetTotalAllocatedMemoryLong() / 1048576f;
            var usedRamGC = GC.GetTotalMemory(false) / 1048576f;

            Debug.Log(
                "==== 메모리 현황 ==== \n" +
                $"기기 전체 RAM: {totalRam} MB | \n" +
                $"유니티 사용 RAM: {usedRamUnity:F1} MB | \n" +
                $"C# 스크립트 RAM: {usedRamGC:F1} MB \n" +
                "====================="
            );
        }

        private IEnumerator MasterLoadingProcess()
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();

            yield return null;

            var currentRam = SystemInfo.systemMemorySize;
            ProfilingMemory(currentRam);
            Debug.Log($"현재 기기 RAM: {currentRam} MB");

            if (forceLoadMode)
            {
                if (isSingleMode)
                {
                    Debug.Log("[LoadingSceneManager] Force Single Load Mode");
                    GameSettingManager.SetDeviceMode(true);
                    yield return LoadSingleMode();
                }
                else
                {
                    Debug.Log("[LoadingSceneManager] Force Preload Mode");
                    GameSettingManager.SetDeviceMode(false);
                    yield return LoadPreloadMode();
                }
            }
            else
            {
                if (currentRam < ramThresholdMb)
                {
                    GameSettingManager.SetDeviceMode(true);
                    yield return LoadSingleMode();
                }
                else
                {
                    GameSettingManager.SetDeviceMode(false);
                    yield return LoadPreloadMode();
                }
            }
        }

        private IEnumerator LoadSingleMode()
        {
            Debug.Log("저사양 기기: Single 로딩 모드 작동");

            var targetScene = SceneTransitData.TargetSceneName;
            if (string.IsNullOrEmpty(targetScene)) targetScene = "03_Main";

            _mainSceneAsync = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Single);

            if (_mainSceneAsync != null) _mainSceneAsync.allowSceneActivation = false;

            if (_mainSceneAsync == null)
            {
                Debug.LogError("Main 씬 로딩 실패");
                yield break;
            }

            while (_mainSceneAsync.progress < 0.9f)
            {
                var normalizedProgress = Mathf.Clamp01(_mainSceneAsync.progress / 0.9f);
                UpdateUI(normalizedProgress);
                yield return null;
            }

            UpdateUI(1.0f);
            yield return new WaitUntil(() => _isUIAnimationComplete);

            yield return StartCoroutine(loadingUIController.LoadingCloseEffect());

            _mainSceneAsync.allowSceneActivation = true;
            yield return _mainSceneAsync;
        }

        private Vector2 GetDefaultPosition(string sceneName)
        {
            return sceneName switch
            {
                "03_Main" => _posCenter,
                "04_BasicInfo" => _posLeft,
                "05_Projects" => _posRight,
                "06_PlayedGames" => _posTop,
                "07_RoadMaps" => _posBottom,
                _ => Vector2.zero
            };
        }

        private IEnumerator LoadPreloadMode()
        {
            Debug.Log("고사양 기기: Preload(Additive) 로딩 모드 작동");

            // [안전장치 1] 로딩 씬의 캔버스를 가장 위로 끌어올려 로딩 중 다른 씬 노출 차단
            var loadingCanvas = loadingUIController.GetComponentInParent<Canvas>();
            if (loadingCanvas != null) loadingCanvas.sortingOrder = 999;

            var preloadOps = new List<AsyncOperation>();

            foreach (var sceneName in preloadSceneNames)
            {
                preloadOps.Add(SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive));
                preloadOps.Last().allowSceneActivation = false;
            }

            while (true)
            {
                var currentTotalProgress = 0f;
                foreach (var preloadOp in preloadOps) currentTotalProgress += preloadOp.progress;

                var averageProgress = currentTotalProgress / preloadOps.Count;
                var normalizedProgress = Mathf.Clamp01(averageProgress / 0.9f);
                UpdateUI(normalizedProgress);

                if (averageProgress >= 0.9f) break;

                yield return null;
            }

            UpdateUI(1.0f);
            yield return new WaitUntil(() => _isUIAnimationComplete);

            // [핵심 해결] 씬 순차적 활성화 및 즉각적인 화면 밖 이동 (겹침/번쩍임 완벽 차단)
            for (var i = 0; i < preloadOps.Count; i++)
            {
                preloadOps[i].allowSceneActivation = true;

                // 해당 씬 활성화 대기
                yield return new WaitUntil(() => preloadOps[i].isDone);

                // 카메라 렌더링이 일어나기 전에 즉시 패널을 화면 저 멀리 우주로 던져버림
                var loadedScene = SceneManager.GetSceneByName(preloadSceneNames[i]);
                var rootObjs = loadedScene.GetRootGameObjects();
                foreach (var root in rootObjs)
                {
                    var transforms = root.GetComponentsInChildren<RectTransform>(true);
                    foreach (var t in transforms)
                        if (t.CompareTag("SlideMasterPanel"))
                        {
                            t.anchoredPosition = new Vector2(99999, 99999);
                            _allLoadedPanels.Add(t);
                        }
                }

                // 패널을 안 보이게 한 상태에서 엔진에 프레임을 넘겨주어 무거운 Awake/Start 연산을 분산 처리
                yield return null;
                yield return null;
            }

            var mainScene = SceneManager.GetSceneByName(mainSceneName);
            SceneManager.SetActiveScene(mainScene);

            // 우주로 던져놨던 씬들을 원래 위치로 재배열 및 비활성화 처리
            SetupOffScreenPanels();
            SceneSlideManager.Instance.InitializeMap(_scenePanels, mainSceneName);

            // 타겟 씬의 UI 배치가 완전히 적용될 때까지 2프레임 대기
            yield return null;
            yield return null;

            // 완벽히 준비된 상태에서 로딩 화면만 부드럽게 퇴장
            yield return StartCoroutine(loadingUIController.LoadingCloseEffect());

            SceneManager.UnloadSceneAsync(gameObject.scene);
        }

        private void SetupOffScreenPanels()
        {
            var target = SceneTransitData.TargetSceneName;
            if (string.IsNullOrEmpty(target)) target = mainSceneName;
            var targetOffset = GetDefaultPosition(target);

            // [안전장치 2] GameObject.Find... 대신 캐싱된 리스트를 사용하여 에디터 참조 에러 방지
            foreach (var rt in _allLoadedPanels)
            {
                if (rt == null || rt.gameObject.scene == gameObject.scene) continue;

                var sceneName = rt.gameObject.scene.name;
                _scenePanels[sceneName] = rt;

                var myDefaultPos = GetDefaultPosition(sceneName);
                rt.anchoredPosition = myDefaultPos - targetOffset;

                // 타겟 씬이 아닌 경우 비활성화하여 깔끔하게 숨김
                if (sceneName != target)
                    rt.gameObject.SetActive(false);
                else
                    rt.gameObject.SetActive(true);
            }
        }

        private void UpdateUI(float progress)
        {
            progress = Mathf.Clamp01(progress);
            _targetLitCount = Mathf.FloorToInt(progress * progressNodes.Count);
            _targetLitCount = Mathf.Clamp(_targetLitCount, 0, progressNodes.Count);
        }

        [Serializable]
        public class ProgressLightNode
        {
            public UIFillEffect baseLight;
            public UIFillEffect bloomLight;

            public void SetDurationAndPlay(float duration)
            {
                if (baseLight != null)
                {
                    baseLight.SetDuration(duration);
                    baseLight.Play();
                }

                if (bloomLight != null)
                {
                    bloomLight.SetDuration(duration);
                    bloomLight.Play();
                }
            }
        }
    }
}