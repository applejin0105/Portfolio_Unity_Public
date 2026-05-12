using System;
using System.Collections;
using System.IO;
using Components.Common.Buttons.Core;
using Components.Effects.UI.Types;
using Scenes.Battle.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scenes.Battle.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField] private BattleDirector battleDirector;

        [Header("UI Elements")]
        [SerializeField] private GameObject gameOverUIPanel;
        [SerializeField] private CanvasGroup blockRaycastPanel;

        [Header("Text Fields")]
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private TextMeshProUGUI deathText;
        [SerializeField] private TextMeshProUGUI killText;
        [SerializeField] private TextMeshProUGUI acquireText;
        [SerializeField] private TextMeshProUGUI spentText;

        [Header("Compound Buttons")]
        [SerializeField] private CompoundButton replayButton;
        [SerializeField] private CompoundButton screenshotButton;

        #region UI Rendering

        public void ShowResult(int round, int deadAllies, int killedEnemies, int acquiredCost, int spentCost)
        {
            if (gameOverUIPanel != null) gameOverUIPanel.SetActive(true);
            if (blockRaycastPanel != null) blockRaycastPanel.blocksRaycasts = true;

            if (roundText != null) roundText.text = $"{round}";
            if (deathText != null) deathText.text = $"{deadAllies}";
            if (killText != null) killText.text = $"{killedEnemies}";
            if (acquireText != null) acquireText.text = $"{acquiredCost}";
            if (spentText != null) spentText.text = $"{spentCost}";
        }

        #endregion

        #region Initialization

        private void OnEnable()
        {
            if (replayButton != null) replayButton.onClickEvent.AddListener(OnClickResetScene);
            if (screenshotButton != null) screenshotButton.onClickEvent.AddListener(OnClickScreenShot);
        }

        private void OnDisable()
        {
            if (replayButton != null) replayButton.onClickEvent.RemoveListener(OnClickResetScene);
            if (screenshotButton != null) screenshotButton.onClickEvent.RemoveListener(OnClickScreenShot);
        }

        #endregion

        #region Button Actions

        private void OnClickResetScene()
        {
            HardResetSystem();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void HardResetSystem()
        {
            if (battleDirector != null)
            {
                if (battleDirector.playerManager != null)
                    battleDirector.playerManager.ResetManager(); // PlayerManager 내부에 리스트 Clear 및 Cost 초기화 로직 구현 필요

                if (battleDirector.handManager != null)
                    battleDirector.handManager.ResetManager(); // 핸드 초기화

                if (battleDirector.fieldManager != null)
                    battleDirector.fieldManager.ResetField(); // 필드 데이터 초기화

                if (battleDirector.shopManager != null)
                    battleDirector.shopManager.ResetShop(); // 상점 및 핸드 슬롯 초기화
            }

            // TimeScale 초기화
            Time.timeScale = 1f;

            // 메모리 정리
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        private void OnClickScreenShot()
        {
            StartCoroutine(TakeScreenshot());
        }

        private IEnumerator TakeScreenshot()
        {
            yield return new WaitForEndOfFrame();

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = "Screenshot_" + timestamp + ".png";

#if UNITY_WEBGL && !UNITY_EDITOR
            ScreenCapture.CaptureScreenshot(fileName);
            Debug.Log("WebGL 스크린샷 다운로드 요청: " + fileName);
#else
            var savePath = Path.Combine(Application.persistentDataPath, fileName);
            ScreenCapture.CaptureScreenshot(savePath);
            Debug.Log("스크린샷 저장 경로: " + savePath);
#endif
        }

        #endregion
    }
}