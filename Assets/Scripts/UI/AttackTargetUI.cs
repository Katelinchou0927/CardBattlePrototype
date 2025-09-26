using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AttackTargetUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject targetSelectionPanel;
    public TextMeshProUGUI promptText;
    public Transform targetButtonsContainer;
    public GameObject targetButtonPrefab;
    public Button confirmButton;
    public float selectionTimeLimit = 10f;

    private List<PlayerData> availableTargets = new List<PlayerData>();
    private PlayerData selectedTarget = null;
    private bool waitingForSelection = false;
    private bool selectionMade = false;
    private List<GameObject> targetButtons = new List<GameObject>();

    void Awake()
    {
        // 初始隐藏面板
        if (targetSelectionPanel != null)
            targetSelectionPanel.SetActive(false);
    }

    /// <summary>
    /// 显示攻击目标选择界面
    /// </summary>
    public IEnumerator ShowTargetSelection(PlayerData attacker, List<PlayerData> possibleTargets)
    {
        Debug.Log($"[AttackTargetUI] Showing target selection for {attacker.playerName}");
        
        availableTargets = possibleTargets;
        selectedTarget = null;
        waitingForSelection = true;
        selectionMade = false;

        // 显示面板
        if (targetSelectionPanel != null)
            targetSelectionPanel.SetActive(true);

        // 设置提示文本
        if (promptText != null)
            promptText.text = $"{attacker.playerName}, choose your target!";

        // 创建目标按钮
        CreateTargetButtons();

        // 设置确认按钮
        if (confirmButton != null)
        {
            confirmButton.interactable = false;
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmSelection);
        }

        // 等待选择（带超时）
        float timer = 0f;
        while (timer < selectionTimeLimit && waitingForSelection && !selectionMade)
        {
            timer += Time.deltaTime;
            
            // 更新倒计时显示
            if (promptText != null)
            {
                int remainingTime = Mathf.CeilToInt(selectionTimeLimit - timer);
                promptText.text = $"{attacker.playerName}, choose your target! ({remainingTime}s)";
            }
            
            yield return null;
        }

        // 如果超时，随机选择
        if (!selectionMade && availableTargets.Count > 0)
        {
            selectedTarget = availableTargets[Random.Range(0, availableTargets.Count)];
            Debug.Log($"[AttackTargetUI] Time out! Auto-selected {selectedTarget.playerName}");
        }

        // 隐藏面板
        HideTargetSelection();
        
        Debug.Log($"[AttackTargetUI] Final selected target: {(selectedTarget != null ? selectedTarget.playerName : "None")}");
    }

    /// <summary>
    /// 获取最后选择的目标（在ShowTargetSelection完成后调用）
    /// </summary>
    public PlayerData GetSelectedTarget()
    {
        return selectedTarget;
    }

    /// <summary>
    /// 创建目标按钮
    /// </summary>
    void CreateTargetButtons()
    {
        // 清除旧按钮
        ClearTargetButtons();

        foreach (PlayerData target in availableTargets)
        {
            GameObject buttonObj = Instantiate(targetButtonPrefab, targetButtonsContainer);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            // 设置按钮文本
            if (buttonText != null)
            {
                buttonText.text = $"{target.playerName}\nHP: {target.currentHP} | ATK: {target.currentATK}";
            }

            // 设置按钮点击事件
            PlayerData targetRef = target; // 避免闭包问题
            button.onClick.AddListener(() => OnTargetSelected(targetRef));

            targetButtons.Add(buttonObj);
        }
    }

    /// <summary>
    /// 目标选择回调
    /// </summary>
    void OnTargetSelected(PlayerData target)
    {
        if (!waitingForSelection) return;

        selectedTarget = target;
        
        // 更新按钮状态
        UpdateButtonStates();
        
        // 启用确认按钮
        if (confirmButton != null)
            confirmButton.interactable = true;

        Debug.Log($"[AttackTargetUI] Target selected: {target.playerName}");
    }

    /// <summary>
    /// 更新按钮显示状态
    /// </summary>
    void UpdateButtonStates()
    {
        for (int i = 0; i < targetButtons.Count && i < availableTargets.Count; i++)
        {
            Button button = targetButtons[i].GetComponent<Button>();
            Image buttonImage = button.GetComponent<Image>();
            
            if (availableTargets[i] == selectedTarget)
            {
                // 选中状态 - 高亮显示
                buttonImage.color = Color.green;
            }
            else
            {
                // 未选中状态
                buttonImage.color = Color.white;
            }
        }
    }

    /// <summary>
    /// 确认选择
    /// </summary>
    void OnConfirmSelection()
    {
        if (selectedTarget == null) return;
        
        selectionMade = true;
        waitingForSelection = false;
        
        Debug.Log($"[AttackTargetUI] Selection confirmed: {selectedTarget.playerName}");
    }

    /// <summary>
    /// 隐藏目标选择面板
    /// </summary>
    void HideTargetSelection()
    {
        if (targetSelectionPanel != null)
            targetSelectionPanel.SetActive(false);
            
        ClearTargetButtons();
        waitingForSelection = false;
    }

    /// <summary>
    /// 清除目标按钮
    /// </summary>
    void ClearTargetButtons()
    {
        foreach (GameObject button in targetButtons)
        {
            if (button != null)
                Destroy(button);
        }
        targetButtons.Clear();
    }

    /// <summary>
    /// 检查是否正在等待选择
    /// </summary>
    public bool IsWaitingForSelection()
    {
        return waitingForSelection;
    }
}