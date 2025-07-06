using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    // 挂在EmptyObject "StartManager"下面而非StartButton下面
    // 此方法将在按钮点击时被调用
    public void StartGame()
    {
        // 切换到名为 "MainBattle" 的场景
        SceneManager.LoadScene("MainBattle");
    }
}
