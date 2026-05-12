using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Scenes.PlayedGames.Controller;
using UnityEngine;
using UnityEngine.Networking;

namespace Scenes.PlayedGames.Manager
{
    public class PlayedGamesPrefabManager : MonoBehaviour
    {
        [Header("Mode & Data")]
        [SerializeField] private bool isLocalMode;
        [SerializeField] private PlayedGamesDataManager dataManager;

        [Header("Animation Method")]
        [SerializeField] private AnimType animType = AnimType.DoTween;

        [Header("UI References")]
        [SerializeField]
        private PlayedGamesPrefabController[] playedGamesContainer = new PlayedGamesPrefabController[3];

        [Header("Animation Settings")]
        [SerializeField] private float zOffset = -50f;
        [SerializeField] private float fadeAlpha = 0.3f;
        [SerializeField] private float zMoveDuration = 0.15f;
        [SerializeField] private float slideDuration = 0.25f;

        [Header("State Variables")]
        private readonly Vector2[] _fixedHidePositions = new Vector2[2];

        private readonly Dictionary<string, Texture2D> _imageCache = new();
        private readonly int _limit = 10;
        private int _currentDataCenterIndex;

        [Header("Pagination State")]
        private int _currentPage = 1;
        private Vector2 _fixedPosition;
        private bool _isAnimating;
        private int _uiHeadIndex;

        public bool IsReady { get; private set; }

        private void Awake()
        {
            _fixedHidePositions[0] = playedGamesContainer[0].GetComponent<RectTransform>().anchoredPosition;
            _fixedHidePositions[1] = playedGamesContainer[2].GetComponent<RectTransform>().anchoredPosition;
            _fixedPosition = playedGamesContainer[1].GetComponent<RectTransform>().anchoredPosition;
        }

        private void Start()
        {
            StartCoroutine(LoadInitialData());
        }

        public event Action<bool> OnCanSlideLeftChanged;
        public event Action<bool> OnCanSlideRightChanged;

        private IEnumerator LoadInitialData()
        {
            if (!isLocalMode)
                yield return StartCoroutine(dataManager.FetchDataServer(_currentPage, _limit));
            else
                dataManager.FetchDataLocal();

            InitPosition();
            SetInitialData();
            UpdateNavigationButtons();

            IsReady = true;
        }

        private void InitPosition()
        {
            if (dataManager.Length >= 2)
            {
                for (var i = 0; i < 3; i++) playedGamesContainer[i].gameObject.SetActive(true);
                return;
            }

            if (dataManager.Length == 1)
                for (var i = 0; i < 3; i++)
                {
                    if (i == 1) continue;
                    RemovePrefab(i);
                }
            else if (dataManager.Length == 0)
                for (var i = 0; i < 3; i++)
                    RemovePrefab(i);

            UpdateBoundaryUILeft();
            UpdateBoundaryUIRight();
        }

        public void ResetToInitialState()
        {
            if (dataManager == null || dataManager.Length == 0) return;

            _isAnimating = false;

            // 논리적 헤드 인덱스 초기화
            _uiHeadIndex = 0;

            // 슬라이드로 인해 뒤섞인 각 컨테이너의 물리적 위치를 원래의 고정 슬롯으로 강제 스냅
            if (playedGamesContainer[0] != null)
                playedGamesContainer[0].GetComponent<RectTransform>().anchoredPosition = _fixedHidePositions[0];
            if (playedGamesContainer[1] != null)
                playedGamesContainer[1].GetComponent<RectTransform>().anchoredPosition = _fixedPosition;
            if (playedGamesContainer[2] != null)
                playedGamesContainer[2].GetComponent<RectTransform>().anchoredPosition = _fixedHidePositions[1];

            // 회전값 초기화 및 Z축/Alpha 상태를 초기 슬롯 위치(0:Left, 1:Center, 2:Right)에 맞게 리셋
            for (var i = 0; i < 3; i++)
                if (playedGamesContainer[i] != null)
                {
                    var rect = playedGamesContainer[i].GetComponent<RectTransform>();
                    var cg = playedGamesContainer[i].GetComponent<CanvasGroup>();

                    rect.localRotation = Quaternion.identity;

                    var initialZ = i == 1 ? 0f : zOffset;
                    var initialAlpha = i == 1 ? 1f : fadeAlpha;

                    rect.anchoredPosition3D = new Vector3(rect.anchoredPosition.x, rect.anchoredPosition.y, initialZ);
                    if (cg != null) cg.alpha = initialAlpha;
                }

            // 활성화 상태, 데이터 인덱스(_currentDataCenterIndex), 네비게이션 버튼 상태를 Start 시점과 동일하게 재배치
            InitPosition();
            SetInitialData();
            UpdateNavigationButtons();
        }

