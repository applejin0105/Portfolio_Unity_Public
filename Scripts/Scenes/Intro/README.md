### Intro
> **Overview:** 프로젝트 진입 화면입니다.

- `IntroSceneManager`: 씬을 전체적으로 관리하는 매니저입니다. Intro Scene의 경우, Preload와 SingleLoad 판별전이라 오직 단일 씬으로 구성되어 있기에 BaseSceneManager를 상속받지 않습니다. 시작하면 PasswordWindow를 노출시킵니다.
- `PasswordManager`: 비밀번호를 입력하고, 다음으로 넘어갈 수 있는 PasswordManager입니다. 처음 제작 및 기획 당시, 홈페이지를 완성한 시점이었기에 당연히 서버까지 구현했습니다. 하지만 생각할수록 개인정보를 담고있는 유니티 프로젝트에 서버 통신까지 넣기에는 개발 경험과 지식이 부족하기도 하고, 개발 시간이 지나치게 오래 걸릴 것 같아 생략하였습니다. 하지만, 서버 모드의 시작과 데이터 로딩 로직은 일정부분 구현해두어 다른 프로젝트나 게임에서 서버 관련 로직을 작성할 때 공부가 되도록 준비했습니다.
- `IntroSceneUIController`: Intro Scene의 UI를 총괄적으로 관리하고 있습니다. 씬 진입 시 연출과 퇴장 시 연출을 앞서 설계한 EffectSequence를 통해 인스펙터에서 유연하게 처리가 가능하게 해두었습니다. 또한 메모리 최적화를 위해 `config`를 캐싱하여 재사용하는 방식으로 구현했습니다. 추후에는 이 모든걸 인스펙터에서 가능하게끔 Effect를 수정하고자 합니다.

## PasswordManager
- 구조적으로 유연하게 설계하는 것에 초점을 맞추었습니다. 기존에는 Server Mode로 베포하여 개인 리눅스 서버와 데이터를 연동하고자 했으나 기술 부족 및 시간 부족으로 진행하지 못하였습니다. 하지만, 서버로직 관련하여 기초는 닦아두었습니다.
  - 보안
    - 클라이언트의 Update 문에서 시간을 계산하지 않고 철저하게 서버의 응답(Response)에 의존하도록 구현했습니다.
    - 클라이언트가 자체 OS 시간을 조작하고, 비밀번호를 계속 뚫으려고 시도하는 것을 방지하기 위해, 실패 횟수 카운팅과 차단 로직은 클라이언트 메모리 베이스가 아닌 서버 DB에 기록하고, 클라이언트는 서버가 내려준 Unix Timestamp를 받아와 표시만 하게끔 설정하였습니다. 이는 기존의 React로 만든 홈페이지에도 동일하게 적용해두었습니다.
  - Rest API 통신 표준 준수
    - 게임 서버와 통신하기 위해 TCP 소캣 대신, 인증 단계에서는 REST API(HTTP POST) 방식을 채택했습니다. 직접 DB 커넥션을 맺는 것은 아무레도 SQL Injection과 같은 보안 취약점이 발생할 수 있으므로, 중간에 웹 서버(Linux/Nginx) 레이어를 두는 구조를 상정하고 UnityWebRequest를 사용했습니다.
      ```csharp
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
      ```
