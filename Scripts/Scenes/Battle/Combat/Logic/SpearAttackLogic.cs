using System.Collections;
using System.Collections.Generic;
using Core.Data.Enums;
using Scenes.Battle.Data;
using Scenes.Battle.Entity;
using Scenes.Battle.Logic;
using UnityEngine;

namespace Scenes.Battle.Combat.Logic
{
    [CreateAssetMenu(fileName = "SpearAttackLogic", menuName = "Battle/Logic/Spear Attack")]
    public class SpearAttackLogic : CoinActionLogic
    {
        [Header("Spear Prefabs")]
        [SerializeField] private SpearProjectile[] spearPrefabs = new SpearProjectile[8];

        [Header("Position Settings")]
        [SerializeField] private float radius = 2.0f;
        [SerializeField] private float yOffset = 1.0f; // 캐릭터 머리 위쪽으로 띄울 높이
        [SerializeField] private Vector3 focusPosition = new Vector3(0, 0, 0); // Focus 시 이동할 좌표

        [Header("Animation Settings")]
        [SerializeField] private float gatherDuration = 1.0f;
        [SerializeField] private float fireDelay = 0.15f;
        [SerializeField] private float fireSpeed = 25f;

        [Header("Sequential Sound Settings")]
        [Tooltip("창을 1개씩 '발사'할 때 순차적으로 재생할 사운드 배열입니다. 비워두면 Coin의 기본 AttackSound를 사용합니다.")]
        [SerializeField] private BattleSoundType[] sequentialAttackSounds;

        [Tooltip("창이 타겟에 '적중'할 때 순차적으로 재생할 사운드 배열입니다. 비워두면 Coin의 기본 HitSound를 사용합니다.")]
        [SerializeField] private BattleSoundType[] sequentialHitSounds;

        public override IEnumerator Execute(Character attacker, Character target, Coin coin, int totalDamage,
            bool isFocus)
        {
            if (isFocus) yield return MoveToFocusPosition(attacker, focusPosition);

            int spearCount = GetSpearCountByUnitRarity(attacker.currentUnitData.rarity);
            int damagePerHit = Mathf.Max(1, totalDamage / spearCount);

            List<SpearProjectile> activeSpears = SpawnSpears(attacker.transform, spearCount);

            if (isFocus) yield return new WaitForSecondsRealtime(gatherDuration);
            else yield return new WaitForSeconds(gatherDuration);

            attacker.ExecuteAttackMotion(coin.animTrigger);

            if (isFocus) yield return new WaitForSecondsRealtime(0.2f);
            else yield return new WaitForSeconds(0.2f);

            int firedCount = 0;

            for (int i = 0; i < activeSpears.Count; i++)
            {
                if (target == null || target.Hp <= 0) break;

                SpearProjectile spear = activeSpears[i];
                if (spear != null)
                {
                    // 1. 순차 발사음(Attack) 결정 및 재생
                    BattleSoundType currentAttackSound = coin.attackSound; // 기본값
                    if (sequentialAttackSounds != null && sequentialAttackSounds.Length > 0)
                    {
                        currentAttackSound = sequentialAttackSounds[i % sequentialAttackSounds.Length];
                    }

                    attacker.PlayCharacterSfx(currentAttackSound);

                    // 2. 순차 피격음(Hit) 결정 (콜백에 넘겨주기 위해 미리 계산)
                    BattleSoundType currentHitSound = coin.hitSound; // 기본값
                    if (sequentialHitSounds != null && sequentialHitSounds.Length > 0)
                    {
                        currentHitSound = sequentialHitSounds[i % sequentialHitSounds.Length];
                    }

                    spear.Fire(target.transform, fireSpeed, (hitSpear) =>
                    {
                        if (target != null)
                        {
                            // 3. 적중 시 피격음 재생 및 대미지 적용
                            target.PlayCharacterSfx(currentHitSound);
                            target.TakeDamage(damagePerHit);
                            target.ExecuteAttackMotion("Hit");
                        }
                    });

                    firedCount++;

                    if (isFocus) yield return new WaitForSecondsRealtime(fireDelay);
                    else yield return new WaitForSeconds(fireDelay);
                }
            }

            // 발사된 투사체가 모두 타겟에 명중(파괴)될 때까지 강제 대기하여 포커스 해제를 막음
            for (int i = 0; i < firedCount; i++)
            {
                while (activeSpears[i] != null)
                {
                    yield return null;
                }
            }

            for (int i = firedCount; i < activeSpears.Count; i++)
            {
                if (activeSpears[i] != null) Destroy(activeSpears[i].gameObject);
            }
        }

        private IEnumerator MoveToFocusPosition(Character attacker, Vector3 targetPos)
        {
            float duration = 0.3f;
            float elapsed = 0f;
            Vector3 startPos = attacker.transform.position;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                attacker.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
                yield return null;
            }

            attacker.transform.position = targetPos;
        }

        private int GetSpearCountByUnitRarity(UnitRarity rarity)
        {
            return rarity switch
            {
                UnitRarity.Common => 1,
                UnitRarity.Rare => 4,
                UnitRarity.Legendary => 8,
                _ => 1
            };
        }

        private List<SpearProjectile> SpawnSpears(Transform attackerTransform, int count)
        {
            List<SpearProjectile> spears = new List<SpearProjectile>();
            float angleStep = 360f / count;

            for (int i = 0; i < count; i++)
            {
                int prefabIndex = Random.Range(0, spearPrefabs.Length);
                SpearProjectile prefab = spearPrefabs[prefabIndex];
                if (prefab == null) continue;

                SpearProjectile spear = Instantiate(prefab, attackerTransform.position, Quaternion.identity);
                // Initialize에 yOffset(높이) 인자를 추가하여 전달합니다.
                spear.Initialize(attackerTransform, i * angleStep, radius, yOffset);
                spears.Add(spear);
            }

            return spears;
        }
    }
}