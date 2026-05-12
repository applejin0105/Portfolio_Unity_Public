using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scenes.Battle.Data
{
    [CreateAssetMenu(fileName = "StageData", menuName = "Battle/StageData")]
    public class StageData : ScriptableObject
    {
        public int stageIndex;
        public List<RoundData> rounds;
    }

    [Serializable]
    public struct EnemySpawnData
    {
        public UnitData enemyData;
        public int starLevel;
    }

    [Serializable]
    public struct RoundData
    {
        public int roundIndex;
        public List<EnemySpawnData> enemiesToSpawn;
    }
}