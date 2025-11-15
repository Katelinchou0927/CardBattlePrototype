using UnityEngine;

[System.Serializable]
public class BattlePlayer
{
    public string name;
    public int maxHP;
    public int currentHP;
    public int attack;
    public bool isEliminated;
    
    // 新增：角色信息
    public int characterIndex;        // 角色索引（0-3）
    public bool hasSkills;            // 是否有技能
    public Sprite characterArt;       // 角色图片（可选）

    public BattlePlayer(string playerName, int hp, int atk)
    {
        name = playerName;
        maxHP = hp;
        currentHP = hp;
        attack = atk;
        isEliminated = false;
        characterIndex = 0;
        hasSkills = false;
        characterArt = null;
    }
    
    // 新增：带角色信息的构造函数
    public BattlePlayer(string playerName, int hp, int atk, int charIndex, bool skills = false)
    {
        name = playerName;
        maxHP = hp;
        currentHP = hp;
        attack = atk;
        isEliminated = false;
        characterIndex = charIndex;
        hasSkills = skills;
        characterArt = null;
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

    public void UpdateHP(int newHP)
    {
        currentHP = newHP;
        maxHP = Mathf.Max(maxHP, newHP);
        if (currentHP > 0)
        {
            isEliminated = false;
        }
    }

    public void UpdateAttack(int newAttack)
    {
        attack = newAttack;
    }
}