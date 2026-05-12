using System;
using System.Collections.Generic;
using Scenes.Battle.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.Battle.UI
{
    [Serializable]
    public class ToolTipUIGroup
    {
        public GameObject toolTipSkillLayout;

        public TextMeshProUGUI[] basicValueTexts;
        public TextMeshProUGUI[] addValueTexts;

        public Image[] skillImages;
    }

    public class ToolTipUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform tooltipPanel;
        [SerializeField] private TextMeshProUGUI rarityText;

        [Header("Star Groups (Index 0=1성, 1=2성, 2=3성)")]
        [SerializeField] private ToolTipUIGroup[] starGroups = new ToolTipUIGroup[3];

        [Header("Settings")]
        [SerializeField] private Vector2 offset;

        private UnitData _currentDisplayedData;
        private Camera _uiCamera;

        private void Awake()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                _uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            }
            else
            {
                _uiCamera = Camera.main;
            }

            HideTooltip();
        }

        public void SetTooltipData(UnitData data)
        {
            if (data == null || data.star is < 1 or > 3) return;

            if (_currentDisplayedData == data)
            {
                tooltipPanel.gameObject.SetActive(true);
                return;
            }

            _currentDisplayedData = data;
            var targetIndex = data.star - 1;

            rarityText.text = data.rarity.ToString();

            for (var i = 0; i < starGroups.Length; i++) starGroups[i].toolTipSkillLayout.SetActive(i == targetIndex);

            var currentSkillBonus = data.starBonuses[targetIndex].addedSkillBonus;
            var skillCount = targetIndex + 1;

            var activeSkills = new List<Skill> { data.skillStar1 };
            if (skillCount >= 2) activeSkills.Add(data.skillStar2);
            if (skillCount >= 3) activeSkills.Add(data.skillStar3);

            for (var j = 0; j < skillCount; j++)
            {
                if (j >= starGroups[targetIndex].basicValueTexts.Length ||
                    j >= starGroups[targetIndex].addValueTexts.Length ||
                    j >= starGroups[targetIndex].skillImages.Length) continue;

                var baseSkill = activeSkills[j];

                // SO 데이터 직접 할당 (간결하고 효율적인 방식)
                var targetImage = starGroups[targetIndex].skillImages[j];
                targetImage.sprite = baseSkill.skillIcon;
                // SO에 아이콘이 지정되어 있으면 skillColor를, 없으면 투명하게 처리
                targetImage.color = baseSkill.skillIcon != null ? baseSkill.skillColor : Color.clear;

                var totalBasicValue = baseSkill.basicValue + currentSkillBonus.addedBasicValue;
                starGroups[targetIndex].basicValueTexts[j].text = totalBasicValue.ToString();

                if (baseSkill.coins is { Count: > 0 })
                {
                    if (baseSkill.sign)
                    {
                        var totalFrontValue = baseSkill.coins[0].frontValue + currentSkillBonus.addedFrontValue;
                        starGroups[targetIndex].addValueTexts[j].text = "+" + totalFrontValue;
                    }
                    else
                    {
                        var totalBackValue = baseSkill.coins[0].backValue + currentSkillBonus.addedBackValue;
                        starGroups[targetIndex].addValueTexts[j].text = totalBackValue.ToString();
                    }
                }
                else
                {
                    starGroups[targetIndex].addValueTexts[j].text = "0";
                }
            }

            tooltipPanel.gameObject.SetActive(true);
        }

        public void UpdatePosition(Vector2 mousePos)
        {
            var parentRect = tooltipPanel.parent as RectTransform;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, mousePos, _uiCamera,
                    out var localPoint))
                tooltipPanel.localPosition = localPoint + offset;
        }

        public void HideTooltip()
        {
            _currentDisplayedData = null;
            tooltipPanel.gameObject.SetActive(false);
        }
    }
}