        private void SetInitialData()
        {
            if (dataManager.Length == 0) return;

            _currentDataCenterIndex = 0;

            if (dataManager.Length == 1)
            {
                playedGamesContainer[1].Setup(dataManager.dataList[0], this);
                return;
            }

            for (var i = 0; i < 3; i++)
            {
                var dataIndex = _currentDataCenterIndex + (i - 1);

                if (dataIndex >= 0 && dataIndex < dataManager.Length)
                {
                    playedGamesContainer[i].gameObject.SetActive(true);
                    playedGamesContainer[i].Setup(dataManager.dataList[dataIndex], this);

                    // 초기 Z축 및 Alpha값을 슬롯 위치(0:Left, 1:Center, 2:Right)에 맞게 강제 셋팅
                    var rect = playedGamesContainer[i].GetComponent<RectTransform>();
                    var cg = playedGamesContainer[i].GetComponent<CanvasGroup>();
                    var initialZ = i == 1 ? 0f : zOffset;
                    var initialAlpha = i == 1 ? 1f : fadeAlpha;

                    rect.anchoredPosition3D = new Vector3(rect.anchoredPosition.x, rect.anchoredPosition.y, initialZ);
                    if (cg != null) cg.alpha = initialAlpha;
                }
                else
                {
                    // 범위를 벗어난 더미 컨테이너는 확실히 꺼둠
                    playedGamesContainer[i].gameObject.SetActive(false);
                }
            }
        }

        private void RemovePrefab(int removeIndex)
        {
            playedGamesContainer[removeIndex].gameObject.SetActive(false);
        }

        private void UpdateBoundaryUILeft()
        {
            OnCanSlideLeftChanged?.Invoke(_currentDataCenterIndex > 0);
        }

        private void UpdateBoundaryUIRight()
        {
            OnCanSlideRightChanged?.Invoke(_currentDataCenterIndex < dataManager.Length - 1);
        }

        public void OnClickSlideLeft()
        {
            if (_isAnimating || _currentDataCenterIndex <= 0) return;
            StartCoroutine(SlideRoutine(-1));
        }

        public void OnClickSlideRight()
        {
            if (_isAnimating || _currentDataCenterIndex >= dataManager.Length - 1) return;
            StartCoroutine(SlideRoutine(1));
        }

