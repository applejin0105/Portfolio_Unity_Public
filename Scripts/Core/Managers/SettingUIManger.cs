using System.Collections.Generic;
using Core.Data.Enums;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

namespace Core.Managers
{
    public class SettingUIManger : MonoBehaviour
    {
        [SerializeField] private GameObject raycastPanel;
        [SerializeField] private GameObject settingPanel;

        [SerializeField] private TMP_InputField bgmInputField;
        [Range(0, 100)] private int _bgmSoundVolume = 100;
        [SerializeField] private TMP_InputField sfxInputField;
        [Range(0, 100)] private int _sfxSoundVolume = 100;

        [SerializeField] private Scrollbar bgmScrollbar;
        [SerializeField] private Scrollbar sfxScrollbar;

        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private TMP_Dropdown windowTypeDropdown;

        [SerializeField] private TMP_Dropdown hzDropdown;

        private List<Resolution> _filteredResolutions = new List<Resolution>();
        private List<string> _filteredWindowTypes = new List<string> { "Full Screen", "Windowed" };

        private InputAction _escapeAction;
        private bool _isUpdatingVolumeUI = false;

        private bool _isOpen;

        private void Start()
        {
            InitializeResolutionDropdown();
            InitializeWindowTypeDropdown();
            InitializeHzDropdown();
        }

        private void Awake()
        {
            CloseSettingPanel();

            bgmScrollbar.onValueChanged.AddListener(OnBgmScrollbarChanged);
            bgmInputField.onEndEdit.AddListener(OnBgmInputChanged);

            sfxScrollbar.onValueChanged.AddListener(OnSfxScrollbarChanged);
            sfxInputField.onEndEdit.AddListener(OnSfxInputChanged);

            resolutionDropdown.onValueChanged.AddListener(ChangeResolution);
            windowTypeDropdown.onValueChanged.AddListener(ChangeWindowType);
            hzDropdown.onValueChanged.AddListener(ChangeHertz);

            _escapeAction = new InputAction(type: InputActionType.Button);
            _escapeAction.AddBinding("<Keyboard>/escape");
        }

        private void OnEnable()
        {
            _escapeAction.Enable();
            _escapeAction.performed += OnEscapePerformed;
        }

        private void OnDisable()
        {
            _escapeAction.Disable();
            _escapeAction.performed -= OnEscapePerformed;
        }

        private void OnEscapePerformed(InputAction.CallbackContext obj)
        {
            if (!_isOpen)
                OpenSettingPanel();
            else
                CloseSettingPanel();
        }

        private void OpenSettingPanel()
        {
            Time.timeScale = 0f;
            _isOpen = true;
            raycastPanel.SetActive(true);
            settingPanel.SetActive(true);
        }

        public void CloseSettingPanel()
        {
            Time.timeScale = 1f;
            _isOpen = false;
            raycastPanel.SetActive(false);
            settingPanel.SetActive(false);
        }

        private void OnBgmScrollbarChanged(float value)
        {
            if (_isUpdatingVolumeUI) return;

            _isUpdatingVolumeUI = true;

            _bgmSoundVolume = Mathf.RoundToInt(value * 100);
            bgmInputField.text = _bgmSoundVolume.ToString();

            if (SoundManager.Instance != null)
                SoundManager.Instance.SetBgmVolume(value);

            _isUpdatingVolumeUI = false;
        }

        private void OnBgmInputChanged(string text)
        {
            if (_isUpdatingVolumeUI) return;
            _isUpdatingVolumeUI = true;

            if (float.TryParse(text, out float result))
            {
                float clamp = Mathf.Clamp(result, 0f, 100f);

                _bgmSoundVolume = Mathf.RoundToInt(clamp);
                bgmInputField.text = _bgmSoundVolume.ToString();

                float normalizedVolume = clamp / 100f;
                bgmScrollbar.value = normalizedVolume;

                if (SoundManager.Instance != null)
                    SoundManager.Instance.SetBgmVolume(normalizedVolume);
            }

            _isUpdatingVolumeUI = false;
        }

        private void OnSfxInputChanged(string text)
        {
            if (_isUpdatingVolumeUI) return;
            _isUpdatingVolumeUI = true;

            if (float.TryParse(text, out float result))
            {
                float clamp = Mathf.Clamp(result, 0f, 100f);

                _sfxSoundVolume = Mathf.RoundToInt(clamp);
                sfxInputField.text = _sfxSoundVolume.ToString();

                float normalizedVolume = clamp / 100f;
                sfxScrollbar.value = normalizedVolume;

                if (SoundManager.Instance != null)
                    SoundManager.Instance.SetSfxVolume(normalizedVolume);
            }

            _isUpdatingVolumeUI = false;
        }

        private void OnSfxScrollbarChanged(float value)
        {
            if (_isUpdatingVolumeUI) return;

            _isUpdatingVolumeUI = true;

            _sfxSoundVolume = Mathf.RoundToInt(value * 100);
            sfxInputField.text = _sfxSoundVolume.ToString();

            if (SoundManager.Instance != null)
                SoundManager.Instance.SetSfxVolume(value);

            _isUpdatingVolumeUI = false;
        }

        private void InitializeResolutionDropdown()
        {
            GameSettingManager.InitializeResolutions();

            List<string> options = new List<string>();
            int currentResIndex = 0;

            for (int i = 0; i < GameSettingManager.AvailableResolutions.Count; i++)
            {
                var res = GameSettingManager.AvailableResolutions[i];
                options.Add($"{res.width} x {res.height}");

                if (res.width == Screen.width && res.height == Screen.height)
                    currentResIndex = i;
            }

            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResIndex;
            resolutionDropdown.RefreshShownValue();
        }

        private void InitializeWindowTypeDropdown()
        {
            windowTypeDropdown.ClearOptions();
            windowTypeDropdown.AddOptions(new List<string>(System.Enum.GetNames(typeof(ScreenType))));

            windowTypeDropdown.value = Screen.fullScreenMode == FullScreenMode.Windowed
                ? (int)ScreenType.Windowed
                : (int)ScreenType.FullScreen;
            windowTypeDropdown.RefreshShownValue();
        }

        private void InitializeHzDropdown()
        {
            hzDropdown.ClearOptions();

            List<string> hzOptions = new List<string>(System.Enum.GetNames(typeof(HertzType)));
            hzDropdown.AddOptions(hzOptions);

            int currentHzIndex = (int)GameSettingManager.CurrentHertzType;

            hzDropdown.value = currentHzIndex;
            hzDropdown.RefreshShownValue();
        }

        private void ChangeResolution(int index)
        {
            GameSettingManager.SetResolution(index);
        }

        private void ChangeWindowType(int index)
        {
            GameSettingManager.SetScreenType((ScreenType)index);
        }

        private void ChangeHertz(int index)
        {
            HertzType selectedHertz = (HertzType)index;
            GameSettingManager.SetHertz(selectedHertz);
        }

        public void ResetApplication()
        {
            Debug.Log("[Setting UI Manager] 시스템 하드 리셋 시작");

            Time.timeScale = 1f;

            EventManager.ClearAllEvents();

            Events.GlobalSceneEvents.ReleaseAll();

            if (SoundManager.Instance != null)
                SoundManager.Instance.DestroyInstance();

            if (SceneSlideManager.Instance != null)
                SceneSlideManager.Instance.DestroyInstance();

            System.GC.Collect();

            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        public void ExitApplication()
        {
            Application.Quit();
        }
    }
}