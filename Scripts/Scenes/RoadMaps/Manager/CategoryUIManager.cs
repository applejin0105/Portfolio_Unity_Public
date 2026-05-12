using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Scenes.RoadMaps.Manager
{
    public class CategoryUIManager : MonoBehaviour
    {
        [Header("UI References")]
        public ScrollRect scrollView;
        public Transform contentParent;
        public GameObject[] itemPrefabs;

        [Header("Interaction Control")]
        public CanvasGroup canvasGroup;
        private readonly List<GameObject> activeObjects = new();
        private readonly Dictionary<GameObject, int> objectToPoolIndex = new();

        private ObjectPool<GameObject>[] pools;

        private void Awake()
        {
            InitializePools();
            SetupScrollView();
            SetInteractable(false);
        }

        private void InitializePools()
        {
            pools = new ObjectPool<GameObject>[itemPrefabs.Length];

            for (var i = 0; i < itemPrefabs.Length; i++)
            {
                var index = i;

                pools[i] = new ObjectPool<GameObject>(
                    () =>
                    {
                        var obj = Instantiate(itemPrefabs[index], contentParent, false);
                        return obj;
                    },
                    obj =>
                    {
                        obj.SetActive(true);
                        obj.transform.SetAsLastSibling();
                        obj.transform.localScale = Vector3.one;

                        var rectTransform = obj.GetComponent<RectTransform>();
                        if (rectTransform != null)
                        {
                            rectTransform.anchoredPosition3D = Vector3.zero;
                            rectTransform.localRotation = Quaternion.identity;
                        }
                    },
                    obj => obj.SetActive(false),
                    Destroy,
                    false,
                    10,
                    50
                );
            }
        }

        private void SetupScrollView()
        {
            if (scrollView == null) return;

            if (scrollView.horizontalScrollbar != null)
            {
                Destroy(scrollView.horizontalScrollbar.gameObject);
                scrollView.horizontalScrollbar = null;
            }

            scrollView.horizontal = false;
            scrollView.vertical = true;
            scrollView.scrollSensitivity = 25f;
        }

        public void ChangeContent(int categoryIndex)
        {
            foreach (var obj in activeObjects)
            {
                if (obj != null && obj.activeSelf && objectToPoolIndex.TryGetValue(obj, out int poolIndex))
                {
                    pools[poolIndex].Release(obj);
                }
            }

            activeObjects.Clear();
            objectToPoolIndex.Clear();

            if (categoryIndex < 0 || categoryIndex >= pools.Length)
            {
                SetInteractable(false);
                return;
            }

            var newObj = pools[categoryIndex].Get();
            activeObjects.Add(newObj);
            objectToPoolIndex.Add(newObj, categoryIndex);

            SetInteractable(true);
            ResetScrollPosition();
        }

        private void ResetScrollPosition()
        {
            if (scrollView != null) scrollView.normalizedPosition = new Vector2(0, 1);
        }

        private void SetInteractable(bool state)
        {
            if (canvasGroup != null)
            {
                canvasGroup.interactable = state;
                canvasGroup.blocksRaycasts = state;
            }
        }
    }
}