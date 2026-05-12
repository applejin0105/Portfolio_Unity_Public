using Scenes.Warning.Controller;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scenes.Warning.Manager
{
    public class WarningSceneManager : MonoBehaviour
    {
        private static WarningSceneManager _warningSceneManager;

        [Header("References")]
        [SerializeField] private WarningSceneUIController warningSceneUIController;
        public static WarningSceneManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            warningSceneUIController.PlayWarningSceneAppear();
        }

        public void LoadNextScene()
        {
            SceneManager.LoadScene("02_Loading");
        }
    }
}