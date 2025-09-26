using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject startButton; // 直接引用StartButton
    public CharacterSelectionUI characterSelectionUI;

    void Start()
    {
        // 确保开始时显示开始按钮
        ShowStartButton();
    }

    /// <summary>
    /// 显示开始按钮
    /// </summary>
    void ShowStartButton()
    {
        if (startButton != null)
            startButton.SetActive(true);
            
        // 确保角色选择界面隐藏
        if (characterSelectionUI != null)
            characterSelectionUI.HideCharacterSelection();
    }

    /// <summary>
    /// 开始游戏按钮点击事件 - 显示角色选择
    /// </summary>
    public void StartGame()
    {
        Debug.Log("[StartMenu] Start Game clicked - showing character selection");
        
        // 隐藏开始按钮
        if (startButton != null)
            startButton.SetActive(false);
            
        // 显示角色选择界面
        if (characterSelectionUI != null)
        {
            characterSelectionUI.ShowCharacterSelection();
        }
        else
        {
            Debug.LogError("[StartMenu] CharacterSelectionUI reference not assigned!");
            // 备用方案：直接进入游戏
            SceneManager.LoadScene("MainBattle");
        }
    }

    /// <summary>
    /// 返回开始界面（从角色选择界面）
    /// </summary>
    public void BackToStartMenu()
    {
        ShowStartButton();
    }

    /// <summary>
    /// 退出游戏按钮点击事件
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[StartMenu] Quit Game clicked");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    /// <summary>
    /// 获取选择的角色索引
    /// </summary>
    public static int GetSelectedCharacterIndex()
    {
        return PlayerPrefs.GetInt("SelectedCharacterIndex", 0);
    }

    /// <summary>
    /// 检查选择的角色是否有技能
    /// </summary>
    public static bool SelectedCharacterHasSkills()
    {
        return PlayerPrefs.GetInt("HasSkills", 0) == 1;
    }
}