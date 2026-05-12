using Scenes.Battle.Data;
using UnityEngine;
using UnityEngine.U2D.Animation;

namespace Scenes.Battle.Entity
{
    public class Enemy : Character
    {
        // 적 생성 시 호출될 설정 메서드
        public void SetupEnemy(UnitData data, int starLevel = 1)
        {
            // 부모 클래스의 기본 설정 실행 (HP, 스킬 등)
            base.SetupUnit(data, starLevel);
        }

        // 피격 애니메이션 트리거 (공통 사용)
        public void PlayHitAnimation()
        {
            if (Anim != null)
            {
                Anim.SetTrigger("Hit");
            }
        }

        public override void Attack()
        {
            if (Anim != null) Anim.SetTrigger("Attack");
        }

        public override void Action()
        {
            Debug.Log($"[Enemy: {Name}] 기본 액션 수행");
        }

        public override void Defend()
        {
            Debug.Log($"[Enemy: {Name}] 방어 태세 전환");
        }

        public override void Avoid()
        {
            Debug.Log($"[Enemy: {Name}] 회피 기동");
        }

        public override void CounterAttack()
        {
            Debug.Log($"[Enemy: {Name}] 반격 실행");
        }

        public override void ChangeSp(int amount)
        {
        }

        protected override float CalculateProbability(float x)
        {
            return 0.5f;
        }
    }
}