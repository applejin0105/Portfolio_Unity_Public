using System;
using System.Collections;
using System.Collections.Generic;
using Core.Data.Enums;
using Core.Extensions;
using Core.Managers;
using Scenes.Loading.Controller;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scenes.Loading.Manager
{
    [Serializable]
    public struct LightningSpritePair
    {
        public Sprite baseSprite;
        public Sprite bloomSprite;
    }

    public class LightningManager : MonoBehaviour
    {
        [Header("Lightning Settings")]
        public LightningSpritePair[] lightningPairs;
        [ColorUsage(true, true)]
        public Color lightningColor = new(0.255f, 2.890f, 8.000f, 1.000f);
        public float minSpawnDelay;
        public float maxSpawnDelay;
        public int maxLightningsAtOnce;
        public float edgePadding;

        [Header("Lightning Pools")]
        [SerializeField] private LightningController lightningPrefab;
        [SerializeField] private int poolSize = 10;

        [Header("LoadingManager Parameters")]
        [SerializeField] private Canvas parentCanvas;
        private readonly List<LightningController> _lightningPool = new();
        private RectTransform _canvasRect;
        private float _halfHeight;

        private float _halfWidth;

        private bool _isSpawningLightning;

        private void Awake()
        {
            _canvasRect = parentCanvas.GetComponent<RectTransform>();

            for (var i = 0; i < poolSize; i++)
            {
                var clone = Instantiate(lightningPrefab, parentCanvas.transform);
                clone.gameObject.SetActive(false);
                _lightningPool.Add(clone);
            }
        }

        private IEnumerator Start()
        {
            // 유니티 게이게이야, 캔버스 레이아웃 계산 다 할때까지 한 프레임만 쉬렴
            yield return null;
            SoundManager.Instance.PlayBgm(BGMSoundType.Loading);
            SetScreenSize();

            _isSpawningLightning = true;

            StartCoroutine(SpawnLightningLoop());
        }

        public void OnEnable()
        {
            if (FixedAspectRatio.Instance != null)
                FixedAspectRatio.Instance.OnResolutionChanged
                    += SetScreenSize;
        }

        public void OnDisable()
        {
            if (FixedAspectRatio.Instance != null)
                FixedAspectRatio.Instance.OnResolutionChanged
                    -= SetScreenSize;
        }

        private void SetScreenSize()
        {
            _halfWidth = Mathf.Max(0, _canvasRect.rect.width / 2f - edgePadding);
            _halfHeight = Mathf.Max(0, _canvasRect.rect.height / 2f - edgePadding);
        }

        public void StopLightningEffect()
        {
            _isSpawningLightning = false;
        }

        private IEnumerator SpawnLightningLoop()
        {
            while (_isSpawningLightning)
            {
                var waitTime = Random.Range(minSpawnDelay, maxSpawnDelay);
                yield return new WaitForSeconds(waitTime);

                var spawnCount = Random.Range(1, maxLightningsAtOnce + 1);

                for (var i = 0; i < spawnCount; i++)
                {
                    var availableLightning = GetAvailableImage();
                    if (availableLightning != null) FireLightning(availableLightning);

                    if (spawnCount > 1) yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
                }
            }
        }

        private LightningController GetAvailableImage()
        {
            foreach (var controller in _lightningPool)
                if (!controller.gameObject.activeSelf)
                    return controller;

            return null;
        }

        private void FireLightning(LightningController controller)
        {
            var pair = lightningPairs[Random.Range(0, lightningPairs.Length)];

            var x = Random.Range(-_halfWidth, _halfWidth);
            var y = Random.Range(-_halfHeight, _halfHeight);

            controller.SetupAndPlay(
                pair,
                new Vector2(x, y),
                lightningColor);
        }
    }
}