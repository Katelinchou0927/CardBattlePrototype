using UnityEngine;

// 战斗 UI 管理器：负责加载战斗界面，提供数据对接入口
public class BattleUIManager : Singleton<BattleUIManager>
{
    // 战斗界面预制体路径（需放到 Resources 文件夹）
    private const string BATTLE_UI_PATH = "Prefabs/BattleUI/BattleUIRoot";

    /// <summary>
    /// 显示战斗界面（从 Resources 加载预制体）
    /// </summary>
    public void ShowBattleUI()
    {
        // 假设已有 UIManager 负责加载（若没有，需先实现）
        UIManager.Instance.ShowUI("BattleUI", BATTLE_UI_PATH);
    }

    /// <summary>
    /// 获取战斗视图模型（方便其他系统调用数据接口）
    /// </summary>
    /// <returns>战斗视图模型</returns>
    public BattleViewModel GetBattleViewModel()
    {
        GameObject battleUI = GameObject.Find("BattleUI");
        return battleUI ? battleUI.GetComponent<BattleViewModel>() : null;
    }
}

// 单例基类（若已有可复用，确保与项目框架一致）
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}