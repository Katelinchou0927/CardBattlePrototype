using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Card Resources")]
    public GameObject cardPrefab;
    public Sprite[] cardSprites;
    public Sprite cardBack;
    public Button confirmButton;

    [Header("Hand Areas - 4 Players")]
    public Transform player1HandArea;  // 数字牌区域
    public Transform player2HandArea;  
    public Transform player3HandArea;  
    public Transform player4HandArea;  

    [Header("Skill Card Areas - Only for Players with Skills")]
    public Transform player1SkillArea;  // 技能牌区域

    [Header("UI Panels")]
    public GameObject cardSelectionPanel;

    // 4个玩家的卡牌列表
    private List<CardData> player1Cards = new List<CardData>();
    private List<CardData> player2Cards = new List<CardData>();
    private List<CardData> player3Cards = new List<CardData>();
    private List<CardData> player4Cards = new List<CardData>();

    // 技能牌列表（与数字牌分离）
    private List<CardData> player1SkillCards = new List<CardData>();

    private CardData selectedHpCard = null;
    private CardData selectedAtkCard = null;

    private bool atkConfirmed = false;
    private bool hpConfirmed = false;

    // 轮次相关
    private int currentRound = 1;
    private List<int> usedCards = new List<int>(); // 记录已使用的卡牌

    // 角色相关
    private int selectedCharacterIndex = 0;
    private bool playerHasSkills = false;
    private string selectedCharacterName = "Basic Warrior";
    private string selectedSkill = ""; // 当前选择的技能

    void Awake()
    {
        Instance = this;
        
        // 获取角色选择信息
        LoadSelectedCharacterInfo();
    }

    /// <summary>
    /// 加载选择的角色信息
    /// </summary>
    void LoadSelectedCharacterInfo()
    {
        selectedCharacterIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);
        selectedCharacterName = PlayerPrefs.GetString("SelectedCharacterName", "Basic Warrior");
        playerHasSkills = PlayerPrefs.GetInt("HasSkills", 0) == 1;
        
        Debug.Log($"[GameManager] Loaded character: {selectedCharacterName} (Index: {selectedCharacterIndex}, Has Skills: {playerHasSkills})");
        
        // 根据角色类型调整游戏设置
        ApplyCharacterSettings();
    }

    /// <summary>
    /// 应用角色设置
    /// </summary>
    void ApplyCharacterSettings()
    {
        switch (selectedCharacterIndex)
        {
            case 0: // Basic Warrior - 无技能
                Debug.Log("[GameManager] Basic Warrior selected - no special skills");
                break;
            case 1: // Field Commander - 群体技能
                Debug.Log("[GameManager] Field Commander selected - area effect skills available");
                break;
            case 2: // Shadow Duelist - 个人技能
                Debug.Log("[GameManager] Shadow Duelist selected - personal combat skills available");
                break;
            default:
                Debug.LogWarning($"[GameManager] Unknown character index: {selectedCharacterIndex}");
                break;
        }
    }

    /// <summary>
    /// Get player character info for battle system
    /// </summary>
    public (int characterIndex, bool hasSkills, string characterName) GetPlayerCharacterInfo()
    {
        return (selectedCharacterIndex, playerHasSkills, selectedCharacterName);
    }

    /// <summary>
    /// Check if player has specified skill (for battle system)
    /// </summary>
    public bool PlayerHasSkill(string skillType)
    {
        if (!playerHasSkills) return false;
        
        // All skill-capable characters have J, Q, K skills, just with different effects
        return skillType == "J" || skillType == "Q" || skillType == "K";
    }

    /// <summary>
    /// Get skill description in English (for UI display)
    /// </summary>
    public string GetSkillDescription(string skillType)
    {
        if (!playerHasSkills) return "";
        
        switch (selectedCharacterIndex)
        {
            case 1: // Field Commander - Area Effect Skills
                switch (skillType)
                {
                    case "J": return "All players lose 2 HP";
                    case "Q": return "All players gain 1 HP"; 
                    case "K": return "All players lose 1 ATK";
                    default: return "";
                }
            case 2: // Shadow Duelist - Personal Skills
                switch (skillType)
                {
                    case "J": return "Exchange HP/ATK with target";
                    case "Q": return "Gain 5 HP for yourself";
                    case "K": return "Survive fatal attack with 1 HP";
                    default: return "";
                }
            default:
                return "";
        }
    }

    /// <summary>
    /// Get detailed skill description for tooltips/help
    /// </summary>
    public string GetDetailedSkillDescription(string skillType)
    {
        if (!playerHasSkills) return "";
        
        switch (selectedCharacterIndex)
        {
            case 1: // Field Commander - Area Effect Skills
                switch (skillType)
                {
                    case "J": return "Global Damage: All players on the battlefield take 2 damage";
                    case "Q": return "Global Heal: All players on the battlefield recover 1 HP";
                    case "K": return "Global Weaken: All players on the battlefield lose 1 attack power";
                    default: return "";
                }
            case 2: // Shadow Duelist - Personal Skills  
                switch (skillType)
                {
                    case "J": return "Stat Exchange: Swap your HP or ATK with target player";
                    case "Q": return "Self Heal: Restore 5 HP to yourself";
                    case "K": return "Last Stand: When receiving fatal damage, survive with 1 HP instead";
                    default: return "";
                }
            default:
                return "";
        }
    }

    /// <summary>
    /// Reset skills for new game (called at game start)
    /// </summary>
    public void ResetSkillsForNewGame()
    {
        // Reset the skill UI manager's tracking
        SkillUIManager.ResetAllSkills();
        Debug.Log("[GameManager] Skills reset for new 5-round game");
    }

    /// <summary>
    /// Get available skills for current player (one-time use aware)
    /// </summary>
    public List<string> GetCurrentlyAvailableSkills()
    {
        if (!playerHasSkills) return new List<string>();
        
        // Get skills that haven't been used yet from the UI manager
        SkillUIManager skillUI = FindObjectOfType<SkillUIManager>();
        if (skillUI != null)
        {
            return skillUI.GetAvailableSkills();
        }
        
        // Fallback: assume all skills available if UI manager not found
        return new List<string> { "J", "Q", "K" };
    }

    /// <summary>
    /// Get character name in English
    /// </summary>
    public string GetCharacterNameEnglish()
    {
        switch (selectedCharacterIndex)
        {
            case 0: return "Basic Warrior";
            case 1: return "Field Commander";
            case 2: return "Shadow Duelist";
            default: return "Unknown Character";
        }
    }

    /// <summary>
    /// Get character description in English
    /// </summary>
    public string GetCharacterDescription()
    {
        switch (selectedCharacterIndex)
        {
            case 0: return "No special abilities. Relies on pure strategy.";
            case 1: return "Area effect skills that impact all players on the battlefield.";
            case 2: return "Personal combat skills with powerful individual effects.";
            default: return "Unknown character type.";
        }
    }

    public void StartGame()
    {
        Debug.Log($"[GameManager] Starting game with character: {selectedCharacterName}");
        
        currentRound = 1;
        usedCards.Clear();
        
        // Reset skills for new game (important for one-time use rule)
        if (playerHasSkills)
        {
            ResetSkillsForNewGame();
            Debug.Log("[GameManager] One-time skills reset: J, Q, K available");
        }
        
        // Start first round card selection
        StartRoundCardSelection();
    }

    /// <summary>
    /// 开始某轮的选牌阶段
    /// </summary>
    public void StartRoundCardSelection()
    {
        Debug.Log($"[GameManager] Starting Round {currentRound} card selection for 4 players with character skills: {playerHasSkills}");
        
        // 隐藏技能区域（选牌阶段不显示技能卡）
        if (player1SkillArea != null && player1SkillArea.gameObject != null)
        {
            player1SkillArea.gameObject.SetActive(false);
        }
        
        // 清除现有卡牌
        ClearPlayerHands();
        
        // 计算当前轮可用的卡牌（排除已使用的）
        List<int> availableCards = GetAvailableCards();
        
        // 生成当前轮的卡牌 - 4个玩家，选牌阶段不显示技能牌
        DealAvailableCards("Player1", "club", player1HandArea, player1Cards, availableCards, false);    // 人类玩家（下方）
        DealAvailableCards("Player2", "diamond", player2HandArea, player2Cards, availableCards, false); // AI玩家1（上方）
        DealAvailableCards("Player3", "heart", player3HandArea, player3Cards, availableCards, false);   // AI玩家2（左侧）
        DealAvailableCards("Player4", "spade", player4HandArea, player4Cards, availableCards, false);   // AI玩家3（右侧）

        // 重置选择状态
        ResetSelectionState();
        
        // 显示选牌界面
        ShowCardSelectionUI();
    }

    /// <summary>
    /// 获取当前轮可用的卡牌
    /// </summary>
    List<int> GetAvailableCards()
    {
        List<int> available = new List<int>();
        for (int i = 1; i <= 10; i++)
        {
            if (!usedCards.Contains(i))
            {
                available.Add(i);
            }
        }
        
        Debug.Log($"[GameManager] Round {currentRound}: {available.Count} cards available: [{string.Join(", ", available)}]");
        return available;
    }

    /// <summary>
    /// 修改后的发牌方法 - 选牌阶段只处理数字牌
    /// </summary>
    void DealAvailableCards(string ownerId, string suit, Transform handArea, List<CardData> cardList, List<int> availableCards, bool hasSkills = false)
    {
        // 只处理数字牌，技能牌不在选牌阶段显示
        foreach (int number in availableCards)
        {
            GameObject cardGO = Instantiate(cardPrefab, handArea);
            CardDisplay display = cardGO.GetComponent<CardDisplay>();

            Sprite cardSprite = GetCardSprite(number, suit);
            display.SetCardFace(cardSprite);

            // 只有玩家1（人类玩家）显示正面，其他玩家显示背面
            if (ownerId == "Player1")
                display.ShowFront();
            else
                display.ShowBack();

            CardData data = cardGO.GetComponent<CardData>();
            data.ownerId = ownerId;
            data.number = number;
            data.cardType = CardType.Normal;
            data.used = false;

            cardList.Add(data);
            display.RefreshUI();
        }
        
        Debug.Log($"[GameManager] Created {availableCards.Count} number cards for {ownerId} in {suit} suit");
        // 注意：不再在这里创建技能牌！
    }

    /// <summary>
    /// 在战斗开始时创建技能卡牌显示
    /// </summary>
    void InitializeBattlePhaseSkillCards()
    {
        if (player1SkillArea == null || !playerHasSkills)
        {
            return;
        }

        Debug.Log("[GameManager] Creating battle phase skill cards display");

        // 清空现有内容
        foreach (Transform child in player1SkillArea)
        {
            Destroy(child.gameObject);
        }
        player1SkillCards.Clear();

        // 创建J、Q、K技能卡牌
        string[] skills = { "J", "Q", "K" };
        foreach (string skill in skills)
        {
            CreateBattlePhaseSkillCard(skill);
        }

        // 确保技能区域可见
        if (player1SkillArea.gameObject != null)
        {
            player1SkillArea.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 创建单个战斗阶段技能卡牌
    /// </summary>
    void CreateBattlePhaseSkillCard(string skillType)
    {
        GameObject cardGO = Instantiate(cardPrefab, player1SkillArea);
        CardDisplay display = cardGO.GetComponent<CardDisplay>();

        // 获取技能卡牌图片
        Sprite skillSprite = GetSkillCardSprite(skillType, "club"); // 默认使用club花色
        display.SetCardFace(skillSprite);
        display.ShowFront(); // 技能卡总是显示正面

        CardData data = cardGO.GetComponent<CardData>();
        data.ownerId = "Player1";
        data.number = GetSkillCardNumber(skillType);
        data.cardType = CardType.Special;
        data.used = false; // 初始状态为可用

        player1SkillCards.Add(data);
        display.RefreshUI();

        Debug.Log($"[GameManager] Created battle phase skill card: {skillType}");
    }

    /// <summary>
    /// 更新战斗阶段技能卡牌状态
    /// </summary>
    void UpdateBattlePhaseSkillCards()
    {
        if (player1SkillCards.Count == 0) return;

        foreach (var skillCard in player1SkillCards)
        {
            string skillType = GetSkillType(skillCard);
            bool isUsed = SkillUIManager.IsSkillUsedStatic(skillType);
            
            // 更新使用状态
            skillCard.used = isUsed;
            
            // 刷新UI显示
            var display = skillCard.GetComponent<CardData>();
            if (display != null)
            {
                display.GetComponent<CardDisplay>().RefreshUI();
            }
        }

        Debug.Log("[GameManager] Updated battle phase skill cards status");
    }

    /// <summary>
    /// 获取技能卡牌图片
    /// </summary>
    Sprite GetSkillCardSprite(string skillType, string suit)
    {
        // 尝试多种可能的命名规则
        string[] possibleNames = {
            $"card{GetSuitName(suit)}_{skillType}",           // cardClubs_J
            $"{suit}_{skillType}",                            // club_J  
            $"card_{suit}_{skillType}",                       // card_club_J
            $"{skillType}_{suit}",                            // J_club
            $"{skillType}_{GetSuitName(suit)}",               // J_Clubs
            $"skill_{skillType}",                             // skill_J
            $"{skillType}"                                    // J
        };
        
        Debug.Log($"[GameManager] Looking for skill card with these names:");
        foreach (string name in possibleNames)
        {
            Debug.Log($"  - {name}");
        }

        // 遍历所有可能的命名规则
        foreach (string cardName in possibleNames)
        {
            foreach (Sprite sprite in cardSprites)
            {
                if (sprite.name.Equals(cardName, System.StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log($"[GameManager] Found matching skill card image: {sprite.name}");
                    return sprite;
                }
            }
        }

        // 如果都找不到，列出所有可用的图片供调试
        Debug.LogWarning($"[GameManager] Cannot find skill card image for {skillType}_{suit}");
        Debug.Log("Available sprites:");
        foreach (Sprite sprite in cardSprites)
        {
            if (sprite.name.Contains(skillType) || sprite.name.ToUpper().Contains(skillType))
            {
                Debug.Log($"  - Found potential match: {sprite.name}");
            }
        }

        // 备用方案：使用默认图片或第一个可用图片
        return cardSprites.Length > 0 ? cardSprites[0] : null;
    }

    /// <summary>
    /// 获取花色名称
    /// </summary>
    string GetSuitName(string suit)
    {
        switch (suit.ToLower())
        {
            case "club": return "Clubs";
            case "diamond": return "Diamonds";
            case "heart": return "Hearts";
            case "spade": return "Spades";
            default: return "Clubs";
        }
    }

    /// <summary>
    /// 获取技能卡牌的数字标识
    /// </summary>
    int GetSkillCardNumber(string skillType)
    {
        switch (skillType)
        {
            case "J": return 11; // J = 11
            case "Q": return 12; // Q = 12  
            case "K": return 13; // K = 13
            default: return 0;
        }
    }

    /// <summary>
    /// 检查是否为技能卡
    /// </summary>
    bool IsSkillCard(CardData card)
    {
        return card.number >= 11 && card.number <= 13;
    }

    /// <summary>
    /// 获取技能类型
    /// </summary>
    string GetSkillType(CardData card)
    {
        switch (card.number)
        {
            case 11: return "J";
            case 12: return "Q";
            case 13: return "K";
            default: return "";
        }
    }

    /// <summary>
    /// 清空所有玩家手牌（包括技能牌区域）
    /// </summary>
    void ClearPlayerHands()
    {
        // 清空数字牌区域
        if (player1HandArea != null)
        {
            foreach (Transform child in player1HandArea) Destroy(child.gameObject);
        }
        if (player2HandArea != null)
        {
            foreach (Transform child in player2HandArea) Destroy(child.gameObject);
        }
        if (player3HandArea != null)
        {
            foreach (Transform child in player3HandArea) Destroy(child.gameObject);
        }
        if (player4HandArea != null)
        {
            foreach (Transform child in player4HandArea) Destroy(child.gameObject);
        }
        
        // 清空所有卡牌列表
        player1Cards.Clear();
        player2Cards.Clear();
        player3Cards.Clear();
        player4Cards.Clear();
        
        player1SkillCards.Clear();
        
        Debug.Log("[GameManager] Cleared all 4 players' hand areas and skill areas");
    }

    /// <summary>
    /// 重置选择状态
    /// </summary>
    void ResetSelectionState()
    {
        atkConfirmed = false;
        hpConfirmed = false;
        selectedAtkCard = null;
        selectedHpCard = null;
        selectedSkill = "";
    }

    /// <summary>
    /// 显示选牌界面
    /// </summary>
    void ShowCardSelectionUI()
    {
        // Hide battle UI
        BattleUIManager battleUI = FindObjectOfType<BattleUIManager>();
        if (battleUI != null)
        {
            battleUI.HideBattleUI();
        }

        // Show card selection UI
        if (cardSelectionPanel != null)
            cardSelectionPanel.SetActive(true);

        if (confirmButton != null)
        {
            confirmButton.gameObject.SetActive(true);
            confirmButton.interactable = false;
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmClicked);
            
            // English button text
            confirmButton.GetComponentInChildren<TMP_Text>().text = $"Round {currentRound} - Select ATK";
        }
        
        Debug.Log($"[GameManager] Round {currentRound} card selection UI shown for 4 players, battle UI hidden");
    }

    /// <summary>
    /// 修改后的卡牌点击处理 - 只处理数字牌
    /// </summary>
    public void OnCardClicked(CardDisplay clicked)
    {
        CardData data = clicked.GetData();
        if (data.used) return;

        // 只有人类玩家（Player1）的卡牌可以点击
        if (data.ownerId != "Player1") return;

        // 技能牌不参与HP/ATK选择，直接返回
        if (IsSkillCard(data))
        {
            Debug.Log($"[GameManager] Skill cards cannot be selected for HP/ATK! Use them in battle phase.");
            return;
        }

        // 只处理数字牌的HP/ATK选择
        HandleNumberCardSelection(data, clicked);
    }

    /// <summary>
    /// 处理数字牌选择逻辑
    /// </summary>
    void HandleNumberCardSelection(CardData data, CardDisplay display)
    {
        // 第一阶段：选择攻击牌
        if (!atkConfirmed)
        {
            if (selectedAtkCard == data)
            {
                data.cardType = CardType.Normal;
                selectedAtkCard = null;
            }
            else
            {
                if (selectedAtkCard != null)
                {
                    selectedAtkCard.cardType = CardType.Normal;
                }

                selectedAtkCard = data;
                data.cardType = CardType.Attack;
            }
        }
        // 第二阶段：选择血量牌
        else if (atkConfirmed && !hpConfirmed)
        {
            if (selectedHpCard == data)
            {
                data.cardType = CardType.Normal;
                selectedHpCard = null;
            }
            else
            {
                if (selectedHpCard != null)
                    selectedHpCard.cardType = CardType.Normal;

                selectedHpCard = data;
                data.cardType = CardType.Defense;
            }
        }
        
        Debug.Log($"Round {currentRound} - Clicked number {data.number}, current type: {data.cardType}");

        // 刷新所有人类玩家卡牌显示
        RefreshAllPlayerCards();

        // 更新确认按钮状态和文本
        UpdateConfirmButton();
    }

    /// <summary>
    /// 更新确认按钮状态和文本
    /// </summary>
    void UpdateConfirmButton()
    {
        if (!atkConfirmed && selectedAtkCard != null)
        {
            confirmButton.interactable = true;
            confirmButton.GetComponentInChildren<TMP_Text>().text = $"Confirm ATK {selectedAtkCard.number}";
        }
        else if (atkConfirmed && !hpConfirmed && selectedHpCard != null)
        {
            confirmButton.interactable = true;
            confirmButton.GetComponentInChildren<TMP_Text>().text = $"Confirm HP {selectedHpCard.number}";
        }
        else if (atkConfirmed && !hpConfirmed)
        {
            confirmButton.interactable = false;
            confirmButton.GetComponentInChildren<TMP_Text>().text = $"Round {currentRound} - Select HP";
        }
        else
        {
            confirmButton.interactable = false;
        }
    }

    /// <summary>
    /// 刷新所有玩家的卡牌显示
    /// </summary>
    void RefreshAllPlayerCards()
    {
        // 刷新人类玩家数字牌显示
        foreach (var card in player1Cards)
            card.GetComponent<CardDisplay>().RefreshUI();
        
        // 刷新人类玩家技能牌显示
        foreach (var card in player1SkillCards)
            card.GetComponent<CardDisplay>().RefreshUI();
        
        // 刷新其他AI玩家卡牌显示（虽然是背面，但可能需要状态更新）
        foreach (var card in player2Cards)
            card.GetComponent<CardDisplay>().RefreshUI();
        foreach (var card in player3Cards)
            card.GetComponent<CardDisplay>().RefreshUI();
        foreach (var card in player4Cards)
            card.GetComponent<CardDisplay>().RefreshUI();
    }

    public void OnConfirmClicked()
    {
        if (!atkConfirmed && selectedAtkCard != null)
        {
            selectedAtkCard.used = true;
            atkConfirmed = true;
            confirmButton.interactable = false;

            Debug.Log($"Round {currentRound} - Attack confirmed: ATK = {selectedAtkCard.number}");
            
            confirmButton.GetComponentInChildren<TMP_Text>().text = $"Round {currentRound} - Select HP";
        }
        else if (atkConfirmed && !hpConfirmed && selectedHpCard != null)
        {
            selectedHpCard.used = true;
            hpConfirmed = true;
            confirmButton.interactable = true;
            
            Debug.Log($"Round {currentRound} - HP confirmed: HP = {selectedHpCard.number}");
            confirmButton.GetComponentInChildren<TMP_Text>().text = $"Start Round {currentRound} Battle";

            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(StartBattle);
        }

        RefreshAllPlayerCards();
    }

    void StartBattle()
    {
        // 记录本轮使用的卡牌（只记录数字牌）
        if (selectedHpCard.number > 0 && selectedHpCard.number <= 10)
            usedCards.Add(selectedHpCard.number);
        if (selectedAtkCard.number > 0 && selectedAtkCard.number <= 10)
            usedCards.Add(selectedAtkCard.number);
        
        // 隐藏选牌界面
        if (cardSelectionPanel != null)
            cardSelectionPanel.SetActive(false);
        
        confirmButton.gameObject.SetActive(false);
        
        Debug.Log($"Round {currentRound} Battle starts! ATK = {selectedAtkCard.number}, HP = {selectedHpCard.number}");
        Debug.Log($"Used cards so far: [{string.Join(", ", usedCards)}]");
        
        // 获取选择值
        int playerHP = selectedHpCard.number;
        int playerATK = selectedAtkCard.number;
        
        // 初始化战斗系统
        InitializeBattleSystem(playerHP, playerATK);
    }

    /// <summary>
    /// 当战斗结束后，准备下一轮（由AdvancedBattleSystem调用）
    /// </summary>
    public void OnRoundComplete()
    {
        currentRound++;
        
        // 更新技能卡牌状态（如果有的话）
        if (playerHasSkills)
        {
            UpdateBattlePhaseSkillCards();
        }
        
        if (currentRound <= 5 && usedCards.Count < 10)
        {
            Debug.Log($"[GameManager] Preparing for Round {currentRound} with 4 players");
            StartCoroutine(PrepareNextRound());
        }
        else
        {
            Debug.Log("[GameManager] All 5 rounds completed or no more cards available");
            // 游戏结束逻辑由AdvancedBattleSystem处理
        }
    }

    IEnumerator PrepareNextRound()
    {
        yield return new WaitForSeconds(2f); // 给玩家一些时间查看上轮结果
        StartRoundCardSelection();
    }

    void InitializeBattleSystem(int playerHP, int playerATK)
    {
        Debug.Log($"[GameManager] Initializing battle with HP: {playerHP}, ATK: {playerATK}");

        // 完全隐藏选牌界面
        if (cardSelectionPanel != null)
            cardSelectionPanel.SetActive(false);

        // 如果是第一轮且玩家有技能，创建技能卡牌显示
        if (currentRound == 1 && playerHasSkills)
        {
            InitializeBattlePhaseSkillCards();
        }
        else if (playerHasSkills)
        {
            // 后续轮次，更新技能卡牌状态并显示
            UpdateBattlePhaseSkillCards();
            if (player1SkillArea != null && player1SkillArea.gameObject != null)
            {
                player1SkillArea.gameObject.SetActive(true);
            }
        }

        AdvancedBattleSystem battleSystem = FindObjectOfType<AdvancedBattleSystem>();
        if (battleSystem == null)
        {
            Debug.LogError("[GameManager] AdvancedBattleSystem not found!");
            return;
        }

        battleSystem.InitializeBattle(playerHP, playerATK, currentRound);
        
        Debug.Log($"[GameManager] Battle initialized for Round {currentRound}, card selection UI hidden");
    }

    /// <summary>
    /// 获取卡牌图片 - 支持4种花色
    /// </summary>
    Sprite GetCardSprite(int number, string suit)
    {
        string cardValue = number.ToString();
        string suitName = GetSuitName(suit);
        string name = $"card{suitName}_{cardValue}";

        Debug.Log($"[GameManager] Looking for card image: {name}");

        foreach (Sprite sprite in cardSprites)
        {
            if (sprite.name == name)
            {
                Debug.Log($"[GameManager] Found matching image: {sprite.name}");
                return sprite;
            }
        }

        Debug.LogWarning($"Cannot find corresponding image: {name}");
        
        // 备用索引计算方法
        int baseIndex = number - 1; // 0-9 for values 1-10
        int suitOffset = 0;
        
        switch (suit.ToLower())
        {
            case "club": suitOffset = 0; break;
            case "diamond": suitOffset = 13; break;
            case "heart": suitOffset = 26; break;
            case "spade": suitOffset = 39; break;
        }
        
        int index = baseIndex + suitOffset;
        if (index >= 0 && index < cardSprites.Length)
        {
            Debug.Log($"[GameManager] Using fallback index {index}: {cardSprites[index].name}");
            return cardSprites[index];
        }
        
        // 最后的备用方案
        return cardSprites.Length > 0 ? cardSprites[0] : null;
    }

    // 其他工具方法
    public (int hp, int atk, string skill) GetSelectedValues()
    {
        int hp = selectedHpCard != null ? selectedHpCard.number : 0;
        int atk = selectedAtkCard != null ? selectedAtkCard.number : 0;
        string skill = ""; // 技能通过SkillUIManager单独处理
        return (hp, atk, skill);
    }

    public bool IsSelectionComplete()
    {
        return atkConfirmed && hpConfirmed;
    }

    public int GetCurrentRound()
    {
        return currentRound;
    }

    public List<int> GetUsedCards()
    {
        return new List<int>(usedCards);
    }

    /// <summary>
    /// 获取所有玩家的手牌数量（用于调试）
    /// </summary>
    public void LogAllPlayersCardCounts()
    {
        Debug.Log($"[GameManager] Card counts - Player1: {player1Cards.Count}, Player2: {player2Cards.Count}, Player3: {player3Cards.Count}, Player4: {player4Cards.Count}");
    }
}