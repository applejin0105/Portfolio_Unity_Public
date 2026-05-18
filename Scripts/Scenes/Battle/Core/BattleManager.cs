using System;
using System.Collections;
using System.Collections.Generic;
using Scenes.Battle.Cameras;
using Scenes.Battle.Combat;
using Scenes.Battle.Data;
using Scenes.Battle.Entity;
using UnityEngine;

namespace Scenes.Battle.Core
{
    public class BattleManager : MonoBehaviour
    {
        [Header("System References")]
        public CameraTracker cameraTracker;
        public CameraController cameraController;
        public BattleActionController actionController;

        [Header("Base Battle Stats")]
        public float globalMoveSpeedMultiplier = 5.0f;
        public float baseSpeedPlayer = 10f;
        public float maxSpeedPlayer = 150f;
        public float baseSpeedEnemy = 10f;
        public float maxSpeedEnemy = 150f;
        public int changeSpCount = 5;

        [Header("Sequence Settings")]
        public float sequenceDelay = 1.5f;
        public float baseKnockbackDuration = 1.0f;
        public float knockbackMultiplier = 1.0f;
        public float minKnockbackDistance = 2.0f;
        public float minKnockbackDuration = 0.1f;
        [Range(1f, 5f)] public float speedProgressionFactor = 2.0f;
        [Range(0.01f, 1f)] public float knockbackDistanceDecayFactor = 0.3f;
        [Range(0.01f, 1f)] public float knockbackDurationDecayFactor = 0.4f;

        [Header("Focus Skill")]
        private List<Transform> _currentActiveFighters = new List<Transform>();

        private int _activeClusters;
        private WaitForSeconds _clashDelay;

        private bool _isFocusSequenceActive;
        private WaitForSeconds _sequenceDelayObj;
        public Action OnTurnEnded;

        public static event Action<bool> OnCombatStateChanged;
        private BattleState _currentState;
        public BattleState CurrentState => _currentState;

        private void Awake()
        {
            _clashDelay = new WaitForSeconds(0.5f);
            _sequenceDelayObj = new WaitForSeconds(sequenceDelay);
        }

        private void OnDestroy()
        {
            OnCombatStateChanged = null;
        }

        #region Turn Execution

        public void ExecuteTurn(List<BattleMatchup> turnBattles)
        {
            if (CurrentState == BattleState.Execution || turnBattles == null || turnBattles.Count == 0)
            {
                Debug.Log(
                    $"[BattleManager] {CurrentState}, {turnBattles}, {turnBattles.Count} battles have been executed");
                return;
            }

            StartCoroutine(TurnCoroutine(turnBattles));
        }

        public void SetBattleState(BattleState newState)
        {
            _currentState = newState;

            bool isCombat = (_currentState == BattleState.Execution);
            OnCombatStateChanged?.Invoke(isCombat);
        }

        private IEnumerator TurnCoroutine(List<BattleMatchup> turnBattles)
        {
            SetBattleState(BattleState.Execution);

            _activeClusters = turnBattles.Count;

            _currentActiveFighters.Clear();
            cameraTracker.ClearFocusPoints(); // 턴 시작 시 포커스 포인트 초기화

            foreach (var battle in turnBattles)
            {
                if (battle.AttackerSlot?.Owner != null) _currentActiveFighters.Add(battle.AttackerSlot.Owner.transform);
                if (battle.DefenderSlot?.Owner != null) _currentActiveFighters.Add(battle.DefenderSlot.Owner.transform);
            }

            cameraTracker.SetTargets(_currentActiveFighters);
            cameraController.SetDynamicTracking(CameraViewType.ShowAll);

            foreach (var battle in turnBattles)
            {
                StartCoroutine(ProcessBattleSequence(battle));
            }

            while (_activeClusters > 0)
            {
                yield return null;
            }

            SetBattleState(BattleState.CheckResult);
            OnTurnEnded?.Invoke();
            SetBattleState(BattleState.Idle);
        }

