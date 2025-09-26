using UnityEngine;
/// <summary>
/// 战斗玩家数据类（供UI显示使用）
/// </summary>
[System.Serializable]
public class BattlePlayer
{
    public string name;
    public int maxHP;
    public int currentHP;
    public int attack;
    public bool isEliminated;
    public BattlePlayer(string playerName, int hp, int atk)
    {
        name = playerName;
        maxHP = hp;
        currentHP = hp;
        attack = atk;
        isEliminated = false;
    }

    public void TakeDamage(int damage)
    {
        currentHP = Mathf.Max(0, currentHP - damage);
        if (currentHP <= 0)
        {
            isEliminated = true;
        }
        Debug.Log($"{name} takes {damage} damage, remaining HP: {currentHP}");
    }

    public bool IsAlive()
    {
        return !isEliminated && currentHP > 0;
    }

    /// <summary>
    /// 更新血量（用于回合间恢复等情况）
    /// </summary>
    public void UpdateHP(int newHP)
    {
        currentHP = newHP;
        maxHP = Mathf.Max(maxHP, newHP);
        if (currentHP > 0)
        {
            isEliminated = false;
        }
    }

    /// <summary>
    /// 更新攻击力
    /// </summary>
    public void UpdateAttack(int newAttack)
    {
        attack = newAttack;
    }
}