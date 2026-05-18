using System;
using System.Collections;
using System.Collections.Generic;
using Components.Common.Buttons.Core;
using Components.Effects.UI;
using Components.Effects.UI.Types;
using Scenes.Battle.Data;
using Scenes.Battle.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.Battle.UI
{
    [Serializable]
    public class CardUIGroup
    {
        [Header("Toggle Objects")]
        public GameObject topStarImage;
        public GameObject bottomSkillLayout;
        public GameObject frameImage;

        [Header("Skill Texts")]
        public TextMeshProUGUI[] basicValueTexts;
        public TextMeshProUGUI[] addValueTexts;

        public Image[] skillImages;
    }

    public class CardUI : MonoBehaviour
    {
        [Header("Core Components")]
        [SerializeField] private CompoundButton thisCardButton;
        [SerializeField] private GameObject cardBack;

        [Header("Common UI Elements")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Image profileImage;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI rarityText;

        [Header("Star Groups (Index 0=1성, 1=2성, 2=3성)")]
        [SerializeField] private CardUIGroup[] starGroups = new CardUIGroup[3];

        [Header("Effects")]
        [SerializeField] private UIHoverSwingEffect hoverEffect;

        private bool _isPurchased;
        private ShopManager _shopManager;
        public UnitData AssignedData { get; private set; }

        #region Initialization

        private void Awake()
        {
            if (thisCardButton == null)
                thisCardButton = GetComponent<CompoundButton>();
        }

        private void OnEnable()
        {
            if (thisCardButton != null)
            {
                thisCardButton.onClickEvent.AddListener(OnCardClicked);
                thisCardButton.onPointerEnterEvent.AddListener(OnEnter);
                thisCardButton.onPointerExitEvent.AddListener(OnExit);
            }
        }

        private void OnDisable()
        {
            if (thisCardButton != null)
            {
                thisCardButton.onClickEvent.RemoveListener(OnCardClicked);
                thisCardButton.onPointerEnterEvent.RemoveListener(OnEnter);
                thisCardButton.onPointerExitEvent.RemoveListener(OnExit);
            }
        }

        #endregion

        #region Data Binding

        public void SetupCard(UnitData data, ShopManager shopManager)
        {
            AssignedData = data;
            _shopManager = shopManager;

            if (AssignedData == null || AssignedData.star is < 1 or > 3) return;

            ResetCardState();

            nameText.text = AssignedData.unitName;
            costText.text = AssignedData.cost.ToString();
            rarityText.text = AssignedData.rarity.ToString();

            if (profileImage != null && AssignedData.profileSprite != null)
            {
                profileImage.sprite = AssignedData.profileSprite;
            }

            var targetIndex = AssignedData.star - 1;

            for (var i = 0; i < starGroups.Length; i++)
            {
                var isActiveGroup = i == targetIndex;

                starGroups[i].topStarImage.SetActive(isActiveGroup);
                starGroups[i].bottomSkillLayout.SetActive(isActiveGroup);
                starGroups[i].frameImage.SetActive(isActiveGroup);

                if (isActiveGroup) UpdateSkillTexts(starGroups[i], targetIndex);
            }
        }

        private void ResetCardState()
        {
            _isPurchased = false;
            thisCardButton.SetInteractable(true);

            if (cardBack != null) cardBack.SetActive(false);
            transform.rotation = Quaternion.identity;
        }

        private void UpdateSkillTexts(CardUIGroup activeGroup, int bonusIndex)
        {
            var currentSkillBonus = AssignedData.starBonuses[bonusIndex].addedSkillBonus;
            var skillCount = bonusIndex + 1;

            var activeSkills = new List<Skill> { AssignedData.skillStar1 };
            if (skillCount >= 2) activeSkills.Add(AssignedData.skillStar2);
            if (skillCount >= 3) activeSkills.Add(AssignedData.skillStar3);

            for (var j = 0; j < skillCount; j++)
            {
                if (j >= activeGroup.basicValueTexts.Length ||
                    j >= activeGroup.addValueTexts.Length ||
                    j >= activeGroup.skillImages.Length) continue;

                var baseSkill = activeSkills[j];

                // SO 데이터 직접 할당 (간결하고 효율적인 방식)
                var targetImage = activeGroup.skillImages[j];
                targetImage.sprite = baseSkill.skillIcon;
                // SO에 아이콘이 지정되어 있으면 skillColor를, 없으면 투명하게 처리
                targetImage.color = baseSkill.skillIcon != null ? baseSkill.skillColor : Color.clear;

                var totalBasicValue = baseSkill.basicValue + currentSkillBonus.addedBasicValue;
                activeGroup.basicValueTexts[j].text = totalBasicValue.ToString();

                if (baseSkill.coins != null && baseSkill.coins.Count > 0)
                {
                    if (baseSkill.sign)
                    {
                        var totalFrontValue = baseSkill.coins[0].frontValue + currentSkillBonus.addedFrontValue;
                        activeGroup.addValueTexts[j].text = "+" + totalFrontValue;
                    }
                    else
                    {
                        var totalBackValue = baseSkill.coins[0].backValue + currentSkillBonus.addedBackValue;
                        activeGroup.addValueTexts[j].text = totalBackValue.ToString();
                    }
                }
                else
                {
                    activeGroup.addValueTexts[j].text = "0";
                }
            }
        }

        #endregion

        #region Interaction & Animation

        public void PlayRerollEffect(UnitData newData, ShopManager shopManager)
        {
            StartCoroutine(RerollFlipCoroutine(newData, shopManager));
        }

        private IEnumerator RerollFlipCoroutine(UnitData newData, ShopManager shopManager)
        {
            var duration = 0.15f;
            var elapsedTime = 0f;

            // 1. 카드를 Y축 기준 90도 회전
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                var yRotation = Mathf.Lerp(0f, 90f, elapsedTime / duration);
                transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
                yield return null;
            }

            // 2. 90도 회전된(보이지 않는) 상태에서 새 데이터로 Setup 진행
            SetupCard(newData, shopManager);
            gameObject.SetActive(true);

            elapsedTime = 0f;

            // 3. 90도에서 0도로 복구하여 새로운 카드를 보여줌
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                var yRotation = Mathf.Lerp(90f, 0f, elapsedTime / duration);
                transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
                yield return null;
            }

            transform.rotation = Quaternion.identity;
        }

        private void OnCardClicked()
        {
            if (AssignedData == null || _shopManager == null || _isPurchased) return;
            _shopManager.TryBuyUnit(this, out var suc);
            if (suc)
                hoverEffect.Stop();
        }

        private void OnEnter()
        {
            hoverEffect.Play();
        }

        private void OnExit()
        {
            hoverEffect.Stop();
        }

        public void PlayPurchaseEffect()
        {
            _isPurchased = true;
            thisCardButton.SetInteractable(false);
            StartCoroutine(FlipCoroutine());
        }

        private IEnumerator FlipCoroutine()
        {
            var duration = 0.15f;
            var elapsedTime = 0f;

            // 카드를 Y축 기준 90도 회전
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                var yRotation = Mathf.Lerp(0f, 90f, elapsedTime / duration);
                transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
                yield return null;
            }

            // 2. 카드 뒷면 활성화
            if (cardBack != null) cardBack.SetActive(true);

            elapsedTime = 0f;

            // 90도에서 0도로 복구 (뒷면이 씌워진 채로 정면을 향함)
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                var yRotation = Mathf.Lerp(90f, 0f, elapsedTime / duration);
                transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
                yield return null;
            }

            transform.rotation = Quaternion.identity;
        }

        #endregion

        #region UI Management

        public int ReturnCardCost()
        {
            return AssignedData.cost;
        }

        #endregion
    }
}