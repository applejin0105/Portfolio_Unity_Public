using System;
using System.Collections;
using System.Linq;
using Scenes.Battle.Data;
using Scenes.Battle.Entity;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scenes.Battle.Core
{
    // 스킬 코인의 상태 조회·파괴·토스를 담당하는 순수 헬퍼.
    // 상태를 갖지 않으므로 static. 전투 흐름 제어는 BattleManager가 담당.
    public static class CoinResolver
    {
        private static readonly float MaxSp = 45.0f;
        private static readonly float MinSp = -45.0f;

        private static readonly float MaxProbability = 0.95f;
        private static readonly float MinProbability = 0.05f;

        public static int GetActiveCoinCount(ActionSlot slot)
        {
            return slot.SelectedSkill.coins?.Count(c => !c.isBroken) ?? 0;
        }

        public static void BreakSlotCoin(ActionSlot slot)
        {
            var coins = slot.SelectedSkill.coins;
            if (coins == null) return;
            for (var i = coins.Count - 1; i >= 0; i--)
            {
                if (!coins[i].isBroken)
                {
                    var coin = coins[i];
                    coin.isBroken = true;
                    coins[i] = coin;

                    var skill = slot.SelectedSkill;
                    skill.coins = coins;
                    slot.SelectedSkill = skill;

                    slot.Owner.UpdateCoinUI(i, Character.CoinUIState.Broken);
                    slot.Owner.PlayCharacterSfx(slot.Owner.CoinBreakSound);
                    break;
                }
            }
        }

        public static int TossSingleCoin(ActionSlot slot, int coinIndex)
        {
            var coin = slot.SelectedSkill.coins[coinIndex];
            var prob = CalculateSlotProbability(slot.Owner.Sp);
            bool isFront = Random.value < prob;

            var uiState = isFront ? Character.CoinUIState.Front : Character.CoinUIState.Back;
            slot.Owner.UpdateCoinUI(coinIndex, uiState);
            slot.Owner.PlayCharacterSfx(coin.coinTossSound);

            return isFront ? coin.frontValue : coin.backValue;
        }

        public static IEnumerator TossSlotCoinsRoutine(ActionSlot slot, float totalDuration, Action<int> onComplete,
            bool isFocus)
        {
            var skill = slot.SelectedSkill;
            if (skill.coins == null)
            {
                onComplete?.Invoke(skill.basicValue);
                yield break;
            }

            int activeCoinCount = GetActiveCoinCount(slot);
            int totalPower = skill.basicValue;

            if (activeCoinCount <= 0)
            {
                onComplete?.Invoke(totalPower);
                yield break;
            }

            float delayPerCoin = totalDuration / activeCoinCount;
            var prob = CalculateSlotProbability(slot.Owner.Sp);

            for (var i = 0; i < skill.coins.Count; i++)
            {
                var coin = skill.coins[i];
                if (!coin.isBroken)
                {
                    bool isFront = Random.value < prob;
                    totalPower += isFront ? coin.frontValue : coin.backValue;

                    var uiState = isFront ? Character.CoinUIState.Front : Character.CoinUIState.Back;
                    slot.Owner.UpdateCoinUI(i, uiState);
                    slot.Owner.UpdateTotalValueText(totalPower);
                    slot.Owner.PlayCharacterSfx(coin.coinTossSound);

                    if (isFocus) yield return new WaitForSecondsRealtime(delayPerCoin);
                    else yield return new WaitForSeconds(delayPerCoin);
                }
            }

            onComplete?.Invoke(totalPower);
        }

        private static float CalculateSlotProbability(float sp)
        {
            var a = (MaxProbability - MinProbability) / (MaxSp - MinSp);
            var b = MinProbability - a * -MinSp;
            return Mathf.Clamp(a * sp + b, MinProbability, MaxProbability);
        }

        private static float CalculateSlotProbabilityForce(bool isFront)
        {
            return isFront ? 1.0f : 0.0f;
        }
    }
}