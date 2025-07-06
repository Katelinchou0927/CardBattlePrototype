using UnityEngine;

public static class BattleLog
{
    public static void Log(string message)
    {
        Debug.Log(message);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddLog(message);
        }
    }
}