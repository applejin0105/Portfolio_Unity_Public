using System.Collections;
using Scenes.Battle.Data;
using Scenes.Battle.Entity;
using UnityEngine;

namespace Scenes.Battle.Logic
{
    public abstract class CoinActionLogic : ScriptableObject
    {
        public abstract IEnumerator Execute(Character attacker, Character target, Coin coinData, int totalDamage,
            bool isFocusSequence);
    }
}