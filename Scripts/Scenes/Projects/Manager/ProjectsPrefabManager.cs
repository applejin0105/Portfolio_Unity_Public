using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Scenes.Projects.Controller;
using UnityEngine;
using UnityEngine.Networking;

namespace Scenes.Projects.Manager
{
    public class ProjectsPrefabManager : MonoBehaviour
    {
        private const float FrameOffset = 284.5f;

        [Header("Mode & Data")]
        [SerializeField] private bool isLocalMode;
        [SerializeField] private ProjectDataManager dataManager;

        [Header("Animation Method")]
        [SerializeField] private AnimType animType = AnimType.DoTween;

        [Header("UI References")]
        [SerializeField] private ProjectsPrefabController[] projectContainer = new ProjectsPrefabController[5];

        [Header("State Variables")]
        private readonly Vector2[] _fixedHidePositions = new Vector2[2];
        private readonly Vector2[] _fixedPositions = new Vector2[3];

        private readonly Dictionary<string, Texture2D> _imageCache = new();
        private readonly int _limit = 10;
        private int _currentDataCenterIndex;

        [Header("Pagination State")]
        private int _currentPage = 1;
        private bool _isAnimating;
        private int _uiHeadIndex;

        public bool IsReady { get; private set; }

        private void Awake()
        {
            _fixedHidePositions[0] = projectContainer[0].GetComponent<RectTransform>().anchoredPosition;
            _fixedHidePositions[1] = projectContainer[4].GetComponent<RectTransform>().anchoredPosition;

            _fixedPositions[0] = projectContainer[1].GetComponent<RectTransform>().anchoredPosition;
            _fixedPositions[1] = projectContainer[2].GetComponent<RectTransform>().anchoredPosition;
            _fixedPositions[2] = projectContainer[3].GetComponent<RectTransform>().anchoredPosition;
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
            if (dataManager.Length >= 4)
            {
                for (var i = 0; i < 5; i++) projectContainer[i].gameObject.SetActive(true);
                return;
            }

            switch (dataManager.Length)
            {
                case 0:
                    for (var i = 0; i < 5; i++) RemovePrefab(i);
                    break;
                case 1:
                    for (var i = 0; i < 5; i++)
                    {
                        if (i == 2) continue;
                        RemovePrefab(i);
                    }

                    break;
                case 2:
                    for (var i = 0; i < 5; i++)
                    {
                        if (i is 1 or 2) continue;
                        RemovePrefab(i);
                    }

                    projectContainer[1].gameObject.GetComponent<RectTransform>().anchoredPosition =
                        new Vector2(-FrameOffset, 0);
                    projectContainer[2].gameObject.GetComponent<RectTransform>().anchoredPosition =
                        new Vector2(FrameOffset, 0);
                    break;
                case 3:
                    for (var i = 0; i < 5; i++)
                    {
                        if (i is 1 or 2 or 3) continue;
                        RemovePrefab(i);
                    }

                    break;
            }

            UpdateBoundaryUILeft();
            UpdateBoundaryUIRight();
        }

        public void ResetToInitialState()
        {
            if (dataManager == null || dataManager.Length == 0) return;

            _isAnimating = false;

            _uiHeadIndex = 0;

            if (projectContainer[0] != null)
                projectContainer[0].GetComponent<RectTransform>().anchoredPosition = _fixedHidePositions[0];
            if (projectContainer[1] != null)
                projectContainer[1].GetComponent<RectTransform>().anchoredPosition = _fixedPositions[0];
            if (projectContainer[2] != null)
                projectContainer[2].GetComponent<RectTransform>().anchoredPosition = _fixedPositions[1];
            if (projectContainer[3] != null)
                projectContainer[3].GetComponent<RectTransform>().anchoredPosition = _fixedPositions[2];
            if (projectContainer[4] != null)
                projectContainer[4].GetComponent<RectTransform>().anchoredPosition = _fixedHidePositions[1];

            for (var i = 0; i < 5; i++)
                if (projectContainer[i] != null)
                    projectContainer[i].GetComponent<RectTransform>().localRotation = Quaternion.identity;

            InitPosition();
            SetInitialData();
            UpdateNavigationButtons();
        }

