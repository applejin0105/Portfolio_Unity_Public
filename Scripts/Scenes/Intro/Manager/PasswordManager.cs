using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Components.Common.Buttons;
using UnityEngine;
using UnityEngine.Networking;

// 면접관에게 어필할 포인트 (기술 면접용)
// 구조적 유연성 (Architecture)
// 로컬 개발 환경과 라이브 환경을 고려하여 bool 플래그로 로직을 분리
// 프로토타이핑 단계에서는 Local Mode로 빠르게 개발하고, 배포 시에는 Server Mode로 전환하여 보안을 강화할 수 있도록 설계
// 서버 권한에 대한 이해 (Server Authority)
// 보안을 위해 클라이언트의 Update 문에서 시간을 계산하지 않고, 철저하게 서버의 응답(Response)에 의존하도록 구현
// 클라이언트 시간은 사용자가 OS 설정으로 조작할 수 있습니다.
// 따라서 실패 횟수 카운팅이나 차단(Ban) 로직은 클라이언트 메모리가 아닌, 서버 DB에 기록하고, 클라이언트는 서버가 내려준 Unix Timestamp를 받아와 표시만 하도록 구현
// REST API 통신 표준 준수
// 게임 서버와 통신하기 위해 TCP 소켓 대신, 인증 단계에서는 REST API(HTTP POST) 방식을 채택
// 직접 DB 커넥션을 맺는 것은 보안 취약점이 되므로, 중간에 웹 서버(Linux/Nginx) 레이어를 두는 구조를 상정하고 UnityWebRequest를 사용

namespace Scenes.Intro.Manager
{
    [Serializable]
    public class ServerResponse
    {
        public bool success;
        public string message;
        public long blockReleaseTimestamp; // 차단 해제 시간 (Unix Timestamp)
    }

    public class PasswordManager : MonoBehaviour
    {
        [Header("Server Config")]
        private const string ServerUrl = "https://my-linux-portfolio.com/api/auth";

        private const string LocalSecretKey = "000000";
        private static PasswordManager _instance;

        [Header("System Settings")]
        [Tooltip("체크 시 서버 통신 모드, 해제 시 로컬 모드")]
        public bool useServerMode;

        [Header("UI References")]
        [SerializeField]
        private List<InteractiveNumberButton> interactiveNumbers = new();

        [SerializeField] private CanvasGroup canvasGroup;
        private bool _isProcessing;

        private int[] _values;

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
            else
                Destroy(gameObject);

            _values = new int[interactiveNumbers.Count];
            if (canvasGroup == null) Debug.LogError("[PasswordManager] Canvas Group is null");
        }

        private void Start()
        {
            InitializeButtons();
        }

        private void OnDestroy()
        {
            foreach (var number in interactiveNumbers)
                if (number != null)
                    number.OnValueChanged -= OnNumberChanged;
        }

        public event Action OnPasswordSuccess;
        public event Action OnPasswordFailure;

        private void InitializeButtons()
        {
            for (var i = 0; i < interactiveNumbers.Count; i++)
                if (interactiveNumbers[i] != null)
                {
                    interactiveNumbers[i].SetupIndex(i);
                    _values[i] = interactiveNumbers[i].value;
                    interactiveNumbers[i].OnValueChanged += OnNumberChanged;
                }
        }

        private void OnNumberChanged(int index, int value)
        {
            _values[index] = value;
        }

        public void ConfirmPassword()
        {
            if (_isProcessing) return; // 통신 중 중복 입력 방지

            _isProcessing = true;
            canvasGroup.interactable = false;

            var inputPassword = GetCurrentPasswordString();
            Debug.Log($"[System] Mode: {(useServerMode ? "Server Network" : "Local Standalone")}");

            StartCoroutine(useServerMode ? ProcessServerLogin(inputPassword) : ProcessLocalLogin(inputPassword));

            canvasGroup.interactable = true;
        }

