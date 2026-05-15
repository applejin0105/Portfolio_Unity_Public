### Data
프로젝트 내부에서 사용하는 데이터들을 모아둔 곳입니다. 여러 enum들, json 파일들이 들어가있습니다.
특히 json 파일들을 일괄적으로 직렬화 및 데이터 파싱을 위해서 `DataManager.cs`를 제네릭 타입 클래스로 설계하였습니다. 이건 프로젝트의 PlayedGames와 Project Detail 요소들을 불러올 때, json 파일 기반으로 불러오면서 한번에 관리하기 위해 제작하였습니다.

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using Core.Attributes;
using Core.Data.Enums;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Core.Data
{
    // JSON의 최상위 구조(totalCount, data 배열)를 매핑하는 래퍼 클래스
    [Serializable]
    public class BaseResponse<T>
    {
        [JsonProperty("totalCount")]
        public int totalCount;

        [JsonProperty("data")]
        public List<T> data;
    }

    public class DataManager<T> : MonoBehaviour
    {
        public AppMode currentMode = AppMode.Local;

        [ShowIf("currentMode == Server")]
        [SerializeField] private string serverPath;

        // [핵심 변경 사항] string 경로 대신 TextAsset을 사용하여 인스펙터에서 직접 할당받습니다.
        [ShowIf("currentMode == Local")]
        [SerializeField] private TextAsset localJsonFile;

        public List<T> dataList = new();

        public int TotalCount { get; private set; }

        public int Length => dataList.Count;

        public void FetchDataLocal()
        {
            // 인스펙터에 파일이 잘 할당되었는지 확인합니다.
            if (localJsonFile != null)
            {
                // TextAsset에서 바로 JSON 문자열을 읽어옵니다.
                var jsonText = localJsonFile.text;

                // BaseResponse<T>를 사용하여 역직렬화
                var response = JsonConvert.DeserializeObject<BaseResponse<T>>(jsonText);

                if (response != null && response.data != null)
                {
                    TotalCount = response.totalCount;
                    dataList.AddRange(response.data);

                    Debug.Log($"[DataManager] 로컬 데이터 로드 성공: {localJsonFile.name} (총 {Length}개)");
                }
            }
            else
            {
                // 파일이 할당되지 않았을 때의 에러 처리
                Debug.LogError($"[DataManager] JSON 파일 누락: 인스펙터에 localJsonFile이 할당되지 않았습니다. ({gameObject.name})");
            }
        }

        public IEnumerator FetchDataServer(int page, int limit)
        {
            var url = $"{serverPath}?page={page}&limit={limit}";

            using (var uwr = UnityWebRequest.Get(url))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    var jsonResponse = uwr.downloadHandler.text;

                    // BaseResponse<T>를 사용하여 역직렬화
                    var response = JsonConvert.DeserializeObject<BaseResponse<T>>(jsonResponse);

                    if (response != null && response.data != null)
                    {
                        TotalCount = response.totalCount;
                        dataList.AddRange(response.data);
                    }
                }
                else
                {
                    Debug.LogError("[DataManager] 네트워크 오류: " + uwr.error);
                }
            }
        }
    }
}
```

이 데이터 로직을 통해서, 로컬 기반인지 서버 기반인지 구별합니다. 그리고 로컬이라면 json 파일을 바로 할당할 수 있게 되어있고, 내부 할당된 요소들은 json 파싱을 통해 Data List에 자동으로 들어가게 됩니다. 이를 통해 내부 json에 정보를 넣는 것 만으로도 얼마든지 프리팹을 동적 생성 할 수 있게 설계했습니다.

현재 PlayedGamesDataManager와 ProjectDataManager가 이를 상속받고 있으며, 각각 PlayedGamesDataManager와 ProjectsPrefabManager가 이를 할당받아 사용하여 내부 json 구조가 달라도 데이터를 정상적으로 파싱하여 받아오고 있습니다.
