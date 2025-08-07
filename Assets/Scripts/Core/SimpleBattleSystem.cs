using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BattlePlayer
{
    public string name;
    public int maxHP;
    public int currentHP;
    public int attack;
    public bool isEliminated;

    public BattlePlayer(string playerName, int hp, int atk)
    {
        name = playerName;
        maxHP = hp;
        currentHP = hp;
        attack = atk;
        isEliminated = false;
    }

    public void TakeDamage(int damage)
    {
        currentHP = Mathf.Max(0, currentHP - damage);
        if (currentHP <= 0)
        {
            isEliminated = true;
        }
        Debug.Log($"{name} takes {damage} damage, remaining HP: {currentHP}");
    }

    public bool IsAlive()
    {
        return !isEliminated && currentHP > 0;
    }
}

public class SimpleBattleSystem : MonoBehaviour
{
    [Header("Battle Settings")]
    public int aiPlayerCount = 3;
    public int baseHP = 50;
    public int baseAttack = 10;

    [Header("Battle State")]
    public int currentRound = 1;
    public int currentTurn = 0;

    [Header("UI References")]
    public BattleUIManager uiManager;

    private List<BattlePlayer> players = new List<BattlePlayer>();
    private BattlePlayer humanPlayer;
    private int currentPlayerIndex = 0;
    private bool battleActive = false;

    public void InitializeBattle(int playerHP, int playerATK)
    {
        Debug.Log($"[SimpleBattle] Initializing battle with player stats: HP={playerHP}, ATK={playerATK}");
        
        players.Clear();
        
        // Create human player
        humanPlayer = new BattlePlayer("You", playerHP, playerATK);
        players.Add(humanPlayer);

        // Create AI players
        for (int i = 0; i < aiPlayerCount; i++)
        {
            int aiHP = Random.Range(baseHP - 10, baseHP + 11);
            int aiATK = Random.Range(baseAttack - 3, baseAttack + 4);
            BattlePlayer aiPlayer = new BattlePlayer($"AI Player {i + 1}", aiHP, aiATK);
            players.Add(aiPlayer);
        }

        Debug.Log($"[SimpleBattle] Created {players.Count} players for battle");
        
        // Log all players
        foreach (var player in players)
        {
            Debug.Log($"[SimpleBattle] Player: {player.name}, HP: {player.currentHP}, ATK: {player.attack}");
        }

        // Initialize UI
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<BattleUIManager>();
        }
        
        if (uiManager != null)
        {
            uiManager.InitializePlayers(players);
        }
        else
        {
            Debug.LogError("[SimpleBattle] BattleUIManager not found!");
        }

        battleActive = true;
        currentRound = 1;
        currentTurn = 0;
        currentPlayerIndex = 0;

        StartCoroutine(BattleLoop());
    }

    IEnumerator BattleLoop()
    {
        while (battleActive)
        {
            yield return StartCoroutine(ExecuteRound());
            
            if (CheckBattleEnd())
            {
                battleActive = false;
                break;
            }

            currentRound++;
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator ExecuteRound()
    {
        Debug.Log($"=== Round {currentRound} Start ===");
        
        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateRoundInfo(currentRound, 0);
            uiManager.AddBattleLog($"=== Round {currentRound} Start ===");
        }
        
        // Reset turn counter
        currentTurn = 0;
        currentPlayerIndex = 0;

        // Each player gets a turn
        for (int i = 0; i < players.Count; i++)
        {
            BattlePlayer attacker = players[i];
            
            if (!attacker.IsAlive())
                continue;

            currentTurn++;
            Debug.Log($"Round {currentRound}, Turn {currentTurn}: {attacker.name}'s turn");
            
            // Update UI
            if (uiManager != null)
            {
                uiManager.UpdateRoundInfo(currentRound, currentTurn);
            }

            // Select target
            BattlePlayer target = SelectRandomTarget(attacker);
            if (target == null)
            {
                Debug.Log($"No valid target for {attacker.name}");
                continue;
            }

            // Execute attack with UI animation
            yield return StartCoroutine(ExecuteAttack(attacker, target));
            yield return new WaitForSeconds(1f);

            // Check if battle should end after this attack
            if (CheckBattleEnd())
                break;
        }

        Debug.Log($"=== Round {currentRound} End ===");
        if (uiManager != null)
        {
            uiManager.AddBattleLog($"=== Round {currentRound} End ===");
        }
    }

    IEnumerator ExecuteAttack(BattlePlayer attacker, BattlePlayer target)
    {
        Debug.Log($"{attacker.name} attacks {target.name}!");
        
        // Show attack animation in UI
        if (uiManager != null)
        {
            uiManager.ShowAttackAnimation(attacker, target);
        }
        
        // Calculate damage with some variance
        int baseDamage = attacker.attack;
        float variance = Random.Range(0.8f, 1.2f);
        int finalDamage = Mathf.RoundToInt(baseDamage * variance);

        yield return new WaitForSeconds(0.5f);

        target.TakeDamage(finalDamage);

        // Show damage animation in UI
        if (uiManager != null)
        {
            uiManager.ShowDamageAnimation(target, finalDamage);
            uiManager.UpdateAllPlayerCards();
        }

        if (target.isEliminated)
        {
            Debug.Log($"{target.name} has been eliminated!");
        }

        yield return new WaitForSeconds(0.5f);
    }

    BattlePlayer SelectRandomTarget(BattlePlayer attacker)
    {
        List<BattlePlayer> possibleTargets = new List<BattlePlayer>();

        foreach (var player in players)
        {
            if (player != attacker && player.IsAlive())
            {
                possibleTargets.Add(player);
            }
        }

        if (possibleTargets.Count == 0)
            return null;

        return possibleTargets[Random.Range(0, possibleTargets.Count)];
    }

    bool CheckBattleEnd()
    {
        List<BattlePlayer> alivePlayers = new List<BattlePlayer>();
        
        foreach (var player in players)
        {
            if (player.IsAlive())
            {
                alivePlayers.Add(player);
            }
        }

        if (alivePlayers.Count <= 1)
        {
            BattlePlayer winner = alivePlayers.Count == 1 ? alivePlayers[0] : null;
            
            if (winner != null)
            {
                Debug.Log($"Battle End! Winner: {winner.name}");
                
                if (winner == humanPlayer)
                {
                    Debug.Log("Congratulations! You won the battle!");
                }
                else
                {
                    Debug.Log("You were defeated. Better luck next time!");
                }
            }
            else
            {
                Debug.Log("Battle End! No survivors - it's a draw!");
            }
            
            // Show battle end in UI
            if (uiManager != null)
            {
                uiManager.ShowBattleEnd(winner);
            }
            
            return true;
        }

        return false;
    }

    // Public method to get battle status
    public string GetBattleStatus()
    {
        if (!battleActive)
            return "Battle not active";

        string status = $"Round {currentRound}, Turn {currentTurn}\n";
        foreach (var player in players)
        {
            if (player.IsAlive())
            {
                status += $"{player.name}: {player.currentHP}/{player.maxHP} HP\n";
            }
            else
            {
                status += $"{player.name}: ELIMINATED\n";
            }
        }
        return status;
    }
}