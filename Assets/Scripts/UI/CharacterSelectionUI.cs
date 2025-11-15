using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

[System.Serializable]
public class CharacterInfo
{
    public string characterName;
    public string description;
    public List<string> skillDescriptions = new List<string>();
    public Sprite characterArt;
    public bool hasSkills;
}

[System.Serializable]
public class SkillTextStyle
{
    [Header("尺寸设置")]
    public Vector2 preferredSize = new Vector2(500, 50);
    
    [Header("间距设置")]
    public float spacingBetweenSkills = 10f;
    
    [Header("文字设置")]
    public int fontSize = 35;
    public TextAlignmentOptions alignment = TextAlignmentOptions.Left;
    
    [Header("位置偏移")]
    public Vector2 positionOffset = Vector2.zero;
}

public class CharacterSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject characterSelectionPanel;
    public Transform characterButtonsContainer;
    public GameObject characterButtonPrefab;

    [Header("Character Detail Panel")]
    public GameObject detailPanel;
    public Image detailCharacterImage;
    public TextMeshProUGUI detailCharacterName;
    public TextMeshProUGUI detailDescription;
    public Transform skillsContainer;
    public GameObject skillTextPrefab;

    [Header("Control Buttons")]
    public Button confirmButton;
    public Button backButton;

    [Header("Character Data")]
    public CharacterInfo[] characters = new CharacterInfo[3];

    [Header("技能文本框样式设置")]
    public SkillTextStyle skillTextStyle = new SkillTextStyle();

    private int selectedCharacterIndex = -1;
    private List<GameObject> characterButtons = new List<GameObject>();
    private List<GameObject> skillTexts = new List<GameObject>();

    void Awake()
    {
        // 初始化角色数据
        InitializeCharacterData();
        
        // 初始隐藏面板
        if (characterSelectionPanel != null)
            characterSelectionPanel.SetActive(false);
        
        if (detailPanel != null)
            detailPanel.SetActive(false);
    }

    void InitializeCharacterData()
    {
        // 只设置文本数据，保留Inspector中设置的Character Art
        if (characters == null || characters.Length < 3)
        {
            characters = new CharacterInfo[3];
            for (int i = 0; i < 3; i++)
            {
                characters[i] = new CharacterInfo();
            }
        }

        // 角色一：执法者角色（对应图片Character_Player1）
        characters[0].characterName = "Anne Percy";
        characters[0].description = "Anne Percy, 24, Tier-3 [the Enforcer].Loyal to the System. Born into an enforcer family, confident and idealistic, she views order as sacred duty. Oversees maintenance in the Nova Lab, excelling in every synchronization test. Once wavered after her sister's rebellion but quickly restored her faith. Aims to upload her consciousness fully into the System; fears ideological collapse. Her choices prioritize obedience above all.";
        characters[0].hasSkills = false;
        characters[0].skillDescriptions = new List<string>();

        // 角色二：失格者角色（对应图片Character_Player2）
        characters[1].characterName = "Freya Percy";
        characters[1].description = "Freya Percy, 44, Tier-0 [the Defective], Northern Division Leader of the Silencers. \nBorn in a strict enforcer household, rebellious since youth, repeatedly sabotaged the System and was expelled. Wild yet composed, technically skilled. Seeks to overthrow the System and reclaim free thought, fearing assimilation into emptiness. Her decisions center on resistance and leadership, with family and survival secondary.";
        characters[1].hasSkills = true;
        characters[1].skillDescriptions = new List<string>
        {
            "J - Fearless Decisiveness: Ignore the opponent's health points and kill with one strike",
            "Q - Live and Die Together: Make any player on the field share their current health and attack equally - Attack = Health = (Attack + Health) /2", 
            "K - Choice of a Fork in the Road: Offsets one attack"
        };

        // 角色三：临界者角色（对应图片Character_Player3）
        characters[2].characterName = "Lena Moro";
        characters[2].description = "Lena Moro, 26, Tier-1 [the Limiter], member of the Silencers' moderate faction.\nBorn in the industrial ruins of the Rift Zone, raised by her exiled mother. A medic and consciousness restorer devoted to repairing minds damaged by the System. Calm, introspective, and philosophical, she values awakening over violence. Fears the Silencers' extinction and humanity’s total formatting. Her choices favor healing and research above all else.";
        characters[2].hasSkills = true;
        characters[2].skillDescriptions = new List<string>
        {
            "J - Thought Erosion: Designate one player. After each attack in this round is settled, 2 health points will be deducted",
            "Q - Alarm Bell: Self-destruct, death in this round, deals 5 damage to all players. Each player except yourself gets 1 point",
            "K - Thought Barrier Strikes with Sound: Return 50% of the damage caused to oneself by the source of the damage"
        };

        Debug.Log("[CharacterSelection] Character data initialized, preserving Inspector Character Art settings");
    }

    public void ShowCharacterSelection()
    {
        if (characterSelectionPanel != null)
            characterSelectionPanel.SetActive(true);
            
        CreateCharacterButtons();
        
        // 初始化按钮状态
        if (confirmButton != null)
        {
            confirmButton.interactable = false;
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmSelection);
        }
        
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackToMenu);
        }

        Debug.Log("[CharacterSelection] Character selection UI shown");
    }

    void CreateCharacterButtons()
    {
        ClearCharacterButtons();

        for (int i = 0; i < characters.Length; i++)
        {
            GameObject buttonObj = Instantiate(characterButtonPrefab, characterButtonsContainer);
            Button button = buttonObj.GetComponent<Button>();
            
            // 设置按钮文本
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = characters[i].characterName;
            }

            // 设置按钮点击事件
            int characterIndex = i; // 避免闭包问题
            button.onClick.AddListener(() => OnCharacterSelected(characterIndex));

            characterButtons.Add(buttonObj);
        }
    }

    void OnCharacterSelected(int characterIndex)
    {
        selectedCharacterIndex = characterIndex;
        
        // 更新按钮状态
        UpdateButtonStates();
        
        // 显示角色详情
        ShowCharacterDetail(characterIndex);
        
        // 启用确认按钮
        if (confirmButton != null)
            confirmButton.interactable = true;

        Debug.Log($"[CharacterSelection] Selected character: {characters[characterIndex].characterName}");
    }

    void UpdateButtonStates()
    {
        for (int i = 0; i < characterButtons.Count; i++)
        {
            Button button = characterButtons[i].GetComponent<Button>();
            Image buttonImage = button.GetComponent<Image>();
            
            if (i == selectedCharacterIndex)
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

    void ShowCharacterDetail(int characterIndex)
    {
        if (detailPanel == null || characterIndex < 0 || characterIndex >= characters.Length)
        {
            Debug.LogError($"[CharacterSelection] Cannot show detail - detailPanel: {detailPanel != null}, characterIndex: {characterIndex}");
            return;
        }

        Debug.Log($"[CharacterSelection] Showing detail for character {characterIndex}");
        detailPanel.SetActive(true);
        
        CharacterInfo character = characters[characterIndex];
        
        // 设置角色名称
        if (detailCharacterName != null)
        {
            detailCharacterName.text = character.characterName;
            Debug.Log($"[CharacterSelection] Set character name: {character.characterName}");
        }
        else
        {
            Debug.LogError("[CharacterSelection] detailCharacterName is null!");
        }
        
        // 设置描述
        if (detailDescription != null)
        {
            detailDescription.text = character.description;
            Debug.Log($"[CharacterSelection] Set description: {character.description.Substring(0, Mathf.Min(50, character.description.Length))}...");
        }
        else
        {
            Debug.LogError("[CharacterSelection] detailDescription is null!");
        }
        
        // 设置角色图片
        if (detailCharacterImage != null && character.characterArt != null)
        {
            detailCharacterImage.sprite = character.characterArt;
            Debug.Log($"[CharacterSelection] Set character art: {character.characterArt.name}");
        }
        else
        {
            Debug.LogError($"[CharacterSelection] Image setting failed - detailCharacterImage: {detailCharacterImage != null}, characterArt: {character.characterArt != null}");
        }
        
        // 显示技能信息
        ShowSkillDetails(character);
    }

    void ShowSkillDetails(CharacterInfo character)
    {
        ClearSkillTexts();
        
        // 配置技能容器的LayoutGroup
        VerticalLayoutGroup layoutGroup = skillsContainer.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.spacing = skillTextStyle.spacingBetweenSkills;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = true;
        }
        
        if (!character.hasSkills)
        {
            // 无技能角色
            GameObject skillTextObj = Instantiate(skillTextPrefab, skillsContainer);
            TextMeshProUGUI skillText = skillTextObj.GetComponent<TextMeshProUGUI>();
            if (skillText != null)
            {
                skillText.text = "No special skills - relies on pure strategy";
                skillText.color = Color.gray;
                ApplySkillTextStyle(skillText, skillTextObj);
            }
            skillTexts.Add(skillTextObj);
        }
        else
        {
            // 有技能角色
            foreach (string skillDesc in character.skillDescriptions)
            {
                GameObject skillTextObj = Instantiate(skillTextPrefab, skillsContainer);
                TextMeshProUGUI skillText = skillTextObj.GetComponent<TextMeshProUGUI>();
                if (skillText != null)
                {
                    skillText.text = skillDesc;
                    skillText.color = Color.white;
                    ApplySkillTextStyle(skillText, skillTextObj);
                }
                skillTexts.Add(skillTextObj);
            }
        }
    }

    /// <summary>
    /// 应用技能文本框的样式设置
    /// </summary>
    void ApplySkillTextStyle(TextMeshProUGUI skillText, GameObject skillTextObj)
    {
        // 设置文字大小
        skillText.fontSize = skillTextStyle.fontSize;
        
        // 设置文字对齐方式
        skillText.alignment = skillTextStyle.alignment;
        
        // 设置RectTransform尺寸
        RectTransform rectTransform = skillTextObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = skillTextStyle.preferredSize;
            rectTransform.anchoredPosition = skillTextStyle.positionOffset;
        }
        
        // 设置LayoutElement用于更好的控制
        LayoutElement layoutElement = skillTextObj.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = skillTextObj.AddComponent<LayoutElement>();
        }
        
        layoutElement.preferredWidth = skillTextStyle.preferredSize.x;
        layoutElement.preferredHeight = skillTextStyle.preferredSize.y;
    }

    void OnConfirmSelection()
    {
        if (selectedCharacterIndex < 0) return;

        CharacterInfo selectedCharacter = characters[selectedCharacterIndex];
        Debug.Log($"[CharacterSelection] Confirmed selection: {selectedCharacter.characterName}");

        // 保存选择的角色信息
        PlayerPrefs.SetInt("SelectedCharacterIndex", selectedCharacterIndex);
        PlayerPrefs.SetString("SelectedCharacterName", selectedCharacter.characterName);
        PlayerPrefs.SetInt("HasSkills", selectedCharacter.hasSkills ? 1 : 0);
        PlayerPrefs.Save();

        // 隐藏角色选择面板
        HideCharacterSelection();

        // 进入游戏场景
        SceneManager.LoadScene("MainBattle");
    }

    void OnBackToMenu()
    {
        HideCharacterSelection();
    
        // 通知 StartMenu 显示主菜单按钮
        StartMenu startMenu = FindObjectOfType<StartMenu>();
        if (startMenu != null)
        {
            startMenu.BackToStartMenu();
        }
        else
        {
            Debug.LogWarning("[CharacterSelection] StartMenu not found!");
        }
    }

    public void HideCharacterSelection()
    {
        if (characterSelectionPanel != null)
            characterSelectionPanel.SetActive(false);
            
        if (detailPanel != null)
            detailPanel.SetActive(false);
            
        ClearCharacterButtons();
        ClearSkillTexts();
        
        selectedCharacterIndex = -1;
    }

    void ClearCharacterButtons()
    {
        foreach (GameObject button in characterButtons)
        {
            if (button != null)
                Destroy(button);
        }
        characterButtons.Clear();
    }

    void ClearSkillTexts()
    {
        foreach (GameObject skillText in skillTexts)
        {
            if (skillText != null)
                Destroy(skillText);
        }
        skillTexts.Clear();
    }

    /// <summary>
    /// 获取当前选择的角色索引（用于其他脚本调用）
    /// </summary>
    public int GetSelectedCharacterIndex()
    {
        return selectedCharacterIndex;
    }

    /// <summary>
    /// 获取角色信息（用于其他脚本调用）
    /// </summary>
    public CharacterInfo GetCharacterInfo(int index)
    {
        if (index >= 0 && index < characters.Length)
            return characters[index];
        return null;
    }
}