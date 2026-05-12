using System.Collections;
using System.Collections.Generic;
using Components.Effects.UI.Core;
using Core.Data.Enums;
using Core.Managers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Scenes.Main.Controller
{
    public class DororongController : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private List<SfxSoundType> uiSoundType;
        private bool _isClickable = true;
        private float _nextClickTime = 0f;
        private List<UIEffect> _uiEffect;

        private void Awake()
        {
            _uiEffect = new List<UIEffect>();
            _uiEffect.AddRange(gameObject.GetComponents<UIEffect>());
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isClickable || Time.time < _nextClickTime) return;

            var random = Random.Range(0, _uiEffect.Count);
            var randomSound = Random.Range(0, uiSoundType.Count);

            var selectedSound = uiSoundType[randomSound];

            SoundManager.Instance.PlaySfx(selectedSound);

            float soundDuration = SoundManager.Instance.GetSfxDuration(selectedSound);
            _nextClickTime = Time.time + soundDuration;

            StartCoroutine(PlayEffectAndWaitRoutine(_uiEffect[random]));
        }

        private IEnumerator PlayEffectAndWaitRoutine(UIEffect effect)
        {
            _isClickable = false;

            yield return StartCoroutine(effect.PlayWaitable());

            _isClickable = true;
        }
    }
}