using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class Player : MonoBehaviour
{
    [Header("UI 元素")]
    public TMP_Text nameText;
    public TMP_Text hpText;
    public TMP_Text attackText;
    public Image avatar;
    public GameObject eliminatedOverlay;

    // 玩家属性
    public int PlayerID { get; private set; }
    public string PlayerName { get; private set; }
    public int CurrentHP { get; private set; }
    public int AttackPower { get; private set; }
    public bool IsEliminated { get; private set; }
    public bool HasAttackedThisRound { get; private set; }

    // 新增状态
    public bool IsAttacking { get; private set; }

    // 事件
    public event Action<Player> OnPlayerDamaged;
    public event Action<Player> OnPlayerEliminated;

    // 初始化玩家
    public void Initialize(int id, string name, int hp, int attack)
    {
        PlayerID = id;
        PlayerName = name;
        CurrentHP = hp;
        AttackPower = attack;
        IsEliminated = false;
        HasAttackedThisRound = false;
        IsAttacking = false;

        UpdateUI();
    }

    // 更新UI
    public void UpdateUI()
    {
        if (nameText) nameText.text = PlayerName;
        if (hpText) hpText.text = $"HP: {CurrentHP}";
        if (attackText) attackText.text = $"ATK: {AttackPower}";
        if (eliminatedOverlay) eliminatedOverlay.SetActive(IsEliminated);

        // 根据血量改变血条颜色
        if (hpText)
        {
            hpText.color = CurrentHP > 50 ? Color.green :
                          CurrentHP > 20 ? Color.yellow : Color.red;
        }
    }

    // 开始攻击
    public void StartAttack()
    {
        IsAttacking = true;
        // 这里可以添加攻击动画
    }

    // 结束攻击
    public void EndAttack()
    {
        IsAttacking = false;
    }

    // 受到伤害
    public void TakeDamage(int damage)
    {
        if (IsEliminated) return;

        CurrentHP = Mathf.Max(0, CurrentHP - damage);
        UpdateUI();

        // 触发受伤事件
        OnPlayerDamaged?.Invoke(this);

        // 检查是否被淘汰
        if (CurrentHP <= 0)
        {
            Eliminate();
        }
    }

    // 淘汰玩家
    private void Eliminate()
    {
        IsEliminated = true;
        UpdateUI();
        // 触发淘汰事件
        OnPlayerEliminated?.Invoke(this);
    }

    // 标记为已攻击
    public void MarkAsAttacked()
    {
        HasAttackedThisRound = true;
    }

    // 重置为新一轮
    public void ResetForNewRound()
    {
        HasAttackedThisRound = false;
    }
}