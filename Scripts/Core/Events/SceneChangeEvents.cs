using Core.Data.Models;
using Core.Managers;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Core.Events
{
    public static class SceneChangeEvents
    {
        public static void GoToScene(string nextSceneName)
        {
            SceneTransitData.TargetSceneName = nextSceneName;

            if (GameSettingManager.GetDeviceMode())
                SceneManager.LoadScene("02_Loading");
            else
                // await과 Forget의 차이
                // await -> 끝날 때 까지 대기.
                // forget -> 실행 지시 후 다음 코드로 넘어가기
                // 나는 씬 로딩할때 연출 키고 끄는걸 다 브로드캐스팅으로 구현했음
                // 이벤트 기반 방식이므로 뭘 기다리고 자시고 할 이유가 없음
                SceneSlideManager.Instance.ExecuteSlideAsync(nextSceneName).Forget();
        }
    }
}