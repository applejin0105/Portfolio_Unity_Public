using System;
using Scenes.Projects.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.Projects.Controller
{
    public class ProjectPopupWindowController : MonoBehaviour
    {
        [SerializeField] private Image detailImg;
        [SerializeField] private TextMeshProUGUI detailTitleTxt;
        [SerializeField] private string durationPrefix;
        [SerializeField] private TextMeshProUGUI detailDurationTxt;
        [SerializeField] private string departmentPrefix;
        [SerializeField] private TextMeshProUGUI detailDepartmentTxt;
        [SerializeField] private string stateTxtPrefix;
        [SerializeField] private TextMeshProUGUI detailStateTxt;
        [SerializeField] private TextMeshProUGUI detailTxt;

        private void OnEnable()
        {
            ProjectsPrefabController.OnPrefabClicked += SetupPopup;
        }

        private void OnDisable()
        {
            ProjectsPrefabController.OnPrefabClicked -= SetupPopup;
        }

        public static event Action OnPopupClicked;

        private void SetupPopup(ProjectData data, Image image)
        {
            if (data == null) return;

            Debug.Log("[ProjectPopupWindowController] Popup Set");

            detailTitleTxt.text = data.title;
            detailDurationTxt.text = $"{durationPrefix}{data.duration}";
            detailDepartmentTxt.text = $"{departmentPrefix}{data.department}";
            detailStateTxt.text = $"{stateTxtPrefix}{data.status}";

            detailTxt.text = data.description;

            OnPopupClicked?.Invoke();
        }
    }
}