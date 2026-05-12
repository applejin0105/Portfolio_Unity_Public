using System.Collections;
using Scenes.Battle.Data;
using Scenes.Battle.Entity;
using UnityEngine;

namespace Scenes.Battle.Logic
{
    [CreateAssetMenu(fileName = "EscalatingHit", menuName = "Battle/Logic/EscalatingHit")]
    public class EscalatingHitLogic : CoinActionLogic
    {
        public override IEnumerator Execute(Character attacker, Character target, Coin coinData, int totalDamage,
            bool isFocusSequence)
        {
            var hits = coinData.hitCount > 1 ? coinData.hitCount : 2;
            var delay = coinData.hitDelay > 0f ? coinData.hitDelay : 0.5f;

            var multipliers = new float[hits];
            float sum = 0;
            for (var i = 0; i < hits; i++)
            {
                multipliers[i] = i + 1;
                sum += multipliers[i];
            }

            for (var i = 0; i < hits; i++)
            {
                var damagePerHit = Mathf.RoundToInt(totalDamage * (multipliers[i] / sum));

                target.PlayCharacterSfx(coinData.hitSound);
                target.TakeDamage(damagePerHit);

                if (target.Hp > 0) target.ExecuteAttackMotion("Hit");

                Debug.Log($"[Escalating Hit] {attacker.Name} 타격! ({i + 1}/{hits}타) = {damagePerHit} 대미지.");

                yield return isFocusSequence ? new WaitForSecondsRealtime(delay) : new WaitForSeconds(delay);

                if (target.Hp <= 0) break;
            }
        }
    }
}