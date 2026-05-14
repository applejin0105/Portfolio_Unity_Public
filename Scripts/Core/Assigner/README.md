### Camera Canvas Assigner

**Description**
- **역할:** 프리팹으로 만들어진 카메라를 스크린 스페이스 카메라 렌더모드인 캔버스에 자동으로 할당합니다. Preload와 Single Load 방식 모두를 지원하기 위해 구현한 클래스입니다.
- **구조:**
  - `CanvasCameraAssigner`: 프리팹 형식으로 선언된 카메라를 캔버스에 붙여주는 클래스.
- **주요 로직:**
  - 프리팹으로 만들어진 카메라를 스크린 스페이스 카메라 렌더모드인 캔버스에 자동으로 할당합니다. GlobalManagerBootstarpper에서 메인 카메라를 찾아와 이를 카메라에 등록합니다. 만일 카메라가 이미 존재하는 경우라면 카메라 중복 생성을 방지하고, 월드에 존재하는 메인 카메라를 적용시킵니다.
- **특징 및 고려사항:**
  - 프래팹화하여 PreLoad 방식을 사용하기 위해 프리팹화 한 카메라를 위해 제작한 스크립트입니다. 언제나 16:9 비율을 유지하고, 초기에 화면 비율이 이상하여 카메라가 제대로 작동하지 않는 상황을 방지하기 위해 Canvas Scaler를 껏다 켜서 강제로 다시 계산하게 만들었습니다.
  - Start에서 사용할 경우, 스케일러가 초기화면 비율이 이상할 경우 고장나기 때문에 적용하였습니다.

**Core Code Snippet**
```csharp
    [RequireComponent(typeof(Canvas))]
    public class CanvasCameraAssigner : MonoBehaviour
    {
        // Start가 아닌 Awake에서 가장 먼저 처리해야 스케일러가 고장나지 않음!!!!
        private void Awake()
        {
            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;

            if (GlobalManagerBootstrapper.GlobalMainCamera != null)
            {
                canvas.worldCamera = GlobalManagerBootstrapper.GlobalMainCamera;
                canvas.planeDistance = 100f;

                // Canvas Scaler를 껐다 켜서 강제로 해상도를 다시 계산하게 만듦
                var scaler = GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.enabled = false;
                    scaler.enabled = true;
                }
            }
            else
            {
                canvas.worldCamera = Camera.main;
                if (canvas.worldCamera != null) canvas.planeDistance = 100f;
            }
        }
    }
```