        private IEnumerator ProcessBattleSequence(BattleMatchup battle)
        {
            var attackerSlot = battle.AttackerSlot;
            var targetSlot = battle.DefenderSlot;
            var attackerChar = attackerSlot.Owner;
            var defenderChar = targetSlot.Owner;

            if (attackerSlot != null) attackerSlot.Owner.ResetCoins(attackerSlot.SelectedSkill);
            if (targetSlot != null) targetSlot.Owner.ResetCoins(targetSlot.SelectedSkill);

            while (attackerChar.IsEngaged || defenderChar.IsEngaged)
            {
                yield return null;
            }

            attackerChar.IsEngaged = true;
            defenderChar.IsEngaged = true;

            // 조기 종료 처리: 타겟이 전투 전 사망했을 경우
            if (attackerChar == null || defenderChar == null || attackerChar.Hp <= 0 || defenderChar.Hp <= 0)
            {
                _activeClusters--;
                if (attackerChar != null)
                {
                    attackerChar.IsEngaged = false;
                    attackerChar.HideSkillUI();
                    _currentActiveFighters.Remove(attackerChar.transform);
                    cameraTracker.RemoveFocusPoint(attackerChar.transform);
                }

                if (defenderChar != null)
                {
                    defenderChar.IsEngaged = false;
                    defenderChar.HideSkillUI();
                    _currentActiveFighters.Remove(defenderChar.transform);
                    cameraTracker.RemoveFocusPoint(defenderChar.transform);
                }

                // 포커스 중이 아니라면 남은 인원들로 카메라 타겟 갱신 (줌인 발생)
                if (!_isFocusSequenceActive && _currentActiveFighters.Count > 0)
                {
                    cameraTracker.SetTargets(_currentActiveFighters);
                }

                yield break;
            }

            var initialAttackerPos = attackerChar.transform.position;
            var initialDefenderPos = defenderChar.transform.position;

            bool isFocus = battle.ViewType == CameraViewType.FocusTarget;

            if (isFocus)
            {
                while (_isFocusSequenceActive) yield return null;

                _isFocusSequenceActive = true;
                Time.timeScale = 0.05f;

                // 포커스 스킬 당사자들만 타겟으로 분리
                cameraTracker.SetTargets(new List<Transform> { attackerChar.transform, defenderChar.transform });
                cameraController.SetDynamicTracking(CameraViewType.FocusTarget);

                foreach (var fighter in _currentActiveFighters)
                {
                    if (fighter == null) continue;
                    var character = fighter.GetComponent<Character>();
                    if (character == null) continue;

                    if (fighter == attackerChar.transform || fighter == defenderChar.transform)
                    {
                        character.SetOpacity(1.0f);
                        character.SetSkillUIOpacity(1.0f);
                        character.SetAnimatorUnscaledTime(true);
                    }
                    else
                    {
                        character.SetOpacity(0.3f);
                        character.SetSkillUIOpacity(0.0f);
                        character.SetAnimatorUnscaledTime(false);
                    }
                }
            }

            if (battle.MatchType == BattleMatchType.Clash)
                yield return StartCoroutine(ExecuteClashSequence(attackerSlot, targetSlot, initialAttackerPos,
                    initialDefenderPos, isFocus));
            else if (battle.MatchType == BattleMatchType.OneSided)
                yield return StartCoroutine(ExecuteOneSidedSequence(attackerSlot, targetSlot, isFocus));

            // 교전이 끝났으므로 해당 쌍의 포커스(예측) 포인트를 해제 (원래 자리로 돌아갈 때는 위치 기반 추적)
            cameraTracker.RemoveFocusPoint(attackerChar.transform);
            cameraTracker.RemoveFocusPoint(defenderChar.transform);

            // Focus 연출 종료 및 상태 복구
            if (isFocus)
            {
                yield return new WaitForSecondsRealtime(1.5f); // 슬로모 여운: 바로 풀지 않고 1.5초 유지
                _isFocusSequenceActive = false;
                Time.timeScale = 1.0f;
                cameraTracker.SetTargets(_currentActiveFighters);
                cameraController.SetDynamicTracking(CameraViewType.ShowAll);

                foreach (var fighter in _currentActiveFighters)
                {
                    if (fighter == null) continue;
                    var character = fighter.GetComponent<Character>();
                    if (character != null)
                    {
                        character.SetOpacity(1.0f);
                        character.SetSkillUIOpacity(1.0f);
                        character.SetAnimatorUnscaledTime(false);
                    }
                }
            }

            if (isFocus) yield return new WaitForSecondsRealtime(0.5f);
            else yield return new WaitForSeconds(0.5f);

            attackerChar.SetMoving(true);
            defenderChar.SetMoving(true);

            StartCoroutine(ReturnToPosition(attackerChar.transform, initialAttackerPos, 0.5f, isFocus));
            yield return StartCoroutine(ReturnToPosition(defenderChar.transform, initialDefenderPos, 0.5f, isFocus));

            attackerChar.SetMoving(false);
            defenderChar.SetMoving(false);

            _activeClusters--;

            attackerChar.IsEngaged = false;
            defenderChar.IsEngaged = false;

            attackerChar.HideSkillUI();
            defenderChar.HideSkillUI();

            // 전투가 완료된 인원을 목록에서 제외하고 카메라 타겟 갱신
            _currentActiveFighters.Remove(attackerChar.transform);
            _currentActiveFighters.Remove(defenderChar.transform);

            if (!_isFocusSequenceActive && _currentActiveFighters.Count > 0)
            {
                cameraTracker.SetTargets(_currentActiveFighters);
            }
        }

