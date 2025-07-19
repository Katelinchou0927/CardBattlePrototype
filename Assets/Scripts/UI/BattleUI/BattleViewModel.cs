using UnityEngine;

// 战斗界面对应的视图模型：处理交互逻辑，对接游戏数据
public class BattleViewModel : MonoBehaviour
{
    // 关联战斗视图（需在 Unity 中赋值）
    public BattleView battleView;

    private void Awake()
    {
        // 绑定按钮事件（示例）
        battleView.settingBtn.onClick.AddListener(OnSettingBtnClicked);
        battleView.sendBtn.onClick.AddListener(OnSendBtnClicked);
    }


    // ============= 数据对接点：更新血量 =============
    /// <summary>
    /// 接收游戏数据，更新顶部玩家血量
    /// 后续开发者需在此监听游戏数据变化（如网络同步、战斗逻辑）
    /// </summary>
    /// <param name="hp">当前血量</param>
    public void UpdateTopHp(int hp)
    {
        battleView.UpdateTopHp(hp);
    }

    // ============= 数据对接点：更新攻击力 =============
    /// <summary>
    /// 接收游戏数据，更新顶部玩家攻击力
    /// 后续开发者需在此监听游戏数据变化
    /// </summary>
    /// <param name="atk">当前攻击力</param>
    public void UpdateTopAtk(int atk)
    {
        battleView.UpdateTopAtk(atk);
    }

    // ============= 数据对接点：接收聊天消息 =============
    /// <summary>
    /// 接收聊天消息（来自网络/本地输入），显示到界面
    /// 后续开发者需在此对接聊天系统
    /// </summary>
    /// <param name="message">聊天内容</param>
    public void ReceiveChatMessage(string message)
    {
        battleView.AddChatMessage(message);
    }


    // ============= 内部交互逻辑 =============
    /// <summary>
    /// 发送聊天按钮点击（临时逻辑，需对接网络）
    /// </summary>
    private void OnSendBtnClicked()
    {
        string message = battleView.chatInput.text;
        if (!string.IsNullOrEmpty(message))
        {
            Debug.Log($"发送聊天：{message}");
            battleView.AddChatMessage($"你：{message}"); // 临时显示自己的消息
            battleView.chatInput.text = ""; // 清空输入框

            // 后续开发者需在此添加：网络发送消息逻辑
        }
    }

    /// <summary>
    /// 游戏设置按钮点击（临时逻辑，需对接设置系统）
    /// </summary>
    private void OnSettingBtnClicked()
    {
        battleView.OnSettingBtnClick();
        // 后续开发者需在此添加：打开设置面板逻辑
    }
}