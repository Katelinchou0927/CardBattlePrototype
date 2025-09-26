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

        // 角色一：基础角色
        characters[0].characterName = "Basic Warrior";
        characters[0].description = "A straightforward fighter who relies on pure strategy and card selection. No special abilities, but mastery of fundamentals.";
        characters[0].hasSkills = false;
        characters[0].skillDescriptions = new List<string>();

        // 角色二：支援角色  
        characters[1].characterName = "Field Commander";
        characters[1].description = "A tactical leader who can influence the entire battlefield. Specializes in area-of-effect abilities.";
        characters[1].hasSkills = true;
        characters[1].skillDescriptions = new List<string>
        {
            "J - Mass Strike: All players lose 2 HP",
            "Q - Field Medic: All players recover 1 HP", 
            "K - Tactical Retreat: All players lose 1 ATK"
        };

        // 角色三：决斗者角色
        characters[2].characterName = "Shadow Duelist";
        characters[2].description = "A mysterious fighter who excels in one-on-one combat and survival tactics. Master of personal enhancement and last-minute saves.";
        characters[2].hasSkills = true;
        characters[2].skillDescriptions = new List<string>
        {
            "J - Soul Exchange: Swap HP or ATK with target player",
            "Q - Life Surge: Gain 5 HP immediately",
            "K - Death Defiance: Survive lethal damage with 1 HP"
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
        
        if (!character.hasSkills)
        {
            // 无技能角色
            GameObject skillTextObj = Instantiate(skillTextPrefab, skillsContainer);
            TextMeshProUGUI skillText = skillTextObj.GetComponent<TextMeshProUGUI>();
            if (skillText != null)
            {
                skillText.text = "No special skills - relies on pure strategy";
                skillText.color = Color.gray;
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
                }
                skillTexts.Add(skillTextObj);
            }
        }
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