        private void SetInitialData()
        {
            if (dataManager.Length == 0) return;

            if (dataManager.Length == 1)
            {
                _currentDataCenterIndex = 0;
                projectContainer[2].Setup(dataManager.dataList[0], this);
                return;
            }

            if (dataManager.Length == 2)
            {
                _currentDataCenterIndex = 1;
                projectContainer[1].Setup(dataManager.dataList[0], this);
                projectContainer[2].Setup(dataManager.dataList[1], this);
                return;
            }

            _currentDataCenterIndex = 1;

            for (var i = 0; i < 5; i++)
            {
                if (!projectContainer[i].gameObject.activeSelf) continue;
                var dataIndex = _currentDataCenterIndex + (i - 2);
                if (dataIndex < 0 || dataIndex >= dataManager.Length) continue;
                projectContainer[i].Setup(dataManager.dataList[dataIndex], this);
            }
        }

        private void RemovePrefab(int removeIndex)
        {
            projectContainer[removeIndex].gameObject.SetActive(false);
        }

        private void UpdateBoundaryUILeft()
        {
            OnCanSlideLeftChanged?.Invoke(_currentDataCenterIndex > 1);
        }

        private void UpdateBoundaryUIRight()
        {
            OnCanSlideRightChanged?.Invoke(_currentDataCenterIndex < dataManager.Length - 2);
        }

        public void OnClickSlideLeft()
        {
            if (_isAnimating || _currentDataCenterIndex <= 1) return;
            StartCoroutine(SlideRoutine(-1));
        }

        public void OnClickSlideRight()
        {
            if (_isAnimating || _currentDataCenterIndex >= dataManager.Length - 2) return;
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
            var slideDuration = 0.35f;
            var punchZ = direction * 8f;

            var startPos = new Vector2[5];
            var targetPos = new Vector2[5];

            for (var i = 0; i < 5; i++)
            {
                if (projectContainer[i] == null || !projectContainer[i].gameObject.activeSelf) continue;

                var rect = projectContainer[i].GetComponent<RectTransform>();
                startPos[i] = rect.anchoredPosition;
                rect.localRotation = Quaternion.identity;

                var currentSlot = (i - _uiHeadIndex + 5) % 5;
                var targetSlot = currentSlot - direction;

                switch (targetSlot)
                {
                    case -1:
                        targetPos[i] = _fixedHidePositions[0] + (_fixedHidePositions[0] - _fixedPositions[0]); break;
                    case 0: targetPos[i] = _fixedHidePositions[0]; break;
                    case 1: targetPos[i] = _fixedPositions[0]; break;
                    case 2: targetPos[i] = _fixedPositions[1]; break;
                    case 3: targetPos[i] = _fixedPositions[2]; break;
                    case 4: targetPos[i] = _fixedHidePositions[1]; break;
                    case 5:
                        targetPos[i] = _fixedHidePositions[1] + (_fixedHidePositions[1] - _fixedPositions[2]); break;
                }
            }

            var elapsed = 0f;
            while (elapsed < slideDuration)
            {
                elapsed += Time.deltaTime;
                var progress = Mathf.Clamp01(elapsed / slideDuration);
                var easeOutCubic = 1f - Mathf.Pow(1f - progress, 3f);

                var damp = 1f - progress;
                var currentPunchZ = Mathf.Sin(progress * Mathf.PI * 4f) * damp * punchZ;
                var punchRot = Quaternion.Euler(0, 0, currentPunchZ);

                for (var i = 0; i < 5; i++)
                {
                    if (projectContainer[i] == null || !projectContainer[i].gameObject.activeSelf) continue;
                    var rect = projectContainer[i].GetComponent<RectTransform>();

                    rect.anchoredPosition = Vector2.Lerp(startPos[i], targetPos[i], easeOutCubic);
                    rect.localRotation = punchRot;
                }

                yield return null;
            }

            FinalizeSlideMove(direction, targetPos);
        }