        private IEnumerator ReturnToPosition(Transform target, Vector3 startPos, float duration,
            bool useUnscaledTime = false)
        {
            var currentPos = target.position;
            var elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                target.position = Vector3.Lerp(currentPos, startPos, elapsedTime / duration);
                yield return null;
            }

            target.position = startPos;
        }

        #endregion

        #region Battle Sequences

        private IEnumerator ExecuteClashSequence(ActionSlot attackerSlot, ActionSlot targetSlot, Vector3 initialPosA,
            Vector3 initialPosD, bool isFocus)
        {
            attackerSlot.Owner.ShowSkillUI(attackerSlot.SelectedSkill);
            targetSlot.Owner.ShowSkillUI(targetSlot.SelectedSkill);

            var attacker = attackerSlot.Owner.transform;
            var defender = targetSlot.Owner.transform;

            var currentKnockbackDistance = Vector3.Distance(initialPosA, initialPosD);
            var currentKnockbackDuration = baseKnockbackDuration;
            float currentAccumulatedSpeedA = 0f, currentAccumulatedSpeedD = 0f;
            var currentAddedSpeedA = (attackerSlot.Owner.Speed > 0 ? attackerSlot.Owner.Speed : baseSpeedPlayer) *
                                     globalMoveSpeedMultiplier;
            var currentAddedSpeedD = (targetSlot.Owner.Speed > 0 ? targetSlot.Owner.Speed : baseSpeedEnemy) *
                                     globalMoveSpeedMultiplier;

            while (CoinResolver.GetActiveCoinCount(attackerSlot) > 0 && CoinResolver.GetActiveCoinCount(targetSlot) > 0)
            {
                attackerSlot.Owner.ShowSkillUI(attackerSlot.SelectedSkill);
                targetSlot.Owner.ShowSkillUI(targetSlot.SelectedSkill);

                var moverA = new ClashMover
                {
                    Transform = attacker,
                    CurrentSpeed = currentAccumulatedSpeedA,
                    AddedSpeed = currentAddedSpeedA,
                    Weight = attackerSlot.Owner.Weight,
                    MaxSpeed = maxSpeedPlayer,
                    Range = attackerSlot.Owner.CombatRange,
                    IsImmovable = attackerSlot.Owner.IsImmovable
                };
                var moverD = new ClashMover
                {
                    Transform = defender,
                    CurrentSpeed = currentAccumulatedSpeedD,
                    AddedSpeed = currentAddedSpeedD,
                    Weight = targetSlot.Owner.Weight,
                    MaxSpeed = maxSpeedEnemy,
                    Range = targetSlot.Owner.CombatRange,
                    IsImmovable = targetSlot.Owner.IsImmovable
                };

                var meeting = actionController.CalculateMeetingData(moverA, moverD);

                var timeToMeet = meeting.TimeToMeet;
                if (timeToMeet <= 0f) timeToMeet = 0.05f;

                // 계산된 충돌 교차점을 카메라에 등록 (포커스 연출 땐 두 캐릭터의 정중앙을 쓰므로 제외)
                if (!isFocus)
                {
                    cameraTracker.SetFocusPoint(attacker, meeting.MeetPosA);
                    cameraTracker.SetFocusPoint(defender, meeting.MeetPosB);
                }

                attackerSlot.Owner.SetMoving(true);
                targetSlot.Owner.SetMoving(true);

                var approachA =
                    StartCoroutine(actionController.ExecuteMovement(attacker, meeting.MeetPosA, timeToMeet, isFocus));
                var approachD =
                    StartCoroutine(actionController.ExecuteMovement(defender, meeting.MeetPosB, timeToMeet, isFocus));
                yield return approachA;
                yield return approachD;

                attackerSlot.Owner.SetMoving(false);
                targetSlot.Owner.SetMoving(false);

                string clashTriggerA = string.IsNullOrEmpty(attackerSlot.SelectedSkill.clashAnimTrigger)
                    ? attackerSlot.SelectedSkill.skillName
                    : attackerSlot.SelectedSkill.clashAnimTrigger;

                string clashTriggerD = string.IsNullOrEmpty(targetSlot.SelectedSkill.clashAnimTrigger)
                    ? targetSlot.SelectedSkill.skillName
                    : targetSlot.SelectedSkill.clashAnimTrigger;

                attackerSlot.Owner.ExecuteClashMotion(clashTriggerA);
                targetSlot.Owner.ExecuteClashMotion(clashTriggerD);

                int powerA = 0;
                int powerD = 0;
                bool isTossDoneA = false;
                bool isTossDoneD = false;

                float tossDuration = 1.0f;

                StartCoroutine(CoinResolver.TossSlotCoinsRoutine(attackerSlot, tossDuration, result =>
                {
                    powerA = result;
                    isTossDoneA = true;
                }, isFocus));
                StartCoroutine(CoinResolver.TossSlotCoinsRoutine(targetSlot, tossDuration, result =>
                {
                    powerD = result;
                    isTossDoneD = true;
                }, isFocus));

                yield return new WaitUntil(() => isTossDoneA && isTossDoneD);

                if (powerA > powerD)
                {
                    CoinResolver.BreakSlotCoin(targetSlot);
                    attackerSlot.Owner.ChangeSp(changeSpCount);
                    targetSlot.Owner.ChangeSp(-changeSpCount);
                }
                else if (powerD > powerA)
                {
                    CoinResolver.BreakSlotCoin(attackerSlot);
                    targetSlot.Owner.ChangeSp(changeSpCount);
                    attackerSlot.Owner.ChangeSp(-changeSpCount);
                }

                bool isLastClash = (CoinResolver.GetActiveCoinCount(attackerSlot) <= 0 ||
                                    CoinResolver.GetActiveCoinCount(targetSlot) <= 0);

                var knockback = actionController.CalculateKnockback(moverA, moverD, meeting,
                    currentKnockbackDistance, knockbackMultiplier);

                currentAccumulatedSpeedA = knockback.ReboundA;
                currentAccumulatedSpeedD = knockback.ReboundB;

                attackerSlot.Owner.SetMoving(true);
                targetSlot.Owner.SetMoving(true);

                var knockA =
                    StartCoroutine(actionController.ExecuteMovement(attacker, knockback.TargetA,
                        currentKnockbackDuration, isFocus));
                var knockD =
                    StartCoroutine(actionController.ExecuteMovement(defender, knockback.TargetB,
                        currentKnockbackDuration, isFocus));
                yield return knockA;
                yield return knockD;

                attackerSlot.Owner.SetMoving(false);
                targetSlot.Owner.SetMoving(false);

                if (isLastClash) break;

                currentAddedSpeedA = Mathf.Min(currentAddedSpeedA * speedProgressionFactor, maxSpeedPlayer);
                currentAddedSpeedD = Mathf.Min(currentAddedSpeedD * speedProgressionFactor, maxSpeedEnemy);
                currentKnockbackDistance = Mathf.Max(currentKnockbackDistance * knockbackDistanceDecayFactor,
                    minKnockbackDistance);
                currentKnockbackDuration = Mathf.Max(currentKnockbackDuration * knockbackDurationDecayFactor,
                    minKnockbackDuration);

                if (isFocus) yield return new WaitForSecondsRealtime(0.5f);
                else yield return _clashDelay;
            }

            if (CoinResolver.GetActiveCoinCount(attackerSlot) > 0)
            {
                yield return StartCoroutine(ApproachForAttack(attackerSlot.Owner, targetSlot.Owner, isFocus));
                yield return StartCoroutine(ExecuteMultiHitAttack(attackerSlot, targetSlot.Owner, isFocus));
            }
            else if (CoinResolver.GetActiveCoinCount(targetSlot) > 0)
            {
                yield return StartCoroutine(ApproachForAttack(targetSlot.Owner, attackerSlot.Owner, isFocus));
                yield return StartCoroutine(ExecuteMultiHitAttack(targetSlot, attackerSlot.Owner, isFocus));
            }
        }

