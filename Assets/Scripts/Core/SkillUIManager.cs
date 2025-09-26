using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Enhanced Skill UI Manager - Inspector customizable + One-time use skills
/// </summary>
public class SkillUIManager : MonoBehaviour
{
    [Header("Skill Panel References")]
    public GameObject skillPanel;                
    public Button skillJ_Button;                 
    public Button skillQ_Button;                 
    public Button skillK_Button;                 
    public Button skillSkip_Button;              // 新增：跳过按钮
    public TextMeshProUGUI skillPromptText;      

    [Header("Skip Button Settings")]
    public string skipButtonText = "Skip";
    public string skipButtonDescription = "Don't use any skill";
    public Color skipButtonColor = Color.gray;

    [Header("UI Customization")]
    [Range(3f, 15f)]
    public float skillDecisionTime = 8f;         // Decision time in seconds
    
    [Space(10)]
    [Header("Text Settings")]
    public string promptPrefix = "Your Turn:";
    public string timeoutSuffix = "Choose a skill or wait to skip";
    public string skillUnavailableText = "(Used)";
    public string skipTimeoutMessage = "Time out! Auto-skipped skill usage";
    
    [Space(10)]
    [Header("Button Colors")]
    public Color availableButtonColor = Color.white;
    public Color unavailableButtonColor = Color.gray;
    public Color selectedButtonColor = Color.green;
    
    [Space(10)]
    [Header("Animation Settings")]
    public bool enableButtonAnimation = true;
    public float buttonScaleOnHover = 1.1f;
    public float animationDuration = 0.2f;

    [Header("Debug Settings")]
    public bool showDebugLogs = true;

    // Private variables
    private bool waitingForSkillDecision = false;
    private bool skillDecisionMade = false;
    private string selectedSkill = "";
    private GameManager gameManager;
    
    // Track permanently used skills (across all 5 rounds)
    private static HashSet<string> permanentlyUsedSkills = new HashSet<string>();
    
    void Start()
    {
        gameManager = GameManager.Instance;
    
        // 绑定现有按钮事件
        if (skillJ_Button != null)
        {
            skillJ_Button.onClick.RemoveAllListeners();
            skillJ_Button.onClick.AddListener(() => OnSkillSelected("J"));
        }
    
        if (skillQ_Button != null)
        {
            skillQ_Button.onClick.RemoveAllListeners();
            skillQ_Button.onClick.AddListener(() => OnSkillSelected("Q"));
        }
    
        if (skillK_Button != null)
        {
            skillK_Button.onClick.RemoveAllListeners();
            skillK_Button.onClick.AddListener(() => OnSkillSelected("K"));
        }
    
        // 绑定跳过按钮事件
        if (skillSkip_Button != null)
        {
            skillSkip_Button.onClick.RemoveAllListeners();
            skillSkip_Button.onClick.AddListener(() => OnSkillSelected("SKIP"));
        }
    
        HideSkillPanel();
    
        if (GameManager.Instance != null && GameManager.Instance.GetCurrentRound() == 1)
        {
            ResetAllSkills();
        }
    }

    /// <summary>
    /// 显示技能选择面板，修改提示文本
    /// </summary>
    public IEnumerator ShowSkillChoice(string situation, PlayerData player)
    {
        if (showDebugLogs)
            Debug.Log($"[SkillUI] Showing skill choice for situation: {situation}");
    
        if (gameManager == null)
        {
            Debug.LogError("[SkillUI] GameManager not found!");
            selectedSkill = "SKIP";
            yield break;
        }
    
        var characterInfo = gameManager.GetPlayerCharacterInfo();
        if (!characterInfo.hasSkills)
        {
            if (showDebugLogs)
                Debug.Log("[SkillUI] Player has no skills");
            selectedSkill = "SKIP";
            yield break;
        }

        ShowSkillPanel();
    
        // 设置提示文本（强调跳过选项）
        if (skillPromptText != null)
        {
            string fullPrompt = $"{promptPrefix} {situation}\nChoose a skill or click Skip ({skillDecisionTime:F0}s)";
            skillPromptText.text = fullPrompt;
        }
    
        UpdateSkillButtons(situation, characterInfo);
    
        waitingForSkillDecision = true;
        skillDecisionMade = false;
        selectedSkill = "";
    
        float timer = 0;
        while (timer < skillDecisionTime && !skillDecisionMade)
        {
            timer += Time.deltaTime;
        
            if (skillPromptText != null)
            {
                int remainingTime = Mathf.CeilToInt(skillDecisionTime - timer);
                string fullPrompt = $"{promptPrefix} {situation}\nChoose a skill or click Skip ({remainingTime}s)";
                skillPromptText.text = fullPrompt;
            }
        
            yield return null;
        }
    
        // 如果超时且没有选择，自动跳过
        if (!skillDecisionMade)
        {
            selectedSkill = "SKIP";
            if (showDebugLogs)
                Debug.Log($"[SkillUI] {skipTimeoutMessage}");
        }
    
        waitingForSkillDecision = false;
        HideSkillPanel();
    
        if (showDebugLogs)
            Debug.Log($"[SkillUI] Final skill selection: {selectedSkill}");
    }

