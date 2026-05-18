using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scenes.Battle.Data
{
    [CreateAssetMenu(fileName = "RoundData", menuName = "Battle/RoundData")]
    public class RoundData : ScriptableObject
    {
        public List<Round> rounds;
    }

    [Serializable]
    public struct EnemySpawnData
    {
        public UnitData enemyData;
        public int starLevel;
    }

    [Serializable]
    public struct Round
    {
        public int roundIndex;
        public List<EnemySpawnData> enemiesToSpawn;
    }
}