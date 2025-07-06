using UnityEngine;

public class BattleLifecycleManager : MonoBehaviour
{
    public enum GameState {
        NotStarted,
        Selecting,
        Resolving,
        Ending
    }

    public GameState CurrentState = GameState.NotStarted;
    public GameManager gameManager; // 拖入 GameManager 对象

    void Start()
    {
        Debug.Log("[Battle] Lifecycle Manager Initialized. Current State: " + CurrentState);

        // 自动进入 Selecting 状态
        CurrentState = GameState.Selecting;
        Debug.Log("[Battle] Switching to Selecting. Starting Game.");

        gameManager.StartGame(); // 调用 GameManager 的发牌函数
    }

    

    void Update()
    {
        // 可拓展：按下空格推进状态等
    }
}




