 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class RoundCardSelector : MonoBehaviour
{
[Header("UI References")]
public GameObject cardSelectionPanel;
public Transform cardContainer;
public GameObject cardButtonPrefab;
public Button confirmButton;
public TextMeshProUGUI instructionText;
public TextMeshProUGUI roundInfoText;
private List<int> availableCards = new List<int>();
private int selectedHP = -1;
private int selectedATK = -1;
private List<GameObject> cardButtons = new List<GameObject>();

/// <summary>
/// 显示卡牌选择界面（每回合开始时）
/// </summary>
public IEnumerator ShowCardSelection(PlayerData player, int round)
{
    Debug.Log($"[RoundSelector] Showing card selection for {player.playerName}, Round {round}");
    
    // 设置可用卡牌
    availableCards = new List<int>(player.numberCards);
    selectedHP = -1;
    selectedATK = -1;
    
    // 显示界面
    if (cardSelectionPanel != null)
        cardSelectionPanel.SetActive(true);
    
    // 设置提示文本
    if (roundInfoText != null)
        roundInfoText.text = $"Round {round} - Select Cards";
    
    if (instructionText != null)
        instructionText.text = "Select HP card";
    
    // 创建卡牌按钮
    CreateCardButtons();
    
    // 等待玩家选择
    yield return StartCoroutine(WaitForSelection());
    
    // 隐藏界面
    if (cardSelectionPanel != null)
        cardSelectionPanel.SetActive(false);
    
    // 返回选择结果
    player.currentHP = selectedHP;
    player.currentATK = selectedATK;
    player.numberCards.Remove(selectedHP);
    player.numberCards.Remove(selectedATK);
    
    Debug.Log($"[RoundSelector] {player.playerName} selected: HP={selectedHP}, ATK={selectedATK}");
}

/// <summary>
/// 创建卡牌按钮
/// </summary>
void CreateCardButtons()
{
    // 清除旧按钮
    foreach (var btn in cardButtons)
    {
        Destroy(btn);
    }
    cardButtons.Clear();
    
    // 创建新按钮
    foreach (int cardValue in availableCards)
    {
        GameObject btnObj = Instantiate(cardButtonPrefab, cardContainer);
        Button btn = btnObj.GetComponent<Button>();
        TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        
        if (btnText != null)
            btnText.text = cardValue.ToString();
        
        int value = cardValue; // 避免闭包问题
        btn.onClick.AddListener(() => OnCardSelected(value));
        
        cardButtons.Add(btnObj);
    }
}

/// <summary>
/// 卡牌选择回调
/// </summary>
void OnCardSelected(int value)
{
    if (selectedHP == -1)
    {
        // 选择HP
        selectedHP = value;
        
        if (instructionText != null)
            instructionText.text = "Select ATK card";
        
        // 禁用已选择的按钮
        UpdateButtonStates();
    }
    else if (selectedATK == -1 && value != selectedHP)
    {
        // 选择ATK
        selectedATK = value;
        
        if (instructionText != null)
            instructionText.text = $"HP: {selectedHP}, ATK: {selectedATK}";
        
        // 启用确认按钮
        if (confirmButton != null)
            confirmButton.interactable = true;
        
        UpdateButtonStates();
    }
}

/// <summary>
/// 更新按钮状态
/// </summary>
void UpdateButtonStates()
{
    for (int i = 0; i < cardButtons.Count && i < availableCards.Count; i++)
    {
        Button btn = cardButtons[i].GetComponent<Button>();
        int value = availableCards[i];
        
        if (value == selectedHP)
        {
            // HP卡牌标记为红色
            btn.image.color = Color.red;
            btn.interactable = false;
        }
        else if (value == selectedATK)
        {
            // ATK卡牌标记为蓝色
            btn.image.color = Color.blue;
            btn.interactable = false;
        }
        else if (selectedHP != -1 && selectedATK == -1)
        {
            // 可以选择作为ATK
            btn.interactable = true;
        }
    }
}

/// <summary>
/// 等待选择完成
/// </summary>
IEnumerator WaitForSelection()
{
    // 设置确认按钮
    if (confirmButton != null)
    {
        confirmButton.interactable = false;
        confirmButton.onClick.RemoveAllListeners();
        
        bool confirmed = false;
        confirmButton.onClick.AddListener(() => confirmed = true);
        
        // 等待确认
        while (!confirmed)
        {
            yield return null;
        }
    }
}

/// <summary>
/// AI自动选择卡牌
/// </summary>
public void AISelectCards(PlayerData aiPlayer, int round)
{
    if (aiPlayer.numberCards.Count < 2)
    {
        Debug.LogWarning($"[RoundSelector] {aiPlayer.playerName} doesn't have enough cards!");
        return;
    }
    
    // 简单的AI策略
    List<int> available = new List<int>(aiPlayer.numberCards);
    
    // 根据不同策略选择
    int hp, atk;
    if (aiPlayer.playerID == 1) // 攻击型
    {
        available.Sort((a, b) => b.CompareTo(a)); // 降序
        atk = available[0]; // 最大值作为攻击
        available.RemoveAt(0);
        hp = available[Random.Range(0, Mathf.Min(3, available.Count))]; // 随机选择HP
    }
    else if (aiPlayer.playerID == 2) // 防御型
    {
        available.Sort((a, b) => b.CompareTo(a)); // 降序
        hp = available[0]; // 最大值作为HP
        available.RemoveAt(0);
        atk = available[Random.Range(0, Mathf.Min(3, available.Count))]; // 随机选择攻击
    }
    else // 平衡型
    {
        int index1 = Random.Range(0, available.Count);
        hp = available[index1];
        available.RemoveAt(index1);
        
        int index2 = Random.Range(0, available.Count);
        atk = available[index2];
    }
    
    aiPlayer.currentHP = hp;
    aiPlayer.currentATK = atk;
    aiPlayer.numberCards.Remove(hp);
    aiPlayer.numberCards.Remove(atk);
    
    Debug.Log($"[RoundSelector] {aiPlayer.playerName} selected: HP={hp}, ATK={atk}");
}
}