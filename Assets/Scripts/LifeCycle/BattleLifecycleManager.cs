using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleLifecycleManager : MonoBehaviour
{
    //public bool isRunning;
    public enum BattlePhase
    {
        None,
        Prepare,
        CardSelection,
        RoundExecution,
        JudgeVictory,
        End
    }

    public BattlePhase currentPhase = BattlePhase.None;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RunBattleLifecycle());
    }

    IEnumerator RunBattleLifecycle()
    {
        Debug.Log("生命周期启动");

        yield return StartCoroutine(EnterPreparePhase());

        bool battleOver = false;

        while (!battleOver)
        {
            yield return StartCoroutine(EnterCardSelectionPhase());
            yield return StartCoroutine(EnterRoundExecutionPhase());
            yield return StartCoroutine(EnterJudgeVictoryPhase());

            // 判断战斗是否结束
            battleOver = IsBattleOver();
        }

        yield return StartCoroutine(EnterEndPhase());

    }

    bool IsBattleOver()
    {
        var players = FindObjectsOfType<PlayerManager>();
        int aliveCount = 0;

        foreach (var player in players)
        {
            if (player.isAlive)
                aliveCount++;
        }

        return aliveCount <= 1;
    }


    IEnumerator EnterPreparePhase()
    {
        currentPhase = BattlePhase.Prepare;
        Debug.Log("进入准备阶段");
        yield return new WaitForSeconds(1f); // 模拟等待
    }

    IEnumerator EnterCardSelectionPhase()
    {
        currentPhase = BattlePhase.CardSelection;
        Debug.Log("进入出牌阶段");
        yield return new WaitForSeconds(1f);
    }

    IEnumerator EnterRoundExecutionPhase()
    {
        currentPhase = BattlePhase.RoundExecution;
        Debug.Log("进入执行阶段");
        yield return new WaitForSeconds(1f);
    }

    IEnumerator EnterJudgeVictoryPhase()
    {
        currentPhase = BattlePhase.JudgeVictory;
        Debug.Log("进入判定阶段");

        // 胜负逻辑
        var players = FindObjectsOfType<PlayerManager>();
        int aliveCount = 0;
        PlayerManager winner = null;

        foreach (var player in players)
        {
            if (player.isAlive)
            {
                aliveCount++;
                winner = player;
            }
        }

        if (aliveCount == 1)
        {
            Debug.Log("胜者是：" + winner.name);
        }
        else if (aliveCount == 0)
        {
            Debug.Log("平局");
        }
        else
        {
            Debug.Log("仍有多人存活，进入下一回合");
        }

        yield return new WaitForSeconds(1f);
    }

    IEnumerator EnterEndPhase()
    {
        currentPhase = BattlePhase.End;
        Debug.Log("进入结束阶段");
        yield return null;
    }
    public void RestartBattle()
    {

        Debug.Log("战斗重启！");
        StopAllCoroutines(); // 停止当前生命周期     
        StartCoroutine(RunBattleLifecycle()); // 重新开始
    }

    // Update is called once per frame
    void Update()
    {

        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    RestartBattle();  // 按键重启
        //}
    }
}