    /// <summary>
    /// 更新技能按钮显示，包括跳过按钮
    /// </summary>
    void UpdateSkillButtons(string situation, (int characterIndex, bool hasSkills, string characterName) characterInfo)
    {
        // 获取技能描述
        string jDesc = gameManager != null ? gameManager.GetSkillDescription("J") : "";
        string qDesc = gameManager != null ? gameManager.GetSkillDescription("Q") : "";
        string kDesc = gameManager != null ? gameManager.GetSkillDescription("K") : "";

        // 更新JQK按钮
        UpdateSingleSkillButton(skillJ_Button, "J", jDesc, situation);
        UpdateSingleSkillButton(skillQ_Button, "Q", qDesc, situation);
        UpdateSingleSkillButton(skillK_Button, "K", kDesc, situation);
    
        // 更新跳过按钮（总是可用）
        UpdateSkipButton();
    }
    
    /// <summary>
    /// 更新跳过按钮
    /// </summary>
    void UpdateSkipButton()
    {
        if (skillSkip_Button == null) return;
    
        // 跳过按钮总是可用
        skillSkip_Button.interactable = true;
    
        // 更新按钮文本
        var skipText = skillSkip_Button.GetComponentInChildren<TextMeshProUGUI>();
        if (skipText != null)
        {
            skipText.text = $"{skipButtonText}\n{skipButtonDescription}";
        }
    
        // 更新按钮颜色
        var skipImage = skillSkip_Button.GetComponent<Image>();
        if (skipImage != null)
        {
            skipImage.color = skipButtonColor;
        }
    }

    
    /// <summary>
    /// Update individual skill button
    /// </summary>
    void UpdateSingleSkillButton(Button button, string skillType, string description, string situation)
    {
        if (button == null) return;
        
        bool isPermanentlyUsed = IsSkillPermanentlyUsed(skillType);
        bool canUseInSituation = CanUseSkillInSituation(skillType, situation);
        bool isAvailable = !isPermanentlyUsed && canUseInSituation;
        
        // Set button interactability
        button.interactable = isAvailable;
        
        // Update button text
        var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            if (isPermanentlyUsed)
            {
                buttonText.text = $"{skillType}\n{skillUnavailableText}";
            }
            else if (!canUseInSituation)
            {
                buttonText.text = $"{skillType}\n(Cannot use now)";
            }
            else
            {
                buttonText.text = $"{skillType}\n{description}";
            }
        }
        