        private IEnumerator ApproachForAttack(Character attacker, Character defender, bool isFocus)
        {
            var dir = (defender.transform.position - attacker.transform.position).normalized;
            if (dir == Vector3.zero) dir = Vector3.forward;

            var dashTargetPos = defender.transform.position - dir * attacker.CombatRange;

            attacker.SetMoving(true);
            yield return StartCoroutine(actionController.ExecuteMovement(attacker.transform, dashTargetPos, 0.15f,
                isFocus));
            attacker.SetMoving(false);
        }

        private IEnumerator ExecuteOneSidedSequence(ActionSlot attackerSlot, ActionSlot targetSlot, bool isFocus)
        {
            attackerSlot.Owner.ShowSkillUI(attackerSlot.SelectedSkill);

            var attacker = attackerSlot.Owner.transform;
            var target = targetSlot.Owner.transform;

            var approachSpeed = (attackerSlot.Owner.Speed > 0 ? attackerSlot.Owner.Speed : baseSpeedPlayer) *
                                globalMoveSpeedMultiplier;
            var dir = (target.position - attacker.position).normalized;
            if (dir == Vector3.zero) dir = Vector3.forward;

            var targetPos = target.position - dir * attackerSlot.Owner.CombatRange;
            var distance = Vector3.Distance(attacker.position, targetPos);
            var timeToMeet = distance / Mathf.Max(approachSpeed, 1f);

            // 일방 공격의 최종 타격 지점을 카메라에 등록 (포커스 연출 땐 두 캐릭터의 정중앙을 쓰므로 제외)
            if (!isFocus)
            {
                cameraTracker.SetFocusPoint(attacker, targetPos);
                cameraTracker.SetFocusPoint(target, target.position);
            }

            attackerSlot.Owner.SetMoving(true);
            yield return StartCoroutine(actionController.ExecuteMovement(attacker, targetPos, timeToMeet, isFocus));
            attackerSlot.Owner.SetMoving(false);

            yield return StartCoroutine(ExecuteMultiHitAttack(attackerSlot, targetSlot.Owner, isFocus));

            if (!targetSlot.Owner.IsImmovable)
            {
                var knockbackTarget = target.position + dir * minKnockbackDistance;

                targetSlot.Owner.SetMoving(true);
                yield return StartCoroutine(actionController.ExecuteMovement(target, knockbackTarget,
                    minKnockbackDuration,
                    isFocus));
                targetSlot.Owner.SetMoving(false);
            }
        }

