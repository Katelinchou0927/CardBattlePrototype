using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(PlayerManager)), RequireComponent(typeof(RoundController))]
public class BattleLifecycleController : MonoBehaviour
{
    // ========== 枚举定义 ==========
    public enum BattlePhase
    {
        Preparation,    // 战斗准备阶段
        CardSelection,  // 卡牌选择阶段
        RoundExecution, // 回合执行阶段
        Judgment,      // 胜负判定阶段
        Ended          // 战斗结束阶段
    }

    // ========== 事件定义 ==========
    public event Action onBattleStart;
    public event Action onCardSelectionStart;
    public event Action onCardSelectionEnd;
    public event Action onRoundExecutionStart;
    public event Action onRoundExecutionEnd;
    public event Action<BattleResult> onBattleEnd;

    // ========== 公开配置 ==========
    [Header("阶段设置")]
    [Tooltip("准备阶段持续时间(秒)")]
    public float preparationDuration = 2f;

    [Tooltip("卡牌选择阶段超时时间(秒)")]
    public float cardSelectionTimeout = 30f;

    [Tooltip("战斗结束显示时间(秒)")]
    public float battleEndDisplayDuration = 3f;

    // ========== 运行时状态 ==========
    private BattlePhase _currentPhase;
    private bool _isPaused;
    private Coroutine _battleFlowCoroutine;
    private int _currentRound = 0; // 新增：当前回合计数

    // ========== 组件依赖 ==========
    private PlayerManager _playerManager;
    private RoundController _roundController;
    

    // ========== 属性访问器 ==========
    public BattlePhase CurrentPhase => _currentPhase;
    public bool IsPaused => _isPaused;
    public static BattleLifecycleController Instance { get; private set; }
    public int CurrentRound => _currentRound; // 新增：公开回合计数

    // ========== Unity生命周期 ==========
    private void Awake()
    {
        // 单例模式初始化
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 获取依赖组件
        _playerManager = GetComponent<PlayerManager>();
        _roundController = GetComponent<RoundController>();
        
    }

    private void Start()
    {
        InitializeBattle();
    }

    // ========== 公开控制方法 ==========
    /// <summary>
    /// 初始化并开始战斗
    /// </summary>
    public void InitializeBattle()
    {
        Debug.Log("[Battle] 初始化战斗系统");

        // 停止现有战斗流程
        if (_battleFlowCoroutine != null)
        {
            StopCoroutine(_battleFlowCoroutine);
        }

        // 重置所有状态
        ResetAllStates();

        // 开始新的战斗流程
        _battleFlowCoroutine = StartCoroutine(BattleFlow());
    }

    /// <summary>
    /// 完全重置战斗状态
    /// </summary>
    public void ResetAllStates()
    {
        _currentPhase = BattlePhase.Preparation;
        _isPaused = false;
        _currentRound = 0;

        // 重置所有子系统
        _playerManager.ResetAllPlayers();
        _roundController.ResetRound();


        Debug.Log("[Battle] 所有状态已重置");
    }

    /// <summary>
    /// 重启战斗（用于"再来一局"）
    /// </summary>
    public void RestartBattle()
    {
        Debug.Log("[Battle] 重启战斗");
        InitializeBattle();
    }
    private IEnumerator EnterPreparationPhase()
    {
        // 1. 设置当前阶段标识
        _currentPhase = BattlePhase.Preparation;
        Debug.Log("[Battle] === 进入准备阶段 ===");

        // 2. 初始化玩家数据
        _playerManager.InitializePlayers();

        // 3. 广播战斗开始事件（其他模块可监听）
        onBattleStart?.Invoke();

        // 4. 等待准备时间（可显示倒计时UI等）
        yield return new WaitForSeconds(preparationDuration);

        // 协程结束后自动进入下一阶段
    }
    private IEnumerator EnterCardSelectionPhase()
    {
        _currentPhase = BattlePhase.CardSelection;
        Debug.Log("[Battle] === 进入卡牌选择阶段 ===");

        // 广播阶段开始事件
        onCardSelectionStart?.Invoke();
        yield return null;

        // 广播阶段结束事件
        onCardSelectionEnd?.Invoke();
    }
    private IEnumerator EnterRoundExecutionPhase()
    {
        _currentPhase = BattlePhase.RoundExecution;
        Debug.Log("[Battle] === 进入回合执行阶段 ===");

        // 广播阶段开始事件
        onRoundExecutionStart?.Invoke();

        // 执行回合逻辑
        yield return null;
        // 广播阶段结束事件
        onRoundExecutionEnd?.Invoke();
    }

    private IEnumerator EnterJudgmentPhase()
    {
        _currentPhase = BattlePhase.Judgment;
        Debug.Log("[Battle] === 进入胜负判定阶段 ===");

        yield return null;
    }

    // ========== 战斗核心流程 ==========
    private IEnumerator BattleFlow()
    {
        // 1. 准备阶段
        yield return EnterPreparationPhase();

        // 2. 主战斗循环
        while (_currentPhase != BattlePhase.Ended)
        {
            // 卡牌选择阶段
            yield return EnterCardSelectionPhase();

            // 回合执行阶段
            yield return EnterRoundExecutionPhase();

            // 胜负判定阶段
            yield return EnterJudgmentPhase();

            _currentRound++; // 回合数增加
        }
    }

    private IEnumerator EnterBattleEndPhase(BattleResult result)
    {
        _currentPhase = BattlePhase.Ended;
        Debug.Log($"[Battle] === 战斗结束 === 结果: {result}");

        // 广播战斗结束事件
        onBattleEnd?.Invoke(result);

        // 等待结果展示时间
        yield return new WaitForSeconds(battleEndDisplayDuration);

        // 自动重置并开始新一轮
        RestartBattle();
    }

    // ========== 新增功能 ==========
    /// <summary>
    /// 强制结束当前战斗（用于调试或特殊情况下）
    /// </summary>
 

    /// <summary>
    /// 获取当前存活玩家数量
    /// </summary>
    public int GetAlivePlayerCount()
    {
        return _playerManager.GetAlivePlayers().Count;
    }
}

/// <summary>
/// 战斗结果数据结构
/// </summary>
public struct BattleResult
{
    public bool isBattleEnded;
    public Player winner;
    public bool isDraw;

    public override string ToString()
    {
        if (isDraw)
            return "平局";
        return winner != null ? $"玩家 {winner.PlayerName} 胜利" : "战斗未结束";
    }
}