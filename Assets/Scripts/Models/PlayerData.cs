using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public string playerName;
    public int playerID;
    public bool isHuman;
    
    // 手牌
    public List<int> numberCards = new List<int>();
    public bool hasJ = true;
    public bool hasQ = true;
    public bool hasK = true;

    // 当前回合状态
    public int currentHP;
    public int currentATK;
    public bool isAliveThisRound = true;
    public bool hasAttackedThisRound = false;

    // 游戏统计
    public int totalScore = 0;
    public int killsThisRound = 0;

    // UI关联
    public BattlePlayer battlePlayerRef; // 添加对BattlePlayer的引用

    public PlayerData(string name, int id, bool human = false)
    {
        playerName = name;
        playerID = id;
        isHuman = human;
        InitializeCards();
    }

    void InitializeCards()
    {
        numberCards.Clear();
        for (int i = 1; i <= 10; i++)
        {
            numberCards.Add(i);
        }
    }

    public void ResetForNewGame()
    {
        InitializeCards();
        hasJ = true;
        hasQ = true;
        hasK = true;
        totalScore = 0;
    }

    public void ResetForNewRound()
    {
        isAliveThisRound = true;
        hasAttackedThisRound = false;
        killsThisRound = 0;
        currentHP = 0;
        currentATK = 0;
    }
}