        private IEnumerator ExecuteMultiHitAttack(ActionSlot attackerSlot, Character targetChar, bool isFocus)
        {
            var coins = attackerSlot.SelectedSkill.coins;
            var strength = attackerSlot.Owner.Strength > 0 ? attackerSlot.Owner.Strength : 1;
            var currentPower = attackerSlot.SelectedSkill.basicValue;

            var attackerTransform = attackerSlot.Owner.transform;

            if (coins == null) yield break;

            for (var coinIndex = 0; coinIndex < coins.Count; coinIndex++)
            {
                if (coins[coinIndex].isBroken) continue;
                if (targetChar == null || targetChar.gameObject == null || targetChar.Hp <= 0) break;

                var coin = coins[coinIndex];
                currentPower += CoinResolver.TossSingleCoin(attackerSlot, coinIndex);
                attackerSlot.Owner.UpdateTotalValueText(currentPower);
                var totalDamage = currentPower * strength;

                if (isFocus) yield return new WaitForSecondsRealtime(0.2f);
                else yield return new WaitForSeconds(0.2f);

                attackerSlot.Owner.PlayCharacterSfx(coin.attackSound);

                if (coin.attackDashDistance > 0f)
                {
                    var dashDir = (targetChar.transform.position - attackerTransform.position).normalized;
                    if (dashDir == Vector3.zero) dashDir = Vector3.forward;

                    var dashTargetPos = targetChar.transform.position - dashDir * attackerSlot.Owner.CombatRange;

                    attackerSlot.Owner.ExecuteAttackMotion(coin.animTrigger);
                    yield return StartCoroutine(ReturnToPosition(attackerTransform, dashTargetPos, 0.15f, isFocus));
                }
                else
                {
                    attackerSlot.Owner.ExecuteAttackMotion(coin.animTrigger);
                }

                if (coin.vfxPrefab != null)
                    Instantiate(coin.vfxPrefab, attackerSlot.Owner.transform.position, Quaternion.identity);

                if (coin.customLogic != null)
                {
                    yield return StartCoroutine(coin.customLogic.Execute(attackerSlot.Owner, targetChar, coin,
                        totalDamage, isFocus));
                }
                else
                {
                    var hits = coin.hitCount > 0 ? coin.hitCount : 1;
                    var damagePerHit = totalDamage / hits;

                    for (var i = 0; i < hits; i++)
                    {
                        targetChar.TakeDamage(damagePerHit, coin.hitSound);

                        float currentDelay = coin.hitDelay > 0f ? coin.hitDelay : 0.5f; // 기본값 설정

                        if (coin.customHitDelays != null && i < coin.customHitDelays.Length)
                        {
                            currentDelay = coin.customHitDelays[i];
                        }

                        if (isFocus) yield return new WaitForSecondsRealtime(currentDelay);
                        else yield return new WaitForSeconds(currentDelay);

                        if (targetChar.Hp <= 0) break;
                    }
                }

                if (targetChar.Hp <= 0) break;
            }
        }

        #endregion
    }
}