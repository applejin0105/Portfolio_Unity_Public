using System;
using System.Collections.Generic;
using Core.Events;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Managers
{
    public enum SceneSlideType
    {
        Normal,
        DoTween
    }

    public class SceneSlideManager : MonoBehaviour
    {
        [Header("Slide Settings")]
        [SerializeField] private SceneSlideType sceneSlideType;
        [SerializeField] private float slideDuration = 0.8f; // 살짝 튕기는 느낌을 위해 0.8초 권장
        private readonly Vector2 _posBottom = new(0, -1080);

        private readonly Vector2 _posCenter = new(0, 0);
        private readonly Vector2 _posLeft = new(-1920, 0);
        private readonly Vector2 _posRight = new(1920, 0);
        private readonly Vector2 _posTop = new(0, 1080);
        private string _currentSceneName;
        private bool _isMoving;

        private Dictionary<string, RectTransform> _managedPanels = new();
        public static SceneSlideManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void InitializeMap(Dictionary<string, RectTransform> panels, string startScene)
        {
            _managedPanels = panels;
            _currentSceneName = startScene;
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

        public async UniTask ExecuteSlideAsync(string targetSceneName)
        {
            if (_isMoving || _currentSceneName == targetSceneName) return;

            if (!_managedPanels.ContainsKey(targetSceneName))
            {
                Debug.LogWarning($"[SceneSlideManager] {targetSceneName} 패널을 찾을 수 없습니다.");
                return;
            }

            _isMoving = true;

            var previousSceneName = _currentSceneName;

            // 타겟 씬을 먼저 활성화하여 OnEnable 이벤트 구독이 이루어지도록 순서 변경
            _managedPanels[targetSceneName].gameObject.SetActive(true);

            // 슬라이드 시작 이벤트 발송 (이제 타겟 씬이 정상적으로 이벤트를 수신합니다)
            GlobalSceneEvents.OnSceneSlideStarted?.Invoke(previousSceneName, targetSceneName);

            switch (sceneSlideType)
            {
                case SceneSlideType.Normal:
                    await SlideRoutineAsync(targetSceneName);
                    break;
                case SceneSlideType.DoTween:
                    await SlideDoTweenRoutineAsync(targetSceneName);
                    break;
                default:
                    throw new NotImplementedException();
            }

            _currentSceneName = targetSceneName;

            _isMoving = false;

            if (previousSceneName != targetSceneName && _managedPanels.ContainsKey(previousSceneName))
                _managedPanels[previousSceneName].gameObject.SetActive(false);

            // 슬라이드 완료 이벤트 발송
            GlobalSceneEvents.OnSceneSlideCompleted?.Invoke(targetSceneName);
        }

        // 뒤로 당겼다가 튕겨가는 자연스러운 수학 공식 (InOutBack) 개쩜
        private float EaseInOutBack(float x)
        {
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;

            return x < 0.5f
                ? Mathf.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2) / 2f
                : (Mathf.Pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2f;
        }

        private async UniTask SlideRoutineAsync(string targetSceneName)
        {
            var targetOffset = GetDefaultPosition(targetSceneName);
            var movementData = new List<(RectTransform panel, Vector2 startPos, Vector2 endPos)>();

            foreach (var kvp in _managedPanels)
            {
                var panel = kvp.Value;
                var startPos = panel.anchoredPosition;
                var endPos = GetDefaultPosition(kvp.Key) - targetOffset;
                movementData.Add((panel, startPos, endPos));
            }

            var elapsed = 0f;

            while (elapsed < slideDuration)
            {
                // 시간을 0~1로 정규화
                var linearProgress = Mathf.Clamp01(elapsed / slideDuration);

                // 수학 공식을 거쳐 텐션이 적용된 진행도 추출
                var easeProgress = EaseInOutBack(linearProgress);

                foreach (var data in movementData)
                {
                    // 0 미만(뒤로 가기)과 1 초과(오버슈트)를 허용하기 위해 반드시 LerpUnclamped 사용
                    var currentPos = Vector2.LerpUnclamped(data.startPos, data.endPos, easeProgress);
                    data.panel.anchoredPosition = currentPos;
                }

                elapsed += Time.unscaledDeltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            foreach (var data in movementData) data.panel.anchoredPosition = data.endPos;
        }

        private async UniTask SlideDoTweenRoutineAsync(string targetSceneName)
        {
            var targetOffset = GetDefaultPosition(targetSceneName);
            var tasks = new List<UniTask>();

            foreach (var kvp in _managedPanels)
            {
                var panel = kvp.Value;
                var endPos = GetDefaultPosition(kvp.Key) - targetOffset;

                // DOTween 내장 InOutBack Ease 적용
                var tween = panel.DOAnchorPos(endPos, slideDuration)
                    .SetUpdate(true)
                    .SetEase(Ease.InOutBack);
                tasks.Add(tween.ToUniTask());
            }

            await UniTask.WhenAll(tasks);
        }

        public void DestroyInstance()
        {
            // 들고 있던 참조 지우기
            _managedPanels.Clear();

            // 싱글톤 자리 비우기 (다음 번 0번 씬 로드 시 새 프리팹이 이 자리를 차지할 수 있게)
            Instance = null;

            // 나 자신 파괴
            Destroy(gameObject);
        }
    }
}