        private IEnumerator SlideRoutineDoTween(int direction)
        {
            _isAnimating = true;
            var slideSeq = DOTween.Sequence();
            var slideDuration = 0.35f;
            var punchAngle = new Vector3(0, 0, direction * 8f);

            var targetPos = new Vector2[5];

            for (var i = 0; i < 5; i++)
            {
                if (projectContainer[i] == null || !projectContainer[i].gameObject.activeSelf) continue;

                var currentSlot = (i - _uiHeadIndex + 5) % 5;
                var targetSlot = currentSlot - direction;

                switch (targetSlot)
                {
                    case -1:
                        targetPos[i] = _fixedHidePositions[0] + (_fixedHidePositions[0] - _fixedPositions[0]); break;
                    case 0: targetPos[i] = _fixedHidePositions[0]; break;
                    case 1: targetPos[i] = _fixedPositions[0]; break;
                    case 2: targetPos[i] = _fixedPositions[1]; break;
                    case 3: targetPos[i] = _fixedPositions[2]; break;
                    case 4: targetPos[i] = _fixedHidePositions[1]; break;
                    case 5:
                        targetPos[i] = _fixedHidePositions[1] + (_fixedHidePositions[1] - _fixedPositions[2]); break;
                }

                var rect = projectContainer[i].GetComponent<RectTransform>();
                rect.localRotation = Quaternion.identity;

                slideSeq.Join(rect.DOAnchorPos(targetPos[i], slideDuration).SetEase(Ease.OutCubic));
                slideSeq.Join(rect.DOPunchRotation(punchAngle, slideDuration, 4, 0.5f));
            }

            yield return slideSeq.WaitForCompletion();
            FinalizeSlideMove(direction, targetPos);
        }

        private void FinalizeSlideMove(int direction, Vector2[] finalTargetPos)
        {
            for (var i = 0; i < 5; i++)
                if (projectContainer[i] != null && projectContainer[i].gameObject.activeSelf)
                {
                    if (finalTargetPos != null && finalTargetPos.Length > i)
                        projectContainer[i].GetComponent<RectTransform>().anchoredPosition = finalTargetPos[i];

                    projectContainer[i].GetComponent<RectTransform>().localRotation = Quaternion.identity;
                }

            UpdateFramesAfterSlide(direction);
            UpdateNavigationButtons();
            _isAnimating = false;
        }

        private void UpdateFramesAfterSlide(int direction)
        {
            _currentDataCenterIndex += direction;
            _uiHeadIndex = (_uiHeadIndex + direction + 5) % 5;

            if (direction == 1)
            {
                var wrappedIndex = (_uiHeadIndex + 4) % 5;
                projectContainer[wrappedIndex].GetComponent<RectTransform>().anchoredPosition = _fixedHidePositions[1];

                var newDataIndex = _currentDataCenterIndex + 2;
                if (newDataIndex < dataManager.Length)
                {
                    projectContainer[wrappedIndex].gameObject.SetActive(true);
                    projectContainer[wrappedIndex].Setup(dataManager.dataList[newDataIndex], this);
                }
                else
                {
                    projectContainer[wrappedIndex].gameObject.SetActive(false);
                }
            }
            else if (direction == -1)
            {
                var wrappedIndex = _uiHeadIndex;
                projectContainer[wrappedIndex].GetComponent<RectTransform>().anchoredPosition = _fixedHidePositions[0];

                var newDataIndex = _currentDataCenterIndex - 2;
                if (newDataIndex >= 0)
                {
                    projectContainer[wrappedIndex].gameObject.SetActive(true);
                    projectContainer[wrappedIndex].Setup(dataManager.dataList[newDataIndex], this);
                }
                else
                {
                    projectContainer[wrappedIndex].gameObject.SetActive(false);
                }
            }
        }

        private void UpdateNavigationButtons()
        {
            if (dataManager.Length < 4)
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