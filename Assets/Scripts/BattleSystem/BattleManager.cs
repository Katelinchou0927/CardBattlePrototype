using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("战斗设置")]
    public int playerCount = 4;
    public int initialHP = 100;
    public int minAttack = 5;
    public int maxAttack = 20;

    [Header("玩家预设")]
    public GameObject playerPrefab;
    public Transform playerContainer;

    // 战斗状态
    private int currentRound = 0;
    private int currentTurn = 0;
    private List<Player> players = new List<Player>();
    private Player currentAttacker;
    private Player currentTarget;
    private bool isBattleActive = false;
    private int currentAttackerIndex = 0;

    // 事件系统
    public event Action<int> OnRoundStart;
    public event Action<int> OnTurnStart;
    public event Action<Player, Player> OnPlayerAttack;
    public event Action<Player> OnPlayerEliminated;
    public event Action<Player> OnBattleEnd;
    public event Action<Player, Player> OnTargetSelected;
    public event Action<Player, Player, int> OnDamageCalculated;
    public event Action<Player> OnTurnEnd;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        // 等待 UIManager 初始化
        yield return new WaitUntil(() => UIManager.Instance != null);

        InitializeBattle();
        StartBattle();
    }

    // 初始化战斗
    public void InitializeBattle()
    {
        // 创建玩家
        for (int i = 0; i < playerCount; i++)
        {
            GameObject playerObj = Instantiate(playerPrefab, playerContainer);
            Player player = playerObj.GetComponent<Player>();
            player.Initialize(i, $"玩家 {i + 1}", initialHP,
                             UnityEngine.Random.Range(minAttack, maxAttack + 1));

            // 订阅事件（使用重命名后的处理方法）
            player.OnPlayerDamaged += HandlePlayerDamaged;
            player.OnPlayerEliminated += HandlePlayerEliminated;

            players.Add(player);
        }
        // 确保 UIManager 已初始化
        if (UIManager.Instance == null)
        {
            Debug.LogError("BattleManager: UIManager instance is missing!");
            return;
        }

        try
        {
            UIManager.Instance.SetupPlayerUI(players);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"BattleManager: Failed to setup player UI - {e.Message}");
        }

        BattleLog.Log($"战斗初始化完成，共{playerCount}名玩家");
        UIManager.Instance.SetupPlayerUI(players); 
    }

    // 开始战斗
    public void StartBattle()
    {
        if (isBattleActive) return;

        isBattleActive = true;
        StartNewRound();
    }

    // 开始新回合
    private void StartNewRound()
    {
        currentRound++;
        currentTurn = 0;
        currentAttackerIndex = 0;
        UIManager.Instance.UpdateRoundInfo(currentRound, currentTurn);

        // 重置所有玩家状态
        foreach (Player player in players)
        {
            if (!player.IsEliminated)
            {
                player.ResetForNewRound();
            }
        }

        BattleLog.Log($"===== 第 {currentRound} 回合开始 =====");
        OnRoundStart?.Invoke(currentRound);
        StartNextTurn();
    }

    // 开始下一轮
    private void StartNextTurn()
    {
        currentTurn++;
        UIManager.Instance.UpdateRoundInfo(currentRound, currentTurn);

        // 检查游戏是否结束
        if (CheckBattleEnd())
        {
            EndBattle();
            return;
        }

        currentAttacker = GetNextAttackerByIndex();

        if (currentAttacker == null)
        {
            // 没有可攻击的玩家，结束回合
            EndRound();
            return;
        }

        BattleLog.Log($"第 {currentRound} 回合 - 第 {currentTurn} 轮: {currentAttacker.PlayerName} 准备攻击");
        OnTurnStart?.Invoke(currentTurn);

        // 开始分阶段攻击流程
        StartCoroutine(AttackProcess());
    }

    // 索引式获取下一个攻击者
    private Player GetNextAttackerByIndex()
    {
        int startIndex = currentAttackerIndex;
        Player found = null;

        do
        {
            Player player = players[currentAttackerIndex];

            if (!player.IsEliminated && !player.HasAttackedThisRound)
            {
                found = player;
                currentAttackerIndex = (currentAttackerIndex + 1) % players.Count;
                break;
            }

            currentAttackerIndex = (currentAttackerIndex + 1) % players.Count;
        } while (currentAttackerIndex != startIndex);

        return found;
    }

    // 分阶段攻击流程
    private IEnumerator AttackProcess()
    {
        // 阶段1: 选择目标
        SelectTarget();
        OnTargetSelected?.Invoke(currentAttacker, currentTarget);
        BattleLog.Log($"{currentAttacker.PlayerName} 锁定了 {currentTarget.PlayerName}");
        yield return new WaitForSeconds(0.8f);

        // 阶段2: 执行攻击
        currentAttacker.StartAttack();
        int damage = CalculateDamage();
        BattleLog.Log($"{currentAttacker.PlayerName} 发动攻击!");
        OnPlayerAttack?.Invoke(currentAttacker, currentTarget);
        yield return new WaitForSeconds(0.5f);

        // 阶段3: 伤害结算
        currentTarget.TakeDamage(damage);

        UIManager.Instance.UpdatePlayerUI(currentTarget);//添加内容
        BattleLog.Log($"{currentTarget.PlayerName} 受到 {damage} 点伤害（剩余 HP：{currentTarget.CurrentHP}）");


        OnDamageCalculated?.Invoke(currentAttacker, currentTarget, damage);
        currentAttacker.EndAttack();
        currentAttacker.MarkAsAttacked();
        yield return new WaitForSeconds(1f);

        // 阶段4: 淘汰检查
        if (currentTarget.IsEliminated)
        {
            BattleLog.Log($"{currentTarget.PlayerName} 被淘汰!");
            OnPlayerEliminated?.Invoke(currentTarget);
            yield return new WaitForSeconds(1f);
        }

        // 阶段5: 回合结束
        OnTurnEnd?.Invoke(currentAttacker);
        yield return new WaitForSeconds(0.5f);

        StartNextTurn();
    }

    // 伤害计算
    private int CalculateDamage()
    {
        // 基础伤害
        int damage = currentAttacker.AttackPower;

        // 随机浮动 (80%-120%)
        float variance = UnityEngine.Random.Range(0.8f, 1.2f);
        damage = Mathf.RoundToInt(damage * variance);

        return damage;
    }

    // 选择目标
    private void SelectTarget()
    {
        List<Player> possibleTargets = new List<Player>();

        foreach (Player player in players)
        {
            if (!player.IsEliminated && player != currentAttacker)
            {
                possibleTargets.Add(player);
            }
        }

        if (possibleTargets.Count == 0)
        {
            EndRound();
            return;
        }

        // 随机选择目标
        currentTarget = possibleTargets[UnityEngine.Random.Range(0, possibleTargets.Count)];
    }

    // 结束回合
    private void EndRound()
    {
        BattleLog.Log($"===== 第 {currentRound} 回合结束 =====");
        StartNewRound();
    }

    // 检查战斗是否结束
    private bool CheckBattleEnd()
    {
        int alivePlayers = 0;
        Player lastPlayerAlive = null;

        foreach (Player player in players)
        {
            if (!player.IsEliminated)
            {
                alivePlayers++;
                lastPlayerAlive = player;
            }
        }

        if (alivePlayers <= 1)
        {
            if (alivePlayers == 1)
            {
                BattleLog.Log($"战斗结束！胜利者: {lastPlayerAlive.PlayerName}");
                OnBattleEnd?.Invoke(lastPlayerAlive);
                UIManager.Instance.ShowWinner(lastPlayerAlive);
            }
            else
            {
                BattleLog.Log("战斗结束！没有胜利者，所有玩家都被淘汰");
            }
            return true;
        }
        return false;
    }

    // 结束战斗
    private void EndBattle()
    {
        isBattleActive = false;
        BattleLog.Log("===== 战斗结束 =====");
    }

    // 玩家受伤事件处理（重命名）
    private void HandlePlayerDamaged(Player player)
    {
        UIManager.Instance.UpdatePlayerUI(player);
        PlayerUI playerUI = GetPlayerUI(player);
        playerUI?.PlayDamageEffect();
    }

    // 玩家被淘汰事件处理（重命名）
    private void HandlePlayerEliminated(Player player)
    {
        UIManager.Instance.UpdatePlayerUI(player);
        PlayerUI playerUI = GetPlayerUI(player);
        playerUI?.PlayEliminatedEffect();
    }

    // 获取玩家UI
    private PlayerUI GetPlayerUI(Player player)
    {
        foreach (PlayerUI ui in UIManager.Instance.playerUIs)
        {
            if (ui.Player == player)
            {
                return ui;
            }
        }
        return null;
    }
}