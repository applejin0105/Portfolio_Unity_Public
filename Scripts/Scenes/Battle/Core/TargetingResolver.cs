using System.Collections.Generic;
using System.Linq;
using Scenes.Battle.Data;
using Scenes.Battle.Entity;
using UnityEngine;

namespace Scenes.Battle.Core
{
    public class TargetingResolver
    {
        public List<BattleStruct> ResolveTurn(List<Character> players, List<Character> enemies)
        {
            var finalBattles = new List<BattleStruct>();

            foreach (var character in players)
                if (character != null && character.Hp > 0)
                    character.RollBattleSpeed();
            foreach (var character in enemies)
                if (character != null && character.Hp > 0)
                    character.RollBattleSpeed();

            var playerSlots = GenerateSlots(players, true);
            var enemySlots = GenerateSlots(enemies, false);

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
                    if (eSlot.Owner.Speed <= pSlot.Owner.Speed && !playerSlots.Any(p => p.TargetSlot == eSlot))
                    {
                        candidateSlot.Add(eSlot);
                    }
                }

                if (candidateSlot.Count > 0)
                {
                    var selectedEnemySlot = candidateSlot[Random.Range(0, candidateSlot.Count)];
                    pSlot.TargetSlot = selectedEnemySlot;
                    selectedEnemySlot.TargetSlot = pSlot;
                }
                else
                    pSlot.TargetSlot = enemySlots[Random.Range(0, enemySlots.Count)];
            }

            foreach (var pSlot in playerSlots)
            {
                var viewType = pSlot.SelectedSkill.isFocusSkill ? CameraViewType.FocusTarget : CameraViewType.Normal;

                // 합!
                if (pSlot.TargetSlot.TargetSlot == pSlot)
                {
                    finalBattles.Add(new BattleStruct
                    {
                        AttackerSlot = pSlot,
                        DefenderSlot = pSlot.TargetSlot,
                        matchType = BattleMatchType.Clash,
                        viewType = viewType
                    });
                }
                // 일반공격
                else
                {
                    finalBattles.Add(new BattleStruct
                    {
                        AttackerSlot = pSlot,
                        DefenderSlot = pSlot.TargetSlot,
                        matchType = BattleMatchType.OneSided,
                        viewType = viewType
                    });
                }
            }

            foreach (var eSlot in enemySlots)
            {
                if (eSlot.TargetSlot.TargetSlot != eSlot)
                {
                    var viewType = eSlot.SelectedSkill.isFocusSkill
                        ? CameraViewType.FocusTarget
                        : CameraViewType.Normal;
                    finalBattles.Add(new BattleStruct
                    {
                        AttackerSlot = eSlot,
                        DefenderSlot = eSlot.TargetSlot,
                        matchType = BattleMatchType.OneSided,
                        viewType = viewType
                    });
                }
            }

            return finalBattles;
        }

        private List<ActionSlot> GenerateSlots(List<Character> characters, bool isPlayer)
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