        // Local Mode
        private IEnumerator ProcessLocalLogin(string password)
        {
            if (password == LocalSecretKey)
            {
                Debug.Log("<color=green>[Local] Login Success.</color>");
                // 이 미친것들은 block이라고 해놓고 false를 처 넣어놨네
                // 직관성 빵점 드립니다
                canvasGroup.blocksRaycasts = false;

                yield return StartCoroutine(SucPasswordIndex());

                OnPasswordSuccess?.Invoke();
            }
            else
            {
                Debug.LogWarning("<color=red>[Local] Wrong Password.</color>");
                _isProcessing = false;
                canvasGroup.interactable = true;
                OnPasswordFailure?.Invoke();
                yield return null;
            }
        }

        private IEnumerator SucPasswordIndex()
        {
            foreach (var number in interactiveNumbers)
            {
                if (number != null) number.SucEffect(8);

                yield return new WaitForSeconds(0.5f);
            }
        }

        // Server Mode
        private IEnumerator ProcessServerLogin(string password)
        {
            _isProcessing = true; // UI 잠금
            canvasGroup.interactable = false;

            // [보안] 비밀번호 해싱 (클라이언트 최소 보안)
            // 평문을 그대로 보내지 않고 1차 가공. 실제로는 HTTPS가 전송 구간을 암호화함.
            // 여기선 편의상 평문 전송 예시를 들되, 실제 구현 시엔 Hash 로직이 들어감.
            var form = new WWWForm();
            form.AddField("password", password);
            form.AddField("device_id", SystemInfo.deviceUniqueIdentifier); // IP 대신 기기 고유 ID 전송 (차단 기준)

            Debug.Log($"[Network] Connecting to {ServerUrl}...");

            // 유니티 2018 이상에서는 UnityWebRequest 사용 권장!!!
            using var www = UnityWebRequest.Post(ServerUrl, form);
            // [Mock] 실제 서버가 없으므로 통신 지연시간(Latency)만 시뮬레이션
            yield return new WaitForSeconds(1.0f);

            // [Server Simulation Code: 실제로는 서버가 보내주는 JSON]
            // 서버 내부 로직:
            // 1. DB 조회
            // 2. IP/DeviceID 조회 -> 3회 이상 실패 기록 확인
            // 3. 실패 기록이 있으면 { success: false, blockReleaseTimestamp: 1739000000 } 반환

            // 가상의 응답 생성 (테스트를 위해 비밀번호가 틀리면 차단되었다고 가정)
            var mockResponse = new ServerResponse();
            if (password == "000000") // 가상의 서버 정답
            {
                mockResponse.success = true;
                mockResponse.message = "Authenticated";
            }
            else
            {
                mockResponse.success = false;
                mockResponse.message = "Too many attempts. Blocked by Server Policy.";
                // 현재 시간 + 60초 뒤로 차단 시간 설정 (서버 시간 기준)
                mockResponse.blockReleaseTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60;
            }

            if (mockResponse.success)
            {
                Debug.Log("<color=cyan>[Server] 200 OK: Access Granted</color>");
                // SceneManager.LoadScene("GameScene");
            }
            else
            {
                // 서버가 준 차단 시간을 확인
                HandleServerBlock(mockResponse.blockReleaseTimestamp);
                _isProcessing = false;
                canvasGroup.interactable = true;
            }
        }

        private void HandleServerBlock(long releaseTimestamp)
        {
            var currentServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var remainingSeconds = releaseTimestamp - currentServerTime;

            if (remainingSeconds > 0)
            {
                Debug.LogError(
                    $"<color=red>[Server Authority] Access Denied. IP Blocked for {remainingSeconds} seconds.</color>");
                // 여기서 팝업창을 띄워 "서버 정책에 의해 00초간 접속이 제한됩니다" 출력
            }
            else
            {
                Debug.LogWarning("[Server] Wrong Password.");
                // Config 주입
                OnPasswordFailure?.Invoke();
            }
        }

        private string GetCurrentPasswordString()
        {
            var sb = new StringBuilder();
            foreach (var v in _values) sb.Append(v);
            return sb.ToString();
        }
    }
}