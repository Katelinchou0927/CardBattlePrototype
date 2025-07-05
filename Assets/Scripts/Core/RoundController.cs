using UnityEngine;
using System.Collections;

public class RoundController : MonoBehaviour
{
    [Header("回合设置")]
    public float RoundDuration = 5f; // 每个回合持续时间

    // 单例模式
    public static RoundController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 执行回合逻辑
    /// </summary>
    public IEnumerator ExecuteRound()
    {
        Debug.Log("[Round] 开始执行回合");

        // 1. 处理卡牌效果
        yield return ProcessCardActions();

        // 2. 执行攻击和技能

        // 3. 回合结束清理
        yield return CleanUpRound();

        Debug.Log("[Round] 回合执行完成");
    }

    private IEnumerator ProcessCardActions()
    {
        Debug.Log("[Round] 处理卡牌效果...");
        // 这里实现卡牌效果处理逻辑
        yield return new WaitForSeconds(1f);
    }

    

    private IEnumerator CleanUpRound()
    {
        Debug.Log("[Round] 清理回合状态...");
        // 清除临时状态等
        yield return null;
    }

    /// <summary>
    /// 重置回合状态
    /// </summary>
    public void ResetRound()
    {
        Debug.Log("[Round] 回合状态已重置");
    }
}