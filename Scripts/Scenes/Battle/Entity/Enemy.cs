using UnityEngine;

namespace Scenes.Battle.Entity
{
    public class Enemy : Character
    {
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
    }
}