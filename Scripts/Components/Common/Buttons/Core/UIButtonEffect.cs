using System.Collections;
using Core.Attributes;
using Core.Data.Enums;
using Core.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Components.Common.Buttons.Core
{
    public abstract class UIButtonEffect :
        MonoBehaviour,
        IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public enum ButtonState
        {
            Normal,
            Hover,
            Pressed,
            Disabled
        }

        [Header("Method")]
        [SerializeField] protected AnimType animType;

        [Header("Target RectTransform")]
        [Tooltip("None 설정 시, 자기 자신의 Target Rect로 설정")]
        [SerializeField] protected RectTransform targetRectTransform = null!;

        [Header("Sounds")]
        [SerializeField] protected bool useHoverSound;
        [ShowIf("useHoverSound == true")]
        [SerializeField] protected SfxSoundType hoverSound;
        [ShowIf("useHoverSound == true")]
        [SerializeField] protected float hoverSoundVolume;

        [SerializeField] protected bool usePressSound;
        [ShowIf("usePressSound == true")]
        [SerializeField] protected SfxSoundType pressSound;
        [ShowIf("usePressSound == true")]
        [SerializeField] protected float pressSoundVolume;

        [Header("State")]
        [SerializeField] protected ButtonState currentState = ButtonState.Normal;

        [Header("Interactable")]
        [SerializeField] protected bool isInteractable = true;

        [Header("Events")]
        public UnityEvent onClickEvent;
        public UnityEvent onPointerDownEvent;
        public UnityEvent onPointerUpEvent;
        public UnityEvent onPointerEnterEvent;
        public UnityEvent onPointerExitEvent;

        private float _enableTime;

        protected Coroutine EffectCoroutine;

        protected bool IsHovering;
        protected bool IsPressed;

        public bool IsInteractable
        {
            get => isInteractable;
            set
            {
                if (isInteractable == value) return;
                isInteractable = value;
                UpdateButtonState();
            }
        }

        protected virtual void Awake()
        {
            if (targetRectTransform == null)
            {
                targetRectTransform = GetComponent<RectTransform>();
                if (targetRectTransform == null)
                    Debug.LogError($"[{gameObject.name}] {GetType().Name}에 타겟 RectTransform이 존재하지 않음.", gameObject);
            }
        }

        protected virtual void OnEnable()
        {
            _enableTime = Time.unscaledTime;
            UpdateButtonState();
        }

        protected virtual void OnDisable()
        {
            IsHovering = false;
            IsPressed = false;
            currentState = ButtonState.Normal;
            Stop();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (!isInteractable) return;
            onClickEvent?.Invoke();
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (!isInteractable) return;
            IsPressed = true;

            // 사운드 재생 시 쿨다운 조건 추가
            if (usePressSound && SoundManager.Instance != null && CanPlaySound())
                SoundManager.Instance.PlaySfx(pressSound, pressSoundVolume);

            onPointerDownEvent?.Invoke();
            UpdateButtonState();
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (!isInteractable) return;
            IsHovering = true;

            if (useHoverSound && SoundManager.Instance != null && !IsPressed && CanPlaySound())
                SoundManager.Instance.PlaySfx(hoverSound, hoverSoundVolume);

            onPointerEnterEvent?.Invoke();
            UpdateButtonState();
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (!isInteractable) return;
            IsHovering = false;

            onPointerExitEvent?.Invoke();
            UpdateButtonState();
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (!isInteractable) return;
            IsPressed = false;

            onPointerUpEvent?.Invoke();
            UpdateButtonState();
        }

        protected bool CanPlaySound()
        {
            return Time.unscaledTime - _enableTime > 0.15f;
        }

        public virtual void Play()
        {
            if (!gameObject.activeInHierarchy) return;

            if (EffectCoroutine != null)
            {
                Stop(false);
                EffectCoroutine = null;
            }

            EffectCoroutine = StartCoroutine(ExecuteEffect());
        }

        public virtual void Stop(bool snapToEnd = true)
        {
            if (EffectCoroutine != null)
            {
                StopCoroutine(EffectCoroutine);
                EffectCoroutine = null;
            }
        }

        protected abstract IEnumerator ExecuteEffect();

        public ButtonState GetButtonState()
        {
            return currentState;
        }

        protected void UpdateButtonState()
        {
            ButtonState newState;

            if (!IsInteractable)
            {
                newState = ButtonState.Disabled;
                IsHovering = false;
                IsPressed = false;
            }
            else if (IsPressed)
            {
                newState = IsHovering ? ButtonState.Pressed : ButtonState.Normal;
            }
            else
            {
                newState = IsHovering ? ButtonState.Hover : ButtonState.Normal;
            }

            if (currentState != newState)
            {
                currentState = newState;
                Play();
            }
        }

        // 외부에서 이벤트를 통해 상태를 제어할 수 있는 래퍼 메서드
        public void SetInteractable(bool value)
        {
            IsInteractable = value;
        }

        protected enum AnimType
        {
            Normal,
            DoTween
        }
    }
}