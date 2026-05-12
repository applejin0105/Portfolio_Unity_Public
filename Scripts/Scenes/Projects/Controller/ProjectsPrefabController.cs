using System;
using Core.Attributes;
using Core.Managers;
using Scenes.Projects.Data;
using Scenes.Projects.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.Projects.Controller
{
    public class ProjectsPrefabController : MonoBehaviour
    {
        // Data 기반으로 프리팹 내부 연결해서 사용할 것들
        [SerializeField] private TextMeshProUGUI projectNumberTxt;
        [SerializeField] private TextMeshProUGUI projectNumberTxtShadow;

        [SerializeField] private TextMeshProUGUI projectTitle;

        [SerializeField] private Image projectImage;

        [SerializeField] private string mainTechStackPrefix;
        [SerializeField] private string mainStackTextColor;
        [SerializeField] private TextMeshProUGUI projectMainStackTxt;
        [SerializeField] private TextMeshProUGUI projectMainStackTxtShadow;

        [SerializeField] private TextMeshProUGUI summaryTxt;

        [Header("Link Buttons")]
        [SerializeField] private GameObject buttonContainer;

        [SerializeField] private SerializableDictionary<string, GameObject> buttonPrefab;

        private ProjectData _data;

        public static event Action<ProjectData, Image> OnPrefabClicked;

        public void Setup(ProjectData data, ProjectsPrefabManager manager)
        {
            if (data == null) return;

            _data = data;

            projectNumberTxt.text = data.id.ToString("D2");
            projectNumberTxtShadow.text = data.id.ToString("D2");
            projectTitle.text = data.title;
            projectMainStackTxt.text = $"{mainTechStackPrefix}{mainStackTextColor}{data.mainTechStack}";
            projectMainStackTxtShadow.text = $"{mainTechStackPrefix}{data.mainTechStack}";
            summaryTxt.text = data.summary;

            LoadProjectImage(data.imageSource, manager);
        }

        public void SetupPopup()
        {
            if (_data == null) return;

            OnPrefabClicked?.Invoke(_data, projectImage);
        }

        private void LoadProjectImage(string imageSource, ProjectsPrefabManager manager)
        {
            if (string.IsNullOrWhiteSpace(imageSource) || projectImage == null) return;

            var isLocalMode = GameSettingManager.GetDeviceMode();

            Action<Texture2D> onImageLoaded = tex =>
            {
                if (_data == null || _data.imageSource != imageSource) return;

                if (tex != null)
                {
                    var newSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f));
                    projectImage.sprite = newSprite;
                }
            };

            if (isLocalMode)
                manager.StartCoroutine(manager.LoadImageFromLocal(imageSource, onImageLoaded));
            else
                manager.StartCoroutine(manager.LoadImageFromServer(imageSource, onImageLoaded));
        }
    }
}