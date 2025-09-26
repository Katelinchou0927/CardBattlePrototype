using System.Collections.Generic;
using UnityEngine;

public class AICardSelector : MonoBehaviour
{
    [System.Serializable]
    public class AICardSelection
    {
        public int hpCard;
        public int atkCard;
        public string playerName;
        
        public AICardSelection(string name, int hp, int atk)
        {
            playerName = name;
            hpCard = hp;
            atkCard = atk;
        }
    }

    /// <summary>
    /// 为AI玩家生成卡牌选择
    /// </summary>
    public static AICardSelection GenerateAISelection(string playerName, int aiIndex = 0)
    {
        // 创建1-10的可用卡牌
        List<int> availableCards = new List<int>();
        for (int i = 1; i <= 10; i++)
        {
            availableCards.Add(i);
        }

        int selectedHP = 0;
        int selectedATK = 0;

        // 根据AI索引选择不同策略
        if (aiIndex == 0) // 攻击型AI
        {
            availableCards.Sort((a, b) => b.CompareTo(a)); // 降序
            selectedATK = availableCards[0]; // 最大值作为攻击
            availableCards.Remove(selectedATK);
            selectedHP = availableCards[Random.Range(0, Mathf.Min(3, availableCards.Count))];
        }
        else if (aiIndex == 1) // 防御型AI
        {
            availableCards.Sort((a, b) => b.CompareTo(a)); // 降序
            selectedHP = availableCards[0]; // 最大值作为HP
            availableCards.Remove(selectedHP);
            selectedATK = availableCards[Random.Range(0, Mathf.Min(3, availableCards.Count))];
        }
        else // 平衡型或随机型AI
        {
            selectedHP = availableCards[Random.Range(0, availableCards.Count)];
            availableCards.Remove(selectedHP);
            selectedATK = availableCards[Random.Range(0, availableCards.Count)];
        }

        AICardSelection selection = new AICardSelection(playerName, selectedHP, selectedATK);
        
        Debug.Log($"[AI] {playerName} selected: HP = {selectedHP}, ATK = {selectedATK}");
        
        return selection;
    }
}