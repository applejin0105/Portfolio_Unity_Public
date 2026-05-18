using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scenes.Battle.Data
{
    [Serializable]
    public struct RarityChance
    {
        public UnitRarity rarity;
        [Range(0f, 100f)] public float chance;
    }

    [Serializable]
    public struct ShopLevelProbability
    {
        public int shopLevel;
        public List<RarityChance> rarityChances;
    }
}