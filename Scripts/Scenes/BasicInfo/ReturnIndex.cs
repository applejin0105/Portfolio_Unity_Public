using Components.Common.Buttons.Core;
using Scenes.BasicInfo.Controller;
using TMPro;
using UnityEngine;

namespace Scenes.BasicInfo
{
    [RequireComponent(typeof(CompoundButton), typeof(CanvasGroup))]
    public class ReturnIndex : MonoBehaviour
    {
        [SerializeField] private BasicInfoSceneUIController basicInfoSceneUIController;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI titleText;

        [Header("Button Identity")]
        [SerializeField] private Index myIndex; // String 대신 Enum(Index) 사용

        private CompoundButton _compoundButton;

        public Index MyIndex => myIndex;

        private void Awake()
        {
            _compoundButton = GetComponent<CompoundButton>();
        }

        private void Start()
        {
            if (myIndex == basicInfoSceneUIController.CurrentIndex)
                MakeVisible();
            else
                MakeInvisible();
        }

        private void OnEnable()
        {
            _compoundButton.onClickEvent.AddListener(SendClickEventToController);

            basicInfoSceneUIController.OnIndexChangedEvent += HandleIndexChanged;
        }

        private void OnDisable()
        {
            _compoundButton.onClickEvent.RemoveListener(SendClickEventToController);
            basicInfoSceneUIController.OnIndexChangedEvent -= HandleIndexChanged;
        }

        private void SendClickEventToController()
        {
            // 컨트롤러로 자신의 Index 전달
            basicInfoSceneUIController.OnClickIndexTitle(myIndex);
        }

        private void HandleIndexChanged(Index newIndex, Index oldIndex)
        {
            // 컨트롤러에서 브로드캐스트한 상태와 내 Index를 정수(Enum)로 빠르게 비교
            if (myIndex == newIndex)
                MakeVisible();
            else if (myIndex == oldIndex) MakeInvisible();
        }

        private void MakeVisible()
        {
            _compoundButton.IsInteractable = false;

            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }

        private void MakeInvisible()
        {
            _compoundButton.IsInteractable = true;

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }
}