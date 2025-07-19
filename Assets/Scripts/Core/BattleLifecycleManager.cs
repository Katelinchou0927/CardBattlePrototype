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

    void Start()
    {
        Debug.Log("[Battle] Lifecycle Manager Initialized. Current State: " + CurrentState);
    }

    void Update()
    {
        // 可扩展：调试用快捷键推进状态
    }
}