using UnityEngine;

public class GlobalInputManager : MonoBehaviour
{
    void Start()
    {
        // 确保这个对象不会被销毁
        DontDestroyOnLoad(gameObject);
    }
    
    void Update()
    {
        // ESC键退出
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
        
        // R键重新开始（可选）
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }
    
    void QuitGame()
    {
        Debug.Log("[GlobalInput] Quitting game...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    void RestartGame()
    {
        Debug.Log("[GlobalInput] Restarting game...");
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}