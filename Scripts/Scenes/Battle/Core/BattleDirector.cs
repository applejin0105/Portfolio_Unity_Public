using System.Collections.Generic;
using Scenes.Battle.Board;
using Scenes.Battle.Cameras;
using Scenes.Battle.Controller;
using Scenes.Battle.Data;
using Scenes.Battle.Entity;
using Scenes.Battle.Shop;
using Scenes.Battle.UI;
using UnityEngine;

namespace Scenes.Battle.Core
{
    public class BattleDirector : MonoBehaviour
    {
        [Header("System Managers")]
        public BattleManager battleManager;
        public PlayerManager playerManager;
        public FieldManager fieldManager;
        public ShopManager shopManager;
        public MergeManager mergeManager;
        public HandManager handManager;
        public GameOverUI gameOverUI;
        public CameraController cameraController;
        public BattleSceneUIController battleSceneUIController;

        [Header("Rosters")]
        public List<Character> players = new();
        public List<Character> enemies = new();

        [Header("Round & Spawning")]
        public RoundData currentRoundData;
        public GameObject enemyPrefab;
        [SerializeField] private int currentRoundIndex;
        [SerializeField] private Transform[] enemySlots;
        private TargetingResolver _resolver;
        public int CurrentRoundIndex => currentRoundIndex;

        private bool _isGameOver = false;

        [Header("Statistics")]
        public int TotalAlliesDead { get; private set; }

        public int TotalEnemiesKilled { get; private set; }

        #region Initialization

        private void Awake()
        {
            _resolver = new TargetingResolver();
        }

        private void Start()
        {
            _isGameOver = false;
            currentRoundIndex = 0;
            SpawnEnemies();
        }

        private void OnEnable()
        {
            if (battleManager == null) return;
            battleManager.OnTurnEnded += CheckTurnResult;
        }

        private void OnDisable()
        {
            if (battleManager != null) return;
            battleManager.OnTurnEnded -= CheckTurnResult;
        }

        #endregion

        #region Battle Flow Control

        public bool CanStartCombat(int currentCost, int minUnitCost, int handUnitCount)
        {
            if (fieldManager != null)
            {
                players.Clear();
                players.AddRange(fieldManager.GetActivePlayers());
            }

            if (players.Count > 0 && enemies.Count > 0) return true;

            if (players.Count == 0 && handUnitCount == 0 && currentCost < minUnitCost)
            {
                TriggerGameOver();
                return false;
            }

            Debug.LogWarning($"[BattleDirector] 필드에 캐릭터를 배치해야 합니다.");

            return false;
        }

        private void TriggerGameOver()
        {
            if (_isGameOver) return;
            _isGameOver = true;

            Debug.Log($"[BattleDirector] 더 이상 기물을 구매하거나 배치할 수 없습니다.");

            battleManager.SetBattleState(BattleState.Idle);

            battleSceneUIController.PlayGameOverRoutine();

            if (gameOverUI != null)
            {
                gameOverUI.ShowResult(
                    currentRoundIndex + 1,
                    TotalAlliesDead,
                    TotalEnemiesKilled,
                    playerManager.TotalCostEarned,
                    playerManager.TotalCostSpent
                );
            }
        }

        public void StartCombatPhase()
        {
            foreach (var p in players)
                if (p != null)
                    p.HideInfoUI();
            foreach (var e in enemies)
                if (e != null)
                    e.HideInfoUI();

            var turnBattles = _resolver.ResolveTurn(players, enemies);
            battleManager.ExecuteTurn(turnBattles);
        }

