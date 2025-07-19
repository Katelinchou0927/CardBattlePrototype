using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 战斗界面视图类：关联 UI 组件，提供数据显示方法
public class BattleView : MonoBehaviour
{
    // 顶部玩家模块(下方同理)
    [Header("顶部玩家模块")]
    public Image topPortrait;           // 角色立绘
    public Image topAtkSprite;          // 攻击图标（剑）
    public TMP_Text topAtkNumber;           // 攻击数字
    public Image topHpSprite;           // 血量图标（心）
    public TMP_Text topHpNumber;            // 血量数字
    public Image topDeckArea;           // 牌组位置

    // 下方玩家模块
    [Header("下方玩家模块")]
    public Image bottomPortrait;
    public Image bottomAtkSprite;
    public TMP_Text bottomAtkNumber;
    public Image bottomHpSprite;
    public TMP_Text bottomHpNumber;
    public Image bottomDeckArea;

    // 左侧玩家模块
    [Header("左侧玩家模块")]
    public Image LeftPortrait;
    public Image LeftAtkSprite;
    public TMP_Text LeftAtkNumber;
    public Image LeftHpSprite;
    public TMP_Text LeftHpNumber;
    public Image LeftDeckArea;

    // 右侧玩家模块
    [Header("右侧玩家模块")]
    public Image RightPortrait;
    public Image RightAtkSprite;
    public TMP_Text RightAtkNumber;
    public Image RightHpSprite;
    public TMP_Text RightHpNumber;
    public Image RightDeckArea;


    // 聊天模块
    [Header("聊天模块")]
    public ScrollRect chatScroll;       // 聊天滚动区域
    public TMP_InputField chatInput;        // 聊天输入框
    public Button sendBtn;              // 发送按钮

    // 系统模块
    [Header("系统模块")]
    public Button settingBtn;           // 游戏设置按钮


    // ============= 功能扩展点：血量显示 =============
    /// <summary>
    /// 更新顶部玩家血量（心形图标旁的数字）
    /// 后续开发者可在此添加动画、变色等逻辑
    /// </summary>
    /// <param name="hp">当前血量</param>
    public virtual void UpdateTopHp(int hp)
    {
        topHpNumber.text = hp.ToString();
        // 示例：血量低于5时变红（可忽略，改为其它效果）
        topHpNumber.color = hp < 5 ? Color.red : Color.black;
    }

    // ============= 功能扩展点：攻击力显示 =============
    /// <summary>
    /// 更新顶部玩家攻击力（剑图标旁的数字）
    /// 后续开发者可在此添加动画、变色等逻辑
    /// </summary>
    /// <param name="atk">当前攻击力</param>
    public virtual void UpdateTopAtk(int atk)
    {
        topAtkNumber.text = atk.ToString();
    }

    // ============= 功能扩展点：聊天功能 =============
    /// <summary>
    /// 添加聊天内容到滚动区域
    /// 后续开发者需完善：滚动定位、内容清空、消息发送逻辑
    /// </summary>
    /// <param name="message">聊天内容</param>
    public virtual void AddChatMessage(string message)
    {
        // 临时逻辑：创建文本显示聊天内容（需优化）
        GameObject textObj = new GameObject("ChatText");
        textObj.transform.SetParent(chatScroll.content);
        Text textComp = textObj.AddComponent<Text>();
        textComp.text = message;
        textComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComp.fontSize = 24;
    }

    // ============= 功能扩展点：设置按钮 =============
    /// <summary>
    /// 游戏设置按钮点击事件
    /// 后续开发者在此写弹出设置面板逻辑
    /// </summary>
    public virtual void OnSettingBtnClick()
    {
        Debug.Log("点击了设置按钮，需扩展设置面板逻辑");
    }
}