using System.Collections;
using System.Collections.Generic;
using Components.Effects.UI;
using Components.Effects.UI.Core;
using Components.Effects.UI.Types;
using Core.Data.Enums;
using Core.Managers;
using Scenes.Loading.Manager;
using UnityEngine;
using UnityEngine.UI;
// IEnumerator 사용을 위해 필요합니다
using Random = UnityEngine.Random;

namespace Scenes.Loading.Controller
{
    public class LightningController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Image lightningImage;
        [SerializeField] private Image lightningBloomImage;

        [SerializeField] private UIFlickerEffect uiFlickerEffect;
        [SerializeField] private UIFadeEffect uiFadeEffect;

        [Header("Effect Sequence")]
        [SerializeField] private EffectSequence lightningSequence;

        [Header("Effect Sounds")]
        [SerializeField] private List<SfxSoundType> lightningSound;

        [Header("Bloom Timings (X: Min, Y: Max)")]
        [Tooltip("나타나는 시간 범위 (짧게)")]
        [SerializeField] private Vector2 bloomFadeInTime = new(0.01f, 0.05f);
        [Tooltip("사라지는 시간 범위 (길게)")]
        [SerializeField] private Vector2 bloomFadeOutTime = new(0.15f, 0.4f);

        private Coroutine _bloomCoroutine;

        private void OnDisable()
        {
            lightningSequence.StopAll();
            if (_bloomCoroutine != null)
            {
                StopCoroutine(_bloomCoroutine);
                _bloomCoroutine = null;
            }
        }

        public void SetupAndPlay(LightningSpritePair spritePair, Vector2 position, Color baseColor)
        {
            lightningImage.enabled = true;

            lightningImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
            lightningImage.fillAmount = 0f;
            lightningImage.sprite = spritePair.baseSprite;
            lightningImage.SetNativeSize();

            var rect = lightningImage.rectTransform;
            rect.anchoredPosition = position;
            rect.localRotation =
                Quaternion.Euler(Random.Range(-30f, 30f), Random.Range(-15f, 15f), Random.Range(0, 360f));

            if (lightningBloomImage != null)
            {
                lightningBloomImage.sprite = spritePair.bloomSprite;
                // 나타나기 직전이므로 시작 알파값을 0으로 설정
                lightningBloomImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);

                var bloomRect = lightningBloomImage.rectTransform;
                bloomRect.anchorMin = Vector2.zero;
                bloomRect.anchorMax = Vector2.one;
                bloomRect.offsetMin = Vector2.zero;
                bloomRect.offsetMax = Vector2.zero;
            }

            var newFlickerConfig = uiFlickerEffect.DefaultConfig;
            newFlickerConfig.baseColor = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
            newFlickerConfig.dimColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.2f);
            uiFlickerEffect.SetProperty(newFlickerConfig);

            var newFadeConfig = uiFadeEffect.DefaultConfig;
            newFadeConfig.endAlpha = 0f;
            uiFadeEffect.SetProperty(newFadeConfig);

            transform.SetAsFirstSibling();
            gameObject.SetActive(true);
            SoundManager.Instance.PlaySfx(lightningSound[Random.Range(0, lightningSound.Count)], 0.1f);

            if (lightningBloomImage != null)
            {
                if (_bloomCoroutine != null) StopCoroutine(_bloomCoroutine);
                _bloomCoroutine = StartCoroutine(BloomFadeInRoutine(baseColor));
            }

            StartCoroutine(lightningSequence.PlaySequenceRoutine(() =>
            {
                if (gameObject.activeInHierarchy && lightningBloomImage != null)
                {
                    if (_bloomCoroutine != null) StopCoroutine(_bloomCoroutine);
                    _bloomCoroutine = StartCoroutine(BloomFadeOutRoutine(baseColor));
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }));
        }

        private IEnumerator BloomFadeInRoutine(Color baseColor)
        {
            var time = 0f;
            var duration = Random.Range(bloomFadeInTime.x, bloomFadeInTime.y);

            while (time < duration)
            {
                time += Time.deltaTime;
                var alpha = Mathf.Lerp(0f, 1f, time / duration);
                lightningBloomImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
                yield return null;
            }

            lightningBloomImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
        }

        private IEnumerator BloomFadeOutRoutine(Color baseColor)
        {
            lightningImage.enabled = false;

            var time = 0f;
            var duration = Random.Range(bloomFadeOutTime.x, bloomFadeOutTime.y);

            while (time < duration)
            {
                time += Time.deltaTime;
                var alpha = Mathf.Lerp(1f, 0f, time / duration);
                lightningBloomImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
                yield return null;
            }

            lightningBloomImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
            gameObject.SetActive(false);
        }
    }
}