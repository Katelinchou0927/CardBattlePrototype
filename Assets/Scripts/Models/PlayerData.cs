using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public string playerName;
    public int playerID;
    public bool isHuman;
    public bool isAliveThisRound;
    
    public int currentHP;
    public int currentATK;
    
    public List<int> numberCards = new List<int>();
    public bool hasQ = true;
    
    public int killsThisRound;
    public int totalScore;
    
    public BattlePlayer battlePlayerRef;
    
    // 新增：角色信息
    public int characterIndex;
    public bool hasSkills;
    public string characterName;
    
    public PlayerData(string name, int id, bool human)
    {
        playerName = name;
        playerID = id;
        isHuman = human;
        isAliveThisRound = true;
        
        currentHP = 0;
        currentATK = 0;
        
        // 初始化数字牌（1-10）
        numberCards = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        hasQ = true;
        
        killsThisRound = 0;
        totalScore = 0;
        
        battlePlayerRef = null;
        
        characterIndex = 0;
        hasSkills = false;
        characterName = name;
    }
    
    public void ResetForNewRound()
    {
        isAliveThisRound = true;
        killsThisRound = 0;
    }
}