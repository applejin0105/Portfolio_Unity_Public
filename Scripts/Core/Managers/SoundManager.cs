using System;
using System.Collections;
using System.Collections.Generic;
using Core.Data.Enums;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace Core.Managers
{
    public class SoundManager : MonoBehaviour
    {
        public float fadeDuration = 1.0f;

        public List<BGMSoundData> bgmList;
        public List<UISoundData> sfxSoundList;
        public List<BattleSoundData> battleSoundList;

        [Header("Audio Mixer")]
        public AudioMixer mainMixer;

        [Header("Audio Sources")]
        [SerializeField] public AudioSource bgmSource;
        [SerializeField] public AudioSource[] sfxSources;
        [SerializeField] public AudioSource[] battleSoundSources;

        private int _sfxSourceIndex = 0;
        private int _battleSoundSourceIndex = 0;

        [Header("Audio Data")]
        public AudioClip[] bgmClips;
        public AudioClip[] sfxClips;
        public AudioClip[] battleSoundClips;

        private struct BgmQueueItem
        {
            public AudioClip clip;
            public float volume;
        }

        [Header("BGM Queue System")]
        private readonly List<BgmQueueItem> _bgmQueue = new();

        private Dictionary<BGMSoundType, AudioClip> _bgmDict;
        private Dictionary<SfxSoundType, AudioClip> _sfxDict;
        private Dictionary<BattleSoundType, AudioClip> _battleSoundDict;

        private Coroutine _currentFadeRoutine;
        private int _currentQueueIndex = -1;
        private bool _isPlayingQueue;
        private bool _isShuffleQueue;

        private Coroutine _queueRoutine;

        private float _lastTypingSoundTime;

        // 사운드 작업 중 이러한 생각이 들었다.
        // 보통 유니티 게임 여러개를 하다 보면, 사운드를 헤드셋이나 스피커로 바꿀때 소리가 재생되지 않는 경우가 있다.
        // 그럼 그 부분도 해결하면 좋겠고, 무엇보다 만일 신 전환에서 알 수 없는 오류로 사운드(Dont Destroy On Load)임에도 사라지면
        // 버그가 날것같다고 생각했다. 물론, 그럴 경우는 거의 없겠지만, 방어 코드를 한번 짜보고 싶었다.
        // 잼미니 피셜로 다음과 같은 상황이 있을 수 있다고 한다.
        /*
         * 1. SoundManager를 특정 오브젝트에 자식으로 넣는 경우 -> DontDestroyOnLoad는 최상위 부모에게만 적용되므로
         * 2. 에디터의 Hot Reload 이슈 -> 스크립트 로드 과정에서 발생. 하지만 사실 에디터만의 문제라 상관은 없지만, 빌드시에는 상관 없음. 그래도 개발중엔 좀 불편할 수 있음.
         * 3. 싱글톤 중복 제거 로직 이슈 -> 지금부터 서로 죽여라
         * 가령 if (Instance != null) Destroy(gameObject); 이면 -> 어 죽을게 하면서 스스로 반으로 갈라져 죽음 여기까진 좋음.
         * 하지만 뒤에 실(?)수로 Instance = this; 그리고 DontDestroyOnLoad(gameObject); 이렇게 선언해 버리면 새로 생긴놈이 들어가버림
         * 기존에 살아있던, 죽으면 안됐던(눈물) 매니저는 참조를 잃고, 모든걸 잃고 양산형 복수물을 찍으러 GC에서 돌아와 나를 도륙내버릴 수 있음.
         * 그러면 결국 새로 생긴(망가진건 싫어!) 매니저는 조건문에서 걸려서 자폭하고 결국 내 코드는 모든걸 잃고 나락으로 빠짐.
         * 4. 게임 재시작 등의 기능을 만들때, 씬에 있는 오브젝트를 싹 다 밀어버리는 경우. 예외처리를 안해버리면 쭈인님 나 주거요 하면서 사라짐.
         * 5. Scene Unloading 타이밍 이슈
         *  아주아주 重 씬을 비동기(LoadSceneAsync)로 로드하면서 동시에 현재 씬을 날려버리는... 뭔 개소린지 모르겠어서 다시 질문했음.
         *  쉽게(?) 말해 여권 도장을 찍기도 전에 공항이 폐쇄되는 상황이라고 함.
         *  DontDestroyOnLoad(this)는 Awake에 보통 적음. 그럼 당연히 게임 시작하면 Awake부터 실행하니깐 DontDestroyOnLoad 실행되면서 '나는 이 신(not god, Scene)을 초월하겠다 유니티!'가 되어버림.
         *  그러면 신이 파괴되어도 유니티의 붉은 돌의 힘으로 초월적인 존재가 된 게임오브젝트가 되어 살아남음.
         *  그런데 유니티는 모든 오브젝트의 Awake()를 동시에 실행하지 않음(유니티 이 나쁜자식아! 그러지마!)
         *  유니티가 때가되어(신이 로딩될때) 사도(Awake)들을 하나씩 깨울때 하필이면 SceneLoader를 먼저 깨워버림. 서순이 이래서 중요함. 내가 그래서 하스를 못함.
         *  그러면 SceneLoader가 천천히 일어나(사실 1프레임 내로 매우 빨리 일어남) 베헤리트(SceneManager.LoadScene)를 호출해버리고, '바친다'라며 이전 씬의 모든 것들에게 제물의 낙인을 찍어버림.
         *  그럼 이제 마(Garbage Collector aka GC)의 제물이 된 SoundManager는 DontDestroyOnLoad를 외치지만, 절대자(유니티)는 '늦었다'라며 Awake 실행 도중에 씬 언로딩 프로세스가 덮쳐서 메모리에서 날려버림.
         *  이게 그럼 유니티가 빡통이라서 햇갈리는가? 아니다. LoadScene은 보통 동기적으로 작동해서 그 프레임에 바로 정리 작업이 들억마. 그리고 스크립트 실행 순서를 개발자가 정해주지 않는 이상, '동시에' 실행이라는건 사실 다 구라고 컴퓨터에서는 구현이 불가능하므로
         *  어떤 컴퓨터에서는 It Works! 가 되는거고 어떤 컴퓨터에서는 Why?가 되는거임. (제미나이 피셜, 이게 가장 무서운 버그라고 함. 허접)
         *
         *  그럼 이걸 도대체 어떻게 해결하면 좋을까?
         *  이미 답은 나왔다. 바로 Script Execution Order를 설정해서, 에디터 설정에서 순서를 정해준다.
         *  Edit -> Project Settings -> Script Execution Order 선택.
         *  + 버튼을 눌러 SoundManager 스크립트를 추가.
         *  시간을 **Default Time (0)**보다 빠른 -100 정도로 설정.
         *  Apply
         *  이러면 씬이 시작되자마자 으허허 오징어 덮밥 하면서 SoundManager가 DontDestroyOnLoad를 시작해서 안죽음.
         *
         *  또다른 방법도 있다. (제미나이 피셜 현업 스타일이라는데 신뢰성은 좀 떨어짐)
         *  게임의 첫 시작 씬(Scene 0)에는 아무것도 없고 오직 SystemManager, SoundManager 같은 관리자들만 배치합니다.
         *  게임 시작 -> ManagerScene 로드.
         *  매니저들이 안전하게 생성되고 초기화됨.
         *  매니저가 초기화 끝난 뒤에 GameScene을 로드함.
         *  이라는데 현직자가 아니라서 잘 모르겠음. 좋은 방법인것 같기도 하고.
         *
         *  물론 이런 방법이 있긴 하지만, 그래도 방어적으로 코딩하기를 좋아하므로 Lazy Initialization도 적용해서 매니저를 예토전생 시키는 역할도 수행하게 만들 생각임.
         *  물론, 위 방법도 적용할 생각임. 우효
         */
        public static SoundManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    // 씬에 이미 있는지 확인
                    _instance = FindFirstObjectByType<SoundManager>();

                    // 없으면 새로 생성 (예토전생 로직)
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject("SoundManager");
                        _instance = obj.AddComponent<SoundManager>();
                    }
                }

                return _instance;
            }
        }

        private static SoundManager _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);

                _bgmDict = new Dictionary<BGMSoundType, AudioClip>();
                _sfxDict = new Dictionary<SfxSoundType, AudioClip>();
                _battleSoundDict = new Dictionary<BattleSoundType, AudioClip>();
                InitializeDictionaries();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            AudioSettings.OnAudioConfigurationChanged += OnAudioConfigChanged;
        }

        private void OnAudioConfigChanged(bool deviceWasChanged)
        {
            if (deviceWasChanged)
            {
                StartCoroutine(ResetAudioAndResumeRoutine());
            }
        }

        private IEnumerator ResetAudioAndResumeRoutine()
        {
            bool wasBgmPlaying = bgmSource.isPlaying;
            float currentPlaybackTime = bgmSource.time;

            AudioSettings.Reset(AudioSettings.GetConfiguration());

            yield return null;
            yield return null;

            if (wasBgmPlaying && bgmSource.clip != null)
            {
                bgmSource.Play();
                bgmSource.time = currentPlaybackTime; // 처음부터가 아니라 끊긴 부분부터 자연스럽게 이어가기
                Debug.Log("오디오 재설정 완료 및 BGM 복구 성공.");
            }
        }

        private void InitializeDictionaries()
        {
            foreach (var data in bgmList)
                if (!_bgmDict.ContainsKey(data.type))
                    _bgmDict.Add(data.type, data.clip);

            foreach (var data in sfxSoundList)
                if (!_sfxDict.ContainsKey(data.type))
                    _sfxDict.Add(data.type, data.clip);

            foreach (var data in battleSoundList)
                if (!_battleSoundDict.ContainsKey(data.type))
                    _battleSoundDict.Add(data.type, data.clip);
        }

        public void PlayBattleSound(BattleSoundType sfx, float volume = 1.0f)
        {
            if (!GetBattleSound(sfx, out var clip)) return;

            // 라운드 로빈 방식으로 소스 가져오기
            AudioSource source = battleSoundSources[_battleSoundSourceIndex];

            source.PlayOneShot(clip, volume);

            // 다음 인덱스로 이동
            _battleSoundSourceIndex = (_battleSoundSourceIndex + 1) % battleSoundSources.Length;
        }

        public void PlaySfx(SfxSoundType sfx, float volume = 1.0f)
        {
            if (!GetSfx(sfx, out var clip)) return;

            // 라운드 로빈 방식으로 소스 가져오기
            AudioSource source = sfxSources[_sfxSourceIndex];

            source.PlayOneShot(clip, volume);

            // 다음 인덱스로 이동
            _sfxSourceIndex = (_sfxSourceIndex + 1) % sfxSources.Length;
        }

        public void PlayBgm(BGMSoundType bgm, float volume = 1.0f, bool instantFadeOut = false)
        {
            StopQueue();
            if (!GetBgm(bgm, out var clip)) return;
            if (bgmSource.clip == clip) return;
            ChangeBgm(clip, volume, instantFadeOut);
        }

        private void ChangeBgm(AudioClip nextClip, float targetVolume, bool instantFadeOut = false)
        {
            if (_currentFadeRoutine != null) StopCoroutine(_currentFadeRoutine);
            _currentFadeRoutine = StartCoroutine(FadeBgmRoutine(nextClip, fadeDuration, targetVolume, instantFadeOut));
        }

        public bool GetBattleSound(BattleSoundType sfx, out AudioClip returnClip)
        {
            if (_battleSoundDict.TryGetValue(sfx, out var value))
            {
                returnClip = value;
                return true;
            }

            Debug.LogWarning($"BattleSound Missing: {gameObject} {sfx}");
            returnClip = null;
            return false;
        }

        public bool GetSfx(SfxSoundType sfx, out AudioClip returnClip)
        {
            if (_sfxDict.TryGetValue(sfx, out var value))
            {
                returnClip = value;
                return true;
            }

            Debug.LogWarning($"SFX Missing: {gameObject} {sfx}");
            returnClip = null;
            return false;
        }

        public bool GetBgm(BGMSoundType bgm, out AudioClip returnClip)
        {
            if (_bgmDict.TryGetValue(bgm, out var value))
            {
                returnClip = value;
                return true;
            }

            Debug.LogWarning($"Bgm Missing: {bgm}");
            returnClip = null;
            return false;
        }

        public void StopBattleSound()
        {
            foreach (var source in battleSoundSources)
            {
                source.Stop();
            }
        }

        public void StopSfx()
        {
            foreach (var source in sfxSources)
            {
                source.Stop();
            }
        }

        public void StopBgm()
        {
            bgmSource.Stop();
        }

        private void ChangeBgm(AudioClip nextClip, float targetVolume)
        {
            // 자연스럽게 넘어가야함
            // 기존 음악 페이드 아웃, 현재 음악 페이드인
            // 여기에서 아래 구현한 SetVolume을 사용하지 않는 이유?
            // AudioMixer는 사용자의 환경 설정을 반영하는 용도임. 이걸 건드리면 결국
            // 기존 볼륨도 기억해야하고, 복구하는 로직을 추가해야하는데 굳이? 임. 비효율적임.
            // 현재 재생되는 리로스 자체의 소리 크기를 지우고, 제어하고, 원래 볼륨으로 돌아오는게
            // 직관적이고 더 유리함.
            // 최적화 측면에서도 프레임마다 호출할때 (결국 IEnumerator를 사용해야하니깐)
            // Mixer.SetFloat보다  AudioSource.volume의 float 변수를 바꾸는게 더 가벼움. (물론 최신기기에서는 크게 상관 없음)

            if (_currentFadeRoutine != null) StopCoroutine(_currentFadeRoutine);

            _currentFadeRoutine = StartCoroutine(FadeBgmRoutine(nextClip, fadeDuration, targetVolume, false));
        }

        private IEnumerator FadeBgmRoutine(AudioClip nextClip, float duration, float targetVolume, bool instantFadeOut)
        {
            if (bgmSource.isPlaying)
            {
                // instantFadeOut이 true면 기존 BGM이 서서히 꺼지는 것을 기다리지 않고 즉시 볼륨을 0으로
                if (instantFadeOut)
                {
                    bgmSource.volume = 0f;
                }
                else
                {
                    var startVolume = bgmSource.volume;
                    var time = 0f;

                    while (time < duration)
                    {
                        time += Time.deltaTime;
                        bgmSource.volume = Mathf.Lerp(startVolume, 0f, time / duration);
                        yield return null;
                    }
                }
            }

            bgmSource.volume = 0f;
            bgmSource.Stop();

            bgmSource.clip = nextClip;
            bgmSource.loop = true;
            bgmSource.Play();

            var fadeTime = 0f;

            while (fadeTime < duration)
            {
                fadeTime += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(0f, targetVolume, fadeTime / duration);
                yield return null;
            }

            bgmSource.volume = targetVolume;
            _currentFadeRoutine = null;
        }

        public void SetBgmVolume(float volume)
        {
            /*   사람이 듣기 자연스러운 소리 변화를 위해 Log를 사용.
             *  전력 기준: 10 * log
             *  진폭 기준: 10 * log
             *  슬라이더 1.0(100%): log(1) = 0.0 * 20 = 0dB (기본)
             *  슬라이더 0.1(10%): log(0.1) = -1.0 * 20 = -20dB
             *  슬라이더 0.01(1%): log(0.01) = -2.0 * 20 = -40dB (기본)
             *  즉, 로그를 통해 데시벨 단위로 스케일링을 진행
             *  여기에서 (volume <= 0.001f) 예외처리를 하는 이유는 버그 막는 것. log(0)은 존재하지 않으므로 작으면 음소거 처리
             *  즉, dB = 20 * log{10}(슬라이더 값)
             */
            // 
            var db = volume <= 0.001f ? -80f : Mathf.Log10(volume) * 20f;
            mainMixer.SetFloat("BGM_Volume", db);
        }

        public void SetSfxVolume(float volume)
        {
            var db = volume <= 0.001f ? -80f : Mathf.Log10(volume) * 20f;
            mainMixer.SetFloat("SFX_Volume", db);
        }

        public void SetBattleSoundVolume(float volume)
        {
            var db = volume <= 0.001f ? -80f : Mathf.Log10(volume) * 20f;
            mainMixer.SetFloat("BattleSound_Volume", db);
        }

        public float GetBattleSoundDuration(BattleSoundType battleSound)
        {
            GetBattleSound(battleSound, out var clip);
            return clip.length;
        }

        public float GetSfxDuration(SfxSoundType sfx)
        {
            GetSfx(sfx, out var clip);
            return clip.length;
        }

        public float GetBGMDuration(BGMSoundType bgm)
        {
            GetBgm(bgm, out var clip);
            return clip.length;
        }

        [Serializable]
        public struct UISoundData
        {
            public SfxSoundType type;
            public AudioClip clip;
        }

        [Serializable]
        public struct BGMSoundData
        {
            public BGMSoundType type;
            public AudioClip clip;
        }

        [Serializable]
        public struct BattleSoundData
        {
            public BattleSoundType type;
            public AudioClip clip;
        }

        #region Queue Management

        public void AddBgmToQueue(BGMSoundType bgm, float volume = 1.0f)
        {
            if (GetBgm(bgm, out var clip))
            {
                // 중복 추가 방지
                if (!_bgmQueue.Exists(item => item.clip == clip))
                {
                    _bgmQueue.Add(new BgmQueueItem { clip = clip, volume = volume });
                }
            }
        }

        public void ClearQueue()
        {
            _bgmQueue.Clear();
            StopQueue();
        }

        public void PlayQueue(bool shuffle = false) // 전역 volume 파라미터 제거
        {
            if (_bgmQueue.Count == 0)
            {
                Debug.LogWarning("[SoundManager] 큐에 재생할 BGM이 없습니다.");
                return;
            }

            _isShuffleQueue = shuffle;
            _isPlayingQueue = true;

            _currentQueueIndex = _isShuffleQueue ? Random.Range(0, _bgmQueue.Count) : 0;

            if (_queueRoutine != null) StopCoroutine(_queueRoutine);
            _queueRoutine = StartCoroutine(ProcessQueueRoutine());
        }

        public void StopQueue()
        {
            _isPlayingQueue = false;
            if (_queueRoutine != null)
            {
                StopCoroutine(_queueRoutine);
                _queueRoutine = null;
            }
        }

        public void ToggleShuffle(bool isShuffle)
        {
            _isShuffleQueue = isShuffle;
        }

        private IEnumerator ProcessQueueRoutine()
        {
            while (_isPlayingQueue && _bgmQueue.Count > 0)
            {
                var currentItem = _bgmQueue[_currentQueueIndex];

                // 큐에 저장된 개별 볼륨 적용
                ChangeBgm(currentItem.clip, currentItem.volume);

                var waitTime = currentItem.clip.length - fadeDuration;
                if (waitTime <= 0) waitTime = currentItem.clip.length;

                yield return new WaitForSeconds(waitTime);

                if (_isShuffleQueue && _bgmQueue.Count > 1)
                {
                    var nextIndex = _currentQueueIndex;
                    while (nextIndex == _currentQueueIndex) nextIndex = Random.Range(0, _bgmQueue.Count);
                    _currentQueueIndex = nextIndex;
                }
                else
                {
                    _currentQueueIndex = (_currentQueueIndex + 1) % _bgmQueue.Count;
                }
            }
        }

        #endregion

        public void DestroyInstance()
        {
            // 진행 중인 모든 사운드 페이드, 큐 코루틴 정지
            StopAllCoroutines();
            StopBgm();
            StopSfx();
            _bgmQueue.Clear();

            // 싱글톤 자리 비우기
            _instance = null;

            // 나 자신 파괴
            Destroy(gameObject);
        }
    }
}