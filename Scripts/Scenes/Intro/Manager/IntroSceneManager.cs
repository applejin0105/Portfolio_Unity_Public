using Scenes.Intro.Controller;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scenes.Intro.Manager
{
    public class IntroSceneManager : MonoBehaviour
    {
        private static IntroSceneManager _introSceneManager;

        [Header("References")]
        [SerializeField] private IntroSceneUIController introSceneUIController;

        public static IntroSceneManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            introSceneUIController.PlayPasswordWindowAppear();
        }

        public void LoadNextScene()
        {
            SceneManager.LoadScene("01_Warning");
        }
    }
}