        private IEnumerator SlideRoutine(int direction)
        {
            return animType switch
            {
                AnimType.Normal => SlideRoutineNormal(direction),
                AnimType.DoTween => SlideRoutineDoTween(direction),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private IEnumerator SlideRoutineNormal(int direction)
        {
            _isAnimating = true;

            var totalDuration = zMoveDuration * 2 + slideDuration;
            var startPos = new Vector2[3];
            var targetPos = new Vector2[3];
            var targetSlots = new int[3];
            var currentSlots = new int[3];

            for (var i = 0; i < 3; i++)
            {
                if (playedGamesContainer[i] == null || !playedGamesContainer[i].gameObject.activeSelf) continue;

                var rect = playedGamesContainer[i].GetComponent<RectTransform>();
                startPos[i] = rect.anchoredPosition;

                currentSlots[i] = (i - _uiHeadIndex + 3) % 3;
                targetSlots[i] = currentSlots[i] - direction;

                switch (targetSlots[i])
                {
                    case -1: targetPos[i] = _fixedHidePositions[0] + (_fixedHidePositions[0] - _fixedPosition); break;
                    case 0: targetPos[i] = _fixedHidePositions[0]; break;
                    case 1: targetPos[i] = _fixedPosition; break;
                    case 2: targetPos[i] = _fixedHidePositions[1]; break;
                    case 3: targetPos[i] = _fixedHidePositions[1] + (_fixedHidePositions[1] - _fixedPosition); break;
                }
            }

            var elapsed = 0f;
            while (elapsed < totalDuration)
            {
                elapsed += Time.deltaTime;

                for (var i = 0; i < 3; i++)
                {
                    if (playedGamesContainer[i] == null || !playedGamesContainer[i].gameObject.activeSelf) continue;

                    var rect = playedGamesContainer[i].GetComponent<RectTransform>();
                    var cg = playedGamesContainer[i].GetComponent<CanvasGroup>();

                    var cSlot = currentSlots[i];
                    var tSlot = targetSlots[i];

                    if (elapsed <= zMoveDuration)
                    {
                        // Phase 1: 기존 Center 요소만 뒤로 밀려남 (나머지 배경 요소는 가만히 있음)
                        var p = elapsed / zMoveDuration;
                        if (cSlot == 1)
                        {
                            rect.anchoredPosition3D =
                                new Vector3(startPos[i].x, startPos[i].y, Mathf.Lerp(0, zOffset, p));
                            if (cg != null) cg.alpha = Mathf.Lerp(1f, fadeAlpha, p);
                        }
                    }
                    else if (elapsed <= zMoveDuration + slideDuration)
                    {
                        // Phase 2: 옆으로 슬라이드 (화면 밖으로 나가는 요소는 페이드 아웃)
                        var p = (elapsed - zMoveDuration) / slideDuration;
                        var easeOutCubic = 1f - Mathf.Pow(1f - p, 3f);
                        var currentXY = Vector2.Lerp(startPos[i], targetPos[i], easeOutCubic);
                        rect.anchoredPosition3D = new Vector3(currentXY.x, currentXY.y, zOffset);

                        if (cg != null && (tSlot == -1 || tSlot == 3)) cg.alpha = Mathf.Lerp(fadeAlpha, 0f, p);
                    }
                    else
                    {
                        // Phase 3: 새롭게 Center로 들어온 요소만 팝업되어 전진함
                        var p = (elapsed - zMoveDuration - slideDuration) / zMoveDuration;
                        var currentZ = tSlot == 1 ? Mathf.Lerp(zOffset, 0, p) : zOffset;
                        rect.anchoredPosition3D = new Vector3(targetPos[i].x, targetPos[i].y, currentZ);

                        if (cg != null && tSlot == 1) cg.alpha = Mathf.Lerp(fadeAlpha, 1f, p);
                    }
                }

                yield return null;
            }

            FinalizeSlideMove(direction, targetPos, targetSlots);
        }

        private IEnumerator SlideRoutineDoTween(int direction)
        {
            _isAnimating = true;
            var slideSeq = DOTween.Sequence();

            var targetPos = new Vector2[3];
            var targetSlots = new int[3];

            for (var i = 0; i < 3; i++)
            {
                if (playedGamesContainer[i] == null || !playedGamesContainer[i].gameObject.activeSelf) continue;

                var currentSlot = (i - _uiHeadIndex + 3) % 3;
                var targetSlot = currentSlot - direction;
                targetSlots[i] = targetSlot;

                switch (targetSlot)
                {
                    case -1: targetPos[i] = _fixedHidePositions[0] + (_fixedHidePositions[0] - _fixedPosition); break;
                    case 0: targetPos[i] = _fixedHidePositions[0]; break;
                    case 1: targetPos[i] = _fixedPosition; break;
                    case 2: targetPos[i] = _fixedHidePositions[1]; break;
                    case 3: targetPos[i] = _fixedHidePositions[1] + (_fixedHidePositions[1] - _fixedPosition); break;
                }

                var rect = playedGamesContainer[i].GetComponent<RectTransform>();
                var cg = playedGamesContainer[i].GetComponent<CanvasGroup>();

                // Phase 1: 기존 Center 요소만 뒤로 밀려남
                if (currentSlot == 1)
                {
                    slideSeq.Insert(0f, rect.DOAnchorPos3DZ(zOffset, zMoveDuration).SetEase(Ease.OutQuad));
                    if (cg != null) slideSeq.Insert(0f, cg.DOFade(fadeAlpha, zMoveDuration).SetEase(Ease.OutQuad));
                }

                // Phase 2: 옆으로 슬라이드 (화면 밖으로 나가는 요소는 페이드 아웃)
                slideSeq.Insert(zMoveDuration, rect.DOAnchorPos(targetPos[i], slideDuration).SetEase(Ease.InOutCubic));
                if (cg != null && (targetSlot == -1 || targetSlot == 3))
                    slideSeq.Insert(zMoveDuration, cg.DOFade(0f, slideDuration).SetEase(Ease.InOutCubic));

                // Phase 3: 새롭게 Center로 들어온 요소만 전진함
                if (targetSlot == 1)
                {
                    slideSeq.Insert(zMoveDuration + slideDuration,
                        rect.DOAnchorPos3DZ(0f, zMoveDuration).SetEase(Ease.InQuad));
                    if (cg != null)
                        slideSeq.Insert(zMoveDuration + slideDuration,
                            cg.DOFade(1f, zMoveDuration).SetEase(Ease.InQuad));
                }
            }

            yield return slideSeq.WaitForCompletion();
            FinalizeSlideMove(direction, targetPos, targetSlots);
        }

        private void FinalizeSlideMove(int direction, Vector2[] finalTargetPos, int[] targetSlots)
        {
            for (var i = 0; i < 3; i++)
                if (playedGamesContainer[i] != null && playedGamesContainer[i].gameObject.activeSelf)
                {
                    var rect = playedGamesContainer[i].GetComponent<RectTransform>();
                    var cg = playedGamesContainer[i].GetComponent<CanvasGroup>();
                    var tSlot = targetSlots[i];

                    if (finalTargetPos != null && finalTargetPos.Length > i)
                    {
                        // Center 슬롯(1)인 경우에만 Z=0으로 설정, 그 외(0,2,-1,3)는 배경이므로 zOffset 유지
                        var finalZ = tSlot == 1 ? 0f : zOffset;
                        rect.anchoredPosition3D = new Vector3(finalTargetPos[i].x, finalTargetPos[i].y, finalZ);
                    }

                    if (cg != null)
                        // Center 슬롯만 Alpha 1, 완전 벗어나는 슬롯은 0, 배경 대기열은 fadeAlpha 유지
                        cg.alpha = tSlot == 1 ? 1f : tSlot == -1 || tSlot == 3 ? 0f : fadeAlpha;
                }

            UpdateFramesAfterSlide(direction);
            UpdateNavigationButtons();
            _isAnimating = false;
        }

        private void UpdateFramesAfterSlide(int direction)
        {
            _currentDataCenterIndex += direction;
            _uiHeadIndex = (_uiHeadIndex + direction + 3) % 3;

            var wrappedIndex = direction == 1 ? (_uiHeadIndex + 2) % 3 : _uiHeadIndex;
            var hidePosIndex = direction == 1 ? 1 : 0; // 1이면 Right, -1이면 Left
            var newDataIndex = _currentDataCenterIndex + (direction == 1 ? 1 : -1);

            var rect = playedGamesContainer[wrappedIndex].GetComponent<RectTransform>();
            var cg = playedGamesContainer[wrappedIndex].GetComponent<CanvasGroup>();

            rect.anchoredPosition3D = new Vector3(_fixedHidePositions[hidePosIndex].x,
                _fixedHidePositions[hidePosIndex].y, zOffset);
            if (cg != null) cg.alpha = fadeAlpha;

            if (newDataIndex >= 0 && newDataIndex < dataManager.Length)
            {
                playedGamesContainer[wrappedIndex].gameObject.SetActive(true);
                playedGamesContainer[wrappedIndex].Setup(dataManager.dataList[newDataIndex], this);
            }
            else
            {
                playedGamesContainer[wrappedIndex].gameObject.SetActive(false);
            }
        }

        private void UpdateNavigationButtons()
        {
            if (dataManager.Length < 2)
            {
                OnCanSlideLeftChanged?.Invoke(false);
                OnCanSlideRightChanged?.Invoke(false);
            }
            else
            {
                UpdateBoundaryUILeft();
                UpdateBoundaryUIRight();
            }
        }

        private IEnumerator LoadNextPageData()
        {
            _currentPage++;
            yield return StartCoroutine(dataManager.FetchDataServer(_currentPage, _limit));
        }

        public IEnumerator LoadImageFromServer(string url, Action<Texture2D> onComplete)
        {
            if (string.IsNullOrEmpty(url))
            {
                onComplete?.Invoke(null);
                yield break;
            }

            if (_imageCache.TryGetValue(url, out var cachedTex))
            {
                onComplete?.Invoke(cachedTex);
                yield break;
            }

            using var uwr = UnityWebRequestTexture.GetTexture(url);
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ConnectionError ||
                uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                onComplete?.Invoke(null);
            }
            else
            {
                var tex = DownloadHandlerTexture.GetContent(uwr);
                _imageCache[url] = tex;
                onComplete?.Invoke(tex);
            }
        }

        public IEnumerator LoadImageFromLocal(string path, Action<Texture2D> onComplete)
        {
            if (string.IsNullOrEmpty(path))
            {
                onComplete?.Invoke(null);
                yield break;
            }

            if (_imageCache.TryGetValue(path, out var cachedTex))
            {
                onComplete?.Invoke(cachedTex);
                yield break;
            }

            var request = Resources.LoadAsync<Texture2D>(path);
            yield return request;
            if (request.asset != null)
            {
                var tex = request.asset as Texture2D;
                _imageCache[path] = tex;
                onComplete?.Invoke(tex);
            }
            else
            {
                onComplete?.Invoke(null);
            }
        }

        private enum AnimType
        {
            Normal,
            DoTween
        }
    }
}