        private void CheckTurnResult()
        {
            Debug.Log("Turn End");
            var playerSurvivors = 0;
            var enemiesSurvivors = 0;

            // 아군 결산
            for (var i = players.Count - 1; i >= 0; i--)
            {
                var character = players[i];
                if (character != null)
                {
                    if (character.Hp > 0)
                    {
                        character.SetInfoUI();
                        character.ShowInfoUI();
                        playerSurvivors++;
                    }
                    else
                    {
                        if (fieldManager != null && character.currentUnitData != null)
                            fieldManager.RemoveFromField(character.currentUnitData);

                        players.RemoveAt(i);
                        TotalAlliesDead++;

                        foreach (var ally in players)
                            if (ally != null)
                                ally.ChangeSp(-15);
                    }
                }
            }

            // 적군 결산
            for (var i = enemies.Count - 1; i >= 0; i--)
            {
                var character = enemies[i];
                if (character != null)
                {
                    if (character.Hp > 0)
                    {
                        character.SetInfoUI();
                        character.ShowInfoUI();
                        enemiesSurvivors++;
                    }
                    else
                    {
                        if (playerManager != null) playerManager.ChangeCost(10, out _);
                        Destroy(character.gameObject);
                        enemies.RemoveAt(i);
                        TotalEnemiesKilled++;

                        foreach (var ally in players)
                            if (ally != null)
                                ally.ChangeSp(10);
                    }
                }
            }

            // 결과 판정 분기
            if (playerSurvivors == 0)
            {
                Debug.Log("[BattleDirector] Defeat! 게임 종료.");
                TriggerGameOver();
            }
            else if (enemiesSurvivors == 0)
            {
                Debug.Log("[BattleDirector] Win! (라운드 클리어)");
                foreach (var character in players)
                    if (character != null)
                    {
                        character.SetupUnit(character.currentUnitData, character.StarLevel);
                        character.SetInfoUI();
                    }

                currentRoundIndex++;
                if (currentRoundData != null && currentRoundIndex >= currentRoundData.rounds.Count)
                    currentRoundIndex = currentRoundData.rounds.Count - 1;

                ResetState();
                TurnEnd();
                SpawnEnemies(); // 턴 결산 및 상태 초기화 완료 후 새로운 적 스폰
            }
            else
            {
                Debug.Log("[BattleDirector] Not yet! (전투 지연)");
                ResetState();
                TurnEnd();
            }
        }

        #endregion

        #region Turn End & Economy

        public void TurnEnd()
        {
            var currentCost = playerManager.CurrentCost;
            var interest = currentCost switch
            {
                > 50 => 5,
                > 40 => 4,
                > 30 => 3,
                > 20 => 2,
                > 10 => 1,
                _ => 0
            };

            playerManager.ChangeCost(interest + 5, out _);
            shopManager.RerollShop();
        }

        private void ResetState()
        {
            battleManager.SetBattleState(BattleState.Idle);

            // 카메라 초기 위치 복귀 로직 추가
            if (cameraController != null)
            {
                cameraController.SetInitPosition();
            }

            foreach (var p in players)
                if (p != null)
                    p.ResetStateAndPosition();
            mergeManager.ProcessQueue();
        }

        private void SpawnEnemies()
        {
            if (currentRoundData == null || currentRoundData.rounds == null) return;
            if (currentRoundIndex >= currentRoundData.rounds.Count) return;

            var currentRound = currentRoundData.rounds[currentRoundIndex];

            for (var i = 0; i < currentRound.enemiesToSpawn.Count; i++)
            {
                if (i >= enemySlots.Length) break;

                var spawnData = currentRound.enemiesToSpawn[i];
                if (spawnData.enemyData == null) continue;

                var spawnSlot = enemySlots[i];
                var enemyObj = Instantiate(enemyPrefab, spawnSlot.position, Quaternion.identity, spawnSlot);
                var enemyCharacter = enemyObj.GetComponent<Character>();

                if (enemyCharacter != null)
                {
                    enemyCharacter.SetupUnit(spawnData.enemyData, spawnData.starLevel);
                    enemyCharacter.HomePosition = spawnSlot.position;
                    enemyCharacter.SetInfoUI();
                    enemyCharacter.ShowInfoUI();
                    enemies.Add(enemyCharacter);
                }
            }
        }

        #endregion
    }
}