        // Update button color
        var buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = isAvailable ? availableButtonColor : unavailableButtonColor;
        }
    }

    /// <summary>
    /// Check if skill can be used in current situation
    /// </summary>
    bool CanUseSkillInSituation(string skillType, string situation)
    {
        if (gameManager == null) return false;
        
        var characterInfo = gameManager.GetPlayerCharacterInfo();
        
        switch (skillType)
        {
            case "J":
                // J skill usually available during attack
                return situation.Contains("attack") || situation.Contains("turn");
            case "Q":
                // Q skill usually available anytime
                return true;
            case "K":
                // K skill usually available when under attack or defending
                return situation.Contains("under attack") || situation.Contains("defend") || situation.Contains("attack");
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Check if skill has been permanently used (one-time use rule)
    /// </summary>
    bool IsSkillPermanentlyUsed(string skillType)
    {
        return permanentlyUsedSkills.Contains(skillType);
    }
    
    /// <summary>
    /// Mark skill as permanently used
    /// </summary>
    void MarkSkillAsUsed(string skillType)
    {
        permanentlyUsedSkills.Add(skillType);
        if (showDebugLogs)
            Debug.Log($"[SkillUI] Skill {skillType} marked as permanently used");
    }

    /// <summary>
    /// 技能选择回调，处理跳过逻辑
    /// </summary>
    void OnSkillSelected(string skill)
    {
        if (!waitingForSkillDecision) return;
    
        // 如果选择跳过，不需要检查技能可用性
        if (skill == "SKIP")
        {
            selectedSkill = skill;
            skillDecisionMade = true;
        
            if (showDebugLogs)
                Debug.Log($"[SkillUI] Player chose to skip skill usage");
        
            HideSkillPanel();
            return;
        }
    
        // 检查技能是否仍然可用
        if (IsSkillPermanentlyUsed(skill))
        {
            if (showDebugLogs)
                Debug.LogWarning($"[SkillUI] Attempted to use already used skill: {skill}");
            return;
        }

        selectedSkill = skill;
        skillDecisionMade = true;
    
        // 标记技能为永久使用
        MarkSkillAsUsed(skill);
    
        if (showDebugLogs)
            Debug.Log($"[SkillUI] Player selected skill: {skill} (now permanently used)");
    
        HideSkillPanel();
    
        if (enableButtonAnimation)
        {
            StartCoroutine(ShowButtonSelectedEffect(skill));
        }
    }
    
    /// <summary>
    /// Show button selection animation
    /// </summary>
    IEnumerator ShowButtonSelectedEffect(string skillType)
    {
        Button selectedButton = null;
        switch (skillType)
        {
            case "J": selectedButton = skillJ_Button; break;
            case "Q": selectedButton = skillQ_Button; break;
            case "K": selectedButton = skillK_Button; break;
        }
        
        if (selectedButton != null)
        {
            var originalScale = selectedButton.transform.localScale;
            var targetScale = originalScale * buttonScaleOnHover;
            
            // Scale up
            float elapsedTime = 0;
            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / animationDuration;
                selectedButton.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }
            
            // Scale back down
            elapsedTime = 0;
            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / animationDuration;
                selectedButton.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }
            
            selectedButton.transform.localScale = originalScale;
        }
    }

    /// <summary>
    /// Show skill panel
    /// </summary>
    void ShowSkillPanel()
    {
        if (skillPanel != null)
        {
            skillPanel.SetActive(true);
            if (showDebugLogs)
                Debug.Log("[SkillUI] Skill panel shown");
        }
        else
        {
            Debug.LogWarning("[SkillUI] Skill panel is null!");
        }
    }

    /// <summary>
    /// Hide skill panel
    /// </summary>
    void HideSkillPanel()
    {
        if (skillPanel != null)
        {
            skillPanel.SetActive(false);
            if (showDebugLogs)
                Debug.Log("[SkillUI] Skill panel hidden");
        }
    }

    /// <summary>
    /// Get selected skill (called after coroutine ends)
    /// </summary>
    public string GetSelectedSkill()
    {
        return selectedSkill;
    }

    /// <summary>
    /// Check if waiting for skill selection
    /// </summary>
    public bool IsWaitingForSkillSelection()
    {
        return waitingForSkillDecision;
    }

    /// <summary>
    /// Force skip skill selection (for debugging or special cases)
    /// </summary>
    public void ForceSkipSkill()
    {
        if (waitingForSkillDecision)
        {
            selectedSkill = "SKIP";
            skillDecisionMade = true;
            HideSkillPanel();
            if (showDebugLogs)
                Debug.Log("[SkillUI] Forced skill skip");
        }
    }
    
    /// <summary>
    /// Reset all skills for new game (call at game start)
    /// </summary>
    public static void ResetAllSkills()
    {
        permanentlyUsedSkills.Clear();
        Debug.Log("[SkillUI] All skills reset for new game");
    }
    
    /// <summary>
    /// Get list of remaining available skills
    /// </summary>
    public List<string> GetAvailableSkills()
    {
        List<string> available = new List<string>();
        string[] allSkills = { "J", "Q", "K" };
        
        foreach (string skill in allSkills)
        {
            if (!IsSkillPermanentlyUsed(skill))
            {
                available.Add(skill);
            }
        }
        
        return available;
    }
    
    /// <summary>
    /// Get list of used skills
    /// </summary>
    public List<string> GetUsedSkills()
    {
        return new List<string>(permanentlyUsedSkills);
    }
    
    // Inspector utility methods
    [Header("Debug Tools")]
    [Space(5)]
    public bool resetSkillsOnStart = false;
    
    void Update()
    {
        if (resetSkillsOnStart)
        {
            resetSkillsOnStart = false;
            ResetAllSkills();
        }
    }
    
    /// <summary>
    /// Context menu method for testing
    /// </summary>
    [ContextMenu("Reset All Skills")]
    void ResetSkillsContextMenu()
    {
        ResetAllSkills();
    }
    
    [ContextMenu("Show Available Skills")]
    void ShowAvailableSkillsContextMenu()
    {
        var available = GetAvailableSkills();
        var used = GetUsedSkills();
        Debug.Log($"Available Skills: [{string.Join(", ", available)}]");
        Debug.Log($"Used Skills: [{string.Join(", ", used)}]");
    }
}