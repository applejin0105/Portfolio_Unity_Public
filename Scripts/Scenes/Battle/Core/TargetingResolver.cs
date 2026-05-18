using System.Collections.Generic;
using System.Linq;
using Scenes.Battle.Data;
using Scenes.Battle.Entity;
using UnityEngine;

namespace Scenes.Battle.Core
{
    public class TargetingResolver
    {
        public List<BattleMatchup> ResolveTurn(List<Character> players, List<Character> enemies)
        {
            var finalBattles = new List<BattleMatchup>();

            foreach (var character in players)
                if (character != null && character.Hp > 0)
                    character.RollBattleSpeed();
            foreach (var character in enemies)
                if (character != null && character.Hp > 0)
                    character.RollBattleSpeed();

            var playerSlots = GenerateSlots(players);
            var enemySlots = GenerateSlots(enemies);

            if (playerSlots.Count == 0 || enemySlots.Count == 0) return finalBattles;

            foreach (var enemy in enemySlots)
            {
                enemy.TargetSlot = playerSlots[Random.Range(0, playerSlots.Count)];
            }

            SortSlotsBySpeed(playerSlots);

            foreach (var pSlot in playerSlots)
            {
                List<ActionSlot> candidateSlot = new List<ActionSlot>();
                foreach (var eSlot in enemySlots)
                {
                    if (eSlot.Owner.Speed <= pSlot.Owner.Speed && playerSlots.All(p => p.TargetSlot != eSlot))
                    {
                        candidateSlot.Add(eSlot);
                    }
                }

                if (candidateSlot.Count > 0)
                {
                    SortSlotsBySpeed(candidateSlot);

                    var selectedEnemySlot = candidateSlot[0];

                    pSlot.TargetSlot = selectedEnemySlot;
                    selectedEnemySlot.TargetSlot = pSlot;
                }
                else
                    pSlot.TargetSlot = enemySlots[Random.Range(0, enemySlots.Count)];
            }

            var allSlots = new List<ActionSlot>();
            allSlots.AddRange(playerSlots);
            allSlots.AddRange(enemySlots);

            SortSlotsBySpeed(allSlots);

            var processedSlots = new HashSet<ActionSlot>();

            foreach (var slot in allSlots)
            {
                // 이미 합 진행 했으면 패스. 여기에 추후 흐트러짐 상태면 패스 로직을 넣어도 됨!
                if (processedSlots.Contains(slot)) continue;

                var targetSlot = slot.TargetSlot;
                var isFocus = slot.SelectedSkill.isFocusSkill || targetSlot.SelectedSkill.isFocusSkill;
                var viewType = isFocus ? CameraViewType.FocusTarget : CameraViewType.Normal;

                if (targetSlot.TargetSlot == slot)
                {
                    finalBattles.Add(new BattleMatchup
                    {
                        AttackerSlot = slot,
                        DefenderSlot = targetSlot,
                        MatchType = BattleMatchType.Clash,
                        ViewType = viewType
                    });

                    processedSlots.Add(slot);
                    processedSlots.Add(targetSlot);
                }
                else
                {
                    finalBattles.Add(new BattleMatchup
                    {
                        AttackerSlot = slot,
                        DefenderSlot = targetSlot,
                        MatchType = BattleMatchType.OneSided,
                        ViewType = viewType
                    });

                    processedSlots.Add(slot);
                }
            }

            return finalBattles;
        }

        private List<ActionSlot> GenerateSlots(List<Character> characters)
        {
            var slots = new List<ActionSlot>();
            foreach (var character in characters)
            {
                if (character == null || character.Hp <= 0) continue;

                for (int i = 0; i < character.ActionSlotCount; i++)
                {
                    slots.Add(new ActionSlot
                    {
                        Owner = character,
                        SelectedSkill = character.GetRandomSkillByProbability(),
                    });
                }
            }

            return slots;
        }

        private void SortSlotsBySpeed(List<ActionSlot> slots)
        {
            slots.Sort((a, b) =>
            {
                int result = b.Owner.Speed.CompareTo(a.Owner.Speed);
                if (result == 0)
                    result = a.Owner.MaxSpeed.CompareTo(b.Owner.MaxSpeed); // 보조 기준

                return result;
            });
        }
    }
}