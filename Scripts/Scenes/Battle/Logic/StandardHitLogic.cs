using System.Collections;
using Scenes.Battle.Data;
using Scenes.Battle.Entity;
using UnityEngine;

namespace Scenes.Battle.Logic
{
    [CreateAssetMenu(fileName = "StandardHit", menuName = "Battle/Logic/StandardHit")]
    public class StandardHitLogic : CoinActionLogic
    {
        // 인스펙터에서 피격 시 밀려나는 거리를 조절할 수 있게 변수 추가
        [Header("Impact Settings")]
        public float pushbackDistancePerHit = 0.5f;

        public override IEnumerator Execute(Character attacker, Character target, Coin coinData, int totalDamage,
            bool isFocusSequence)
        {
            var hits = coinData.hitCount > 0 ? coinData.hitCount : 1;
            var damagePerHit = totalDamage / hits;

            // 밀려날 방향 계산 (공격자 -> 방어자 방향)
            var pushDirection = (target.transform.position - attacker.transform.position).normalized;
            if (pushDirection == Vector3.zero) pushDirection = Vector3.forward;

            for (var i = 0; i < hits; i++)
            {
                // 피격음(TakeDamage 내부 재생) 및 데미지 적용
                target.TakeDamage(damagePerHit, coinData.hitSound);

                // 살아있을 때만 밀어내기 및 피격 모션 적용
                if (target.Hp > 0)
                {
                    target.ExecuteAttackMotion("Hit");
                    if (!target.IsImmovable)
                    {
                        var targetPos = target.transform.position + pushDirection * pushbackDistancePerHit;
                        target.transform.position = targetPos;
                    }
                }

                Debug.Log($"[Standard Hit] {attacker.Name} 타격! ({i + 1}/{hits}타) = {damagePerHit} 대미지.");

                float currentDelay = coinData.hitDelay > 0f ? coinData.hitDelay : 0.5f;
                if (coinData.customHitDelays != null && i < coinData.customHitDelays.Length)
                {
                    currentDelay = coinData.customHitDelays[i];
                }

                yield return isFocusSequence
                    ? new WaitForSecondsRealtime(currentDelay)
                    : new WaitForSeconds(currentDelay);

                if (target.Hp <= 0) break;
            }
        }
    }
}