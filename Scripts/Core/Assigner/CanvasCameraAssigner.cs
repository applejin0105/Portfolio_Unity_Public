using Core.Bootstrapper;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Assigner
{
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
}