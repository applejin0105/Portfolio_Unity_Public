using Components.Common.Buttons.Core;
using Components.Effects.UI;
using Components.Effects.UI.Types;
using Core.Extensions;
using Scenes.Projects.Controller;
using UnityEngine;

namespace Scenes.Projects.Manager
{
    public class ProjectsDetailWindowManager : MonoBehaviour
    {
        // 여기서도 미리 연결만 해두고, 이벤트 형식으로 받아와야할듯.
        // 프리팹 자체 클릭하면 해당 정보 받아와서 볼 수 있게.
        // 관건은 서버 딜레이랑 text 로딩 딜레이 해결을 어떻게 하느냐인데.
        // 이건 고민해볼 가치가 있는 문제임
        // 해결함. 데이터 미리 받아오고 text 교체는 바로바로 가능함 개쩜 유니티는 걩쟝해애애애애앳

        [SerializeField] private CompoundButton clickablePanel;

        [Header("Effects")]
        [SerializeField] private UIFadeCanvasGroupEffect detailWindowPanelFadeCanvasGroupEffect;
        [SerializeField] private UIMoveEffect detailWindowMoveEffect;

        [SerializeField] private Vector2 moveStartPos;
        [SerializeField] private Vector2 moveEndPos;
        [SerializeField] private float moveInDuration = 1.0f;
        private MoveConfig _detailWindowMoveConfig;

        [Header("Configs")]
        private FadeCanvasGroupConfig _detailWindowPanelFadeCanvasGroupConfig;

        private void Awake()
        {
            InitToEnd();
        }

        private void OnEnable()
        {
            ProjectPopupWindowController.OnPopupClicked += ProjectsPrefabOnClick;

            if (clickablePanel != null)
            {
                Debug.Log("[ProjectsDetailWindowManager] MESSAGE: OnEnable");
                clickablePanel.onClickEvent.AddListener(OnClickPanelToClose);
            }
        }

        private void OnDisable()
        {
            ProjectPopupWindowController.OnPopupClicked -= ProjectsPrefabOnClick;

            if (clickablePanel != null)
            {
                Debug.Log("[ProjectsDetailWindowManager] MESSAGE: OnDisable");
                clickablePanel.onClickEvent.RemoveListener(OnClickPanelToClose);
            }
        }

        private void InitToStart()
        {
            _detailWindowMoveConfig = detailWindowMoveEffect.DefaultConfig;
            _detailWindowMoveConfig.position.start = moveStartPos;
            _detailWindowMoveConfig.position.end = moveEndPos;

            _detailWindowPanelFadeCanvasGroupConfig = detailWindowPanelFadeCanvasGroupEffect.DefaultConfig;
            _detailWindowPanelFadeCanvasGroupConfig.endAlpha = 0.3f;
            _detailWindowPanelFadeCanvasGroupConfig.blockRaycasts = false;

            detailWindowMoveEffect.SetProperty(_detailWindowMoveConfig, moveInDuration);
            detailWindowPanelFadeCanvasGroupEffect.SetProperty(_detailWindowPanelFadeCanvasGroupConfig, moveInDuration);
            clickablePanel.IsInteractable = true;
            clickablePanel.GetComponent<EmptyGraphic>().raycastTarget = true;
        }

        private void InitToEnd()
        {
            _detailWindowMoveConfig = detailWindowMoveEffect.DefaultConfig;
            _detailWindowMoveConfig.position.start = moveEndPos;
            _detailWindowMoveConfig.position.end = moveStartPos;

            _detailWindowPanelFadeCanvasGroupConfig = detailWindowPanelFadeCanvasGroupEffect.DefaultConfig;
            _detailWindowPanelFadeCanvasGroupConfig.endAlpha = 1.0f;
            _detailWindowPanelFadeCanvasGroupConfig.blockRaycasts = true;

            detailWindowMoveEffect.SetProperty(_detailWindowMoveConfig, moveInDuration);
            detailWindowPanelFadeCanvasGroupEffect.SetProperty(_detailWindowPanelFadeCanvasGroupConfig, moveInDuration);
            clickablePanel.IsInteractable = false;
            clickablePanel.GetComponent<EmptyGraphic>().raycastTarget = false;
        }

        private void ProjectsPrefabOnClick()
        {
            InitToStart();

            detailWindowPanelFadeCanvasGroupEffect.PlayOverrideEffect();
            detailWindowMoveEffect.PlayOverrideEffect();
        }

        private void OnClickPanelToClose()
        {
            Debug.Log("[ProjectsDetailWindowManager] MESSAGE: OnClickPanelToClose");
            InitToEnd();

            detailWindowPanelFadeCanvasGroupEffect.PlayOverrideEffect();
            detailWindowMoveEffect.PlayOverrideEffect();
        }
    }
}