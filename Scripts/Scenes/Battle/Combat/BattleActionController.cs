using System.Collections;
using Core.Extensions;
using UnityEngine;

namespace Scenes.Battle.Combat
{
    // 충돌(합)에 참여하는 한쪽의 운동 정보
    public struct ClashMover
    {
        public Transform Transform;
        public float CurrentSpeed;
        public float AddedSpeed;
        public float Weight;
        public float MaxSpeed;
        public float Range;

        public bool IsImmovable;
    }

    // 두 물체가 만나는 시점·위치·최종 속도 계산 결과
    public struct MeetingResult
    {
        public float TimeToMeet;
        public Vector3 MeetPosA;
        public Vector3 MeetPosB;
        public Vector3 FinalVelA;
        public Vector3 FinalVelB;
    }

    // 충돌 후 두 물체가 튕겨날 목표 지점·반동 속도 계산 결과
    public struct KnockbackResult
    {
        public Vector3 TargetA;
        public Vector3 TargetB;
        public float ReboundA;
        public float ReboundB;
    }

    public class BattleActionController : MonoBehaviour
    {
        #region Movement Execution

        // 한 Transform을 destination까지 duration 동안 부드럽게 이동시킨다 (Y축 고정)
        public IEnumerator ExecuteMovement(Transform target, Vector3 destination, float duration,
            bool useUnscaledTime = false)
        {
            var startPos = target.position;
            var endPos = new Vector3(destination.x, startPos.y, destination.z); // Y축 고정
            var elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                var rawT = elapsedTime / duration;
                var t = 1f - Mathf.Cos(rawT * Mathf.PI * 0.5f);
                target.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            target.position = endPos;
        }

        #endregion

        #region Calculation Logic

        // 두 물체가 가속하며 접근할 때 만나는 시점·위치·최종 속도를 예측한다.
        public MeetingResult CalculateMeetingData(ClashMover a, ClashMover b)
        {
            var posA = new Vector3(a.Transform.position.x, 0f, a.Transform.position.z);
            var posB = new Vector3(b.Transform.position.x, 0f, b.Transform.position.z);

            var dirA = (posB - posA).normalized;
            if (dirA == Vector3.zero) dirA = Vector3.forward;
            var dirB = -dirA;

            var startSpeedA = Mathf.Min(a.CurrentSpeed + a.AddedSpeed, a.MaxSpeed);
            var startSpeedB = Mathf.Min(b.CurrentSpeed + b.AddedSpeed, b.MaxSpeed);

            var velA = dirA * startSpeedA;
            var velB = dirB * startSpeedB;

            var accelA = dirA * (a.AddedSpeed / a.Weight);
            var accelB = dirB * (b.AddedSpeed / b.Weight);

            var willMeet = IntersectionCalculator.CalculateIntersection(
                posA, velA, accelA, posB, velB, accelB,
                out var timeToMeet, out var meetingPoint);

            var result = new MeetingResult();

            if (willMeet)
            {
                result.FinalVelA = Vector3.ClampMagnitude(velA + accelA * timeToMeet, a.MaxSpeed);
                result.FinalVelB = Vector3.ClampMagnitude(velB + accelB * timeToMeet, b.MaxSpeed);
            }
            else
            {
                // 교차하지 않으면 중간 지점에서 만난다고 근사
                timeToMeet = Vector3.Distance(posA, posB) / Mathf.Max(startSpeedA + startSpeedB, 1f);
                meetingPoint = (posA + posB) * 0.5f;
                result.FinalVelA = velA;
                result.FinalVelB = velB;
            }

            result.TimeToMeet = timeToMeet;
            // 실제 충돌점에서 각자 사거리만큼 물러난 곳이 멈출 위치
            result.MeetPosA = meetingPoint - dirA * a.Range;
            result.MeetPosB = meetingPoint - dirB * b.Range;
            return result;
        }

        // 충돌 후 두 물체가 튕겨날 목표 지점과 반동 속도를 계산한다.
        public KnockbackResult CalculateKnockback(ClashMover a, ClashMover b, MeetingResult meeting,
            float baseKnockbackDistance, float knockbackMultiplier)
        {
            var speedA = meeting.FinalVelA.magnitude;
            var speedB = meeting.FinalVelB.magnitude;
            var totalSpeed = speedA + speedB;

            var distanceRatioA = totalSpeed > Mathf.Epsilon ? speedB / totalSpeed : 0.5f;
            var distanceRatioB = totalSpeed > Mathf.Epsilon ? speedA / totalSpeed : 0.5f;

            var speedRatioA = totalSpeed > Mathf.Epsilon ? speedA / totalSpeed : 0.5f;
            var speedRatioB = totalSpeed > Mathf.Epsilon ? speedB / totalSpeed : 0.5f;

            if (a.IsImmovable)
            {
                distanceRatioA = 0f;
                speedRatioA = 0f; // A는 절대 안 밀리고 반동도 없음
                distanceRatioB = 1f;
                speedRatioB = 1f; // B가 넉백 100% 독박
            }

            if (b.IsImmovable)
            {
                distanceRatioA = 1f;
                speedRatioA = 1f;
                distanceRatioB = 0f;
                speedRatioB = 0f;
            }

            var dirA = ResolveKnockbackDir(meeting.FinalVelA, a.Transform.position, b.Transform.position,
                Vector3.back);
            var dirB = ResolveKnockbackDir(meeting.FinalVelB, b.Transform.position, a.Transform.position,
                Vector3.forward);

            var distA = baseKnockbackDistance * distanceRatioA;
            var distB = baseKnockbackDistance * distanceRatioB;

            return new KnockbackResult
            {
                TargetA = a.Transform.position + new Vector3(dirA.x * distA, 0f, dirA.z * distA),
                TargetB = b.Transform.position + new Vector3(dirB.x * distB, 0f, dirB.z * distB),

                ReboundA = speedA * speedRatioA * knockbackMultiplier,
                ReboundB = speedB * speedRatioB * knockbackMultiplier
            };
        }

        // 넉백 방향: 진행 속도의 반대. 속도가 없으면 상대의 반대편, 그것도 없으면 fallback.
        private static Vector3 ResolveKnockbackDir(Vector3 finalVel, Vector3 selfPos, Vector3 otherPos,
            Vector3 fallback)
        {
            var flatVel = new Vector3(-finalVel.x, 0f, -finalVel.z);
            var dir = flatVel.sqrMagnitude > Mathf.Epsilon
                ? flatVel.normalized
                : (selfPos - otherPos).normalized;
            if (dir.sqrMagnitude < Mathf.Epsilon) dir = fallback;
            return dir;
        }

        #endregion
    }
}