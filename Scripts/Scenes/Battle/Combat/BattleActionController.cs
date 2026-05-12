using System.Collections;
using Core.Extensions;
using UnityEngine;

namespace Scenes.Battle.Combat
{
    public class BattleActionController : MonoBehaviour
    {
        #region Movement Execution

        // 단일 객체 이동으로 리팩토링 (개별 호출)
        public IEnumerator ExecuteMovement(Transform target, Vector3 destination, float duration,
            bool useUnscaledTime = false)
        {
            var startPos = target.position;
            var endPos = new Vector3(destination.x, startPos.y, destination.z); // Y축 고정
            var elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                var t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);
                target.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            target.position = endPos;
        }

        #endregion

        #region Calculation Logic

        public void CalculateMeetingData(
            Transform charPlayer, ref float currentSpeedPlayer, float addedSpeedPlayer, float weightPlayer,
            float maxSpeedPlayer, float rangePlayer,
            Transform charEnemy, ref float currentSpeedEnemy, float addedSpeedEnemy, float weightEnemy,
            float maxSpeedEnemy, float rangeEnemy,
            out float timeToMeet, out Vector3 meetPosPlayer, out Vector3 meetPosEnemy, out Vector3 finalVelPlayer,
            out Vector3 finalVelEnemy)
        {
            var posPlayer = new Vector3(charPlayer.position.x, 0f, charPlayer.position.z);
            var posEnemy = new Vector3(charEnemy.position.x, 0f, charEnemy.position.z);

            var dirPlayer = (posEnemy - posPlayer).normalized;
            if (dirPlayer == Vector3.zero) dirPlayer = Vector3.forward;
            var dirEnemy = -dirPlayer;

            var startSpeedPlayer = Mathf.Min(currentSpeedPlayer + addedSpeedPlayer, maxSpeedPlayer);
            var startSpeedEnemy = Mathf.Min(currentSpeedEnemy + addedSpeedEnemy, maxSpeedEnemy);

            var vPlayer = dirPlayer * startSpeedPlayer;
            var vEnemy = dirEnemy * startSpeedEnemy;

            var aPlayer = dirPlayer * (addedSpeedPlayer / weightPlayer);
            var aEnemy = dirEnemy * (addedSpeedEnemy / weightEnemy);

            var willMeet = IntersectionCalculator.CalculateIntersection(
                posPlayer, vPlayer, aPlayer, posEnemy, vEnemy, aEnemy,
                out timeToMeet, out var absoluteMeetingPoint);

            if (willMeet)
            {
                finalVelPlayer = Vector3.ClampMagnitude(vPlayer + aPlayer * timeToMeet, maxSpeedPlayer);
                finalVelEnemy = Vector3.ClampMagnitude(vEnemy + aEnemy * timeToMeet, maxSpeedEnemy);
            }
            else
            {
                timeToMeet = Vector3.Distance(posPlayer, posEnemy) / Mathf.Max(startSpeedPlayer + startSpeedEnemy, 1f);
                absoluteMeetingPoint = (posPlayer + posEnemy) * 0.5f;
                finalVelPlayer = vPlayer;
                finalVelEnemy = vEnemy;
            }

            meetPosPlayer = absoluteMeetingPoint - dirPlayer * rangePlayer;
            meetPosEnemy = absoluteMeetingPoint - dirEnemy * rangeEnemy;
        }

        public void CalculateKnockback(
            Transform charPlayer, Vector3 finalVelPlayer, float weightPlayer,
            Transform charEnemy, Vector3 finalVelEnemy, float weightEnemy,
            float baseKnockbackDistance, float knockbackMultiplier,
            out Vector3 knockbackTargetPlayer, out Vector3 knockbackTargetEnemy,
            out float reboundSpeedPlayer, out float reboundSpeedEnemy)
        {
            var speedPlayer = finalVelPlayer.magnitude;
            var speedEnemy = finalVelEnemy.magnitude;
            var totalSpeed = speedPlayer + speedEnemy;

            var pushRatioPlayer = 0.5f;
            var pushRatioEnemy = 0.5f;

            if (totalSpeed > Mathf.Epsilon)
            {
                pushRatioPlayer = speedPlayer / totalSpeed;
                pushRatioEnemy = speedEnemy / totalSpeed;
            }

            var flatVelPlayer = new Vector3(-finalVelPlayer.x, 0f, -finalVelPlayer.z);
            var dirPlayer = flatVelPlayer.sqrMagnitude > Mathf.Epsilon
                ? flatVelPlayer.normalized
                : (charPlayer.position - charEnemy.position).normalized;
            if (dirPlayer.sqrMagnitude < Mathf.Epsilon) dirPlayer = Vector3.back;

            var flatVelEnemy = new Vector3(-finalVelEnemy.x, 0f, -finalVelEnemy.z);
            var dirEnemy = flatVelEnemy.sqrMagnitude > Mathf.Epsilon
                ? flatVelEnemy.normalized
                : (charEnemy.position - charPlayer.position).normalized;
            if (dirEnemy.sqrMagnitude < Mathf.Epsilon) dirEnemy = Vector3.forward;

            var actualDistancePlayer = baseKnockbackDistance * pushRatioPlayer;
            var actualDistanceEnemy = baseKnockbackDistance * pushRatioEnemy;

            knockbackTargetPlayer = charPlayer.position +
                                    new Vector3(dirPlayer.x * actualDistancePlayer, 0f,
                                        dirPlayer.z * actualDistancePlayer);
            knockbackTargetEnemy = charEnemy.position +
                                   new Vector3(dirEnemy.x * actualDistanceEnemy, 0f, dirEnemy.z * actualDistanceEnemy);

            reboundSpeedPlayer = speedPlayer * pushRatioPlayer * knockbackMultiplier;
            reboundSpeedEnemy = speedEnemy * pushRatioEnemy * knockbackMultiplier;
        }

        #endregion
    }
}