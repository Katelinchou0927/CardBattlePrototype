using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject battlePanel;           // 战斗主面板

    [Header("Player Card Areas - 4 Positions")]
    public Transform player1CardArea;        // 下方（人类玩家）
    public Transform player2CardArea;        // 上方（AI玩家1）
    public Transform player3CardArea;        // 左侧（AI玩家2）
    public Transform player4CardArea;        // 右侧（AI玩家3）

    [Header("Player Card Prefab")]
    public GameObject playerCardPrefab;      // 玩家卡片预制体

    [Header("Character Art Resources")]
    public Sprite[] characterArtSprites;     // 角色美术资源数组

    [Header("Battle Info")]
    public TextMeshProUGUI roundText;        // 回合显示
    public TextMeshProUGUI turnText;         // 轮次显示
    public TextMeshProUGUI battleLogText;    // 战斗日志
    public ScrollRect logScrollRect;         // 日志滚动区域

    [Header("Animation Settings")]
    public float attackAnimationDuration = 1f;
    public float damageAnimationDuration = 0.5f;

    private List<PlayerCard> playerCards = new List<PlayerCard>();
    private string battleLog = "";

    public class PlayerCard
    {
        public BattlePlayer player;
        public GameObject cardObject;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI hpText;
        public TextMeshProUGUI attackText;
        public Slider hpSlider;
        public Image backgroundImage;
        public Image characterArtImage;      // 角色美术图片
        public Image attackHighlight;
        public Image targetHighlight;
        public int playerIndex;              // 玩家索引（0-3）
    }

    void Awake()
    {
        // 确保战斗面板初始时隐藏
        if (battlePanel != null)
            battlePanel.SetActive(false);
    
        // 初始化日志系统
        InitializeLogSystem();
    }

    void InitializeLogSystem()
    {
        if (battleLogText != null)
        {
            battleLogText.text = "";
            battleLog = "";
        
            RectTransform logTextRect = battleLogText.GetComponent<RectTransform>();
            RectTransform contentRect = logTextRect.parent.GetComponent<RectTransform>();
        
            logTextRect.anchorMin = new Vector2(0, 1);
            logTextRect.anchorMax = new Vector2(1, 1);
            logTextRect.pivot = new Vector2(0.5f, 1);
        
            logTextRect.offsetMin = new Vector2(10, -100);
            logTextRect.offsetMax = new Vector2(-10, 0);
        
            if (contentRect != null)
            {
                contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, 100);
            }
        }
    }

    public void ShowBattleUI()
    {
        if (battlePanel != null)
            battlePanel.SetActive(true);
        
        Debug.Log("[BattleUI] Battle UI shown with 4-player layout");
    }

    public void HideBattleUI()
    {
        if (battlePanel != null)
            battlePanel.SetActive(false);
        
        Debug.Log("[BattleUI] Battle UI hidden");
    }

    public void InitializePlayers(List<BattlePlayer> players)
    {
        Debug.Log($"[BattleUI] Initializing UI for {players.Count} players in 4-position layout");
        
        ClearPlayerCards();

        for (int i = 0; i < players.Count && i < 4; i++)
        {
            CreatePlayerCard(players[i], i);
        }

        ShowBattleUI();
        
        UpdateRoundInfo(1, 0);
        AddBattleLog("Battle Start!");
        AddBattleLog($"{players.Count} players enter the battlefield!");
    }

    void CreatePlayerCard(BattlePlayer player, int playerIndex)
    {
        Transform targetArea = GetPlayerCardArea(playerIndex);
        
        if (targetArea == null || playerCardPrefab == null)
        {
            Debug.LogError($"[BattleUI] Cannot create card for player {playerIndex} - missing references!");
            return;
        }

        GameObject cardObj = Instantiate(playerCardPrefab, targetArea);
        PlayerCard playerCard = new PlayerCard
        {
            player = player,
            cardObject = cardObj,
            playerIndex = playerIndex
        };

        // 查找UI组件
        Transform[] allChildren = cardObj.GetComponentsInChildren<Transform>();
        
        foreach (Transform child in allChildren)
        {
            string childName = child.name.ToLower();
            
            if (childName.Contains("name"))
                playerCard.nameText = child.GetComponent<TextMeshProUGUI>();
            else if (childName.Contains("hp") && childName.Contains("text"))
                playerCard.hpText = child.GetComponent<TextMeshProUGUI>();
            else if (childName.Contains("attack") && childName.Contains("text"))
                playerCard.attackText = child.GetComponent<TextMeshProUGUI>();
            else if (childName.Contains("slider"))
                playerCard.hpSlider = child.GetComponent<Slider>();
            else if (childName.Contains("characterart") || childName.Contains("character"))
                playerCard.characterArtImage = child.GetComponent<Image>();
            else if (childName.Contains("attackhighlight"))
                playerCard.attackHighlight = child.GetComponent<Image>();
            else if (childName.Contains("targethighlight"))
                playerCard.targetHighlight = child.GetComponent<Image>();
        }

        playerCard.backgroundImage = cardObj.GetComponent<Image>();

        // Debug输出
        Debug.Log($"[BattleUI] Player {playerIndex} components found:");
        Debug.Log($"- NameText: {(playerCard.nameText != null ? "✓" : "✗")}");
        Debug.Log($"- HPText: {(playerCard.hpText != null ? "✓" : "✗")}");
        Debug.Log($"- AttackText: {(playerCard.attackText != null ? "✓" : "✗")}");
        Debug.Log($"- HPSlider: {(playerCard.hpSlider != null ? "✓" : "✗")}");
        Debug.Log($"- CharacterArt: {(playerCard.characterArtImage != null ? "✓" : "✗")}");

        // 设置角色美术
        SetCharacterArt(playerCard, playerIndex);

        // 初始化UI显示
        UpdatePlayerCard(playerCard);
        
        playerCards.Add(playerCard);
        
        Debug.Log($"[BattleUI] Created card for {player.name} at position {playerIndex}");
    }

    Transform GetPlayerCardArea(int playerIndex)
    {
        switch (playerIndex)
        {
            case 0: return player1CardArea; // 下方（人类玩家）
            case 1: return player2CardArea; // 上方（AI玩家1）
            case 2: return player3CardArea; // 左侧（AI玩家2）
            case 3: return player4CardArea; // 右侧（AI玩家3）
            default: 
                Debug.LogWarning($"[BattleUI] Invalid player index: {playerIndex}");
                return player1CardArea;
        }
    }

    void SetCharacterArt(PlayerCard playerCard, int playerIndex)
    {
        if (playerCard.characterArtImage == null) return;

        // 使用 BattlePlayer 中存储的角色索引，而不是玩家位置索引
        int characterIndex = playerCard.player.characterIndex;
    
        if (characterArtSprites != null && characterIndex < characterArtSprites.Length)
        {
            playerCard.characterArtImage.sprite = characterArtSprites[characterIndex];
            Debug.Log($"[BattleUI] Set character art for {playerCard.player.name} using character index {characterIndex}");
        }
        else
        {
            Debug.LogWarning($"[BattleUI] No character art found for character index {characterIndex}");
        }
    }

    void UpdatePlayerCard(PlayerCard playerCard)
    {
        BattlePlayer player = playerCard.player;

        if (playerCard.nameText != null)
            playerCard.nameText.text = player.name;

        if (playerCard.hpText != null)
            playerCard.hpText.text = $"HP: {player.currentHP}/{player.maxHP}";

        if (playerCard.attackText != null)
            playerCard.attackText.text = $"ATK: {player.attack}";

        if (playerCard.hpSlider != null)
        {
            playerCard.hpSlider.maxValue = player.maxHP;
            playerCard.hpSlider.value = player.currentHP;
        }

        if (playerCard.backgroundImage != null)
        {
            if (player.isEliminated)
            {
                playerCard.backgroundImage.color = Color.gray;
            }
            else if (player.currentHP < player.maxHP * 0.3f)
            {
                playerCard.backgroundImage.color = Color.red;
            }
            else if (player.currentHP < player.maxHP * 0.6f)
            {
                playerCard.backgroundImage.color = Color.yellow;
            }
            else
            {
                playerCard.backgroundImage.color = Color.white;
            }
        }

        if (playerCard.attackHighlight != null)
            playerCard.attackHighlight.gameObject.SetActive(false);
        
        if (playerCard.targetHighlight != null)
            playerCard.targetHighlight.gameObject.SetActive(false);
    }

    public void UpdateAllPlayerCards()
    {
        foreach (PlayerCard playerCard in playerCards)
        {
            UpdatePlayerCard(playerCard);
        }
    }

    public void UpdateRoundInfo(int round, int turn)
    {
        if (roundText != null)
            roundText.text = $"Round: {round}";
        
        if (turnText != null)
            turnText.text = $"Turn: {turn}";
    }

    public void ShowAttackAnimation(BattlePlayer attacker, BattlePlayer target)
    {
        StartCoroutine(AttackAnimationCoroutine(attacker, target));
    }

    IEnumerator AttackAnimationCoroutine(BattlePlayer attacker, BattlePlayer target)
    {
        PlayerCard attackerCard = GetPlayerCard(attacker);
        PlayerCard targetCard = GetPlayerCard(target);

        if (attackerCard?.attackHighlight != null)
        {
            attackerCard.attackHighlight.gameObject.SetActive(true);
            attackerCard.attackHighlight.color = Color.red;
        }

        if (targetCard?.targetHighlight != null)
        {
            targetCard.targetHighlight.gameObject.SetActive(true);
            targetCard.targetHighlight.color = Color.blue;
        }

        AddBattleLog($"{attacker.name} attacks {target.name}!");

        yield return new WaitForSeconds(attackAnimationDuration);

        if (attackerCard?.attackHighlight != null)
            attackerCard.attackHighlight.gameObject.SetActive(false);
        
        if (targetCard?.targetHighlight != null)
            targetCard.targetHighlight.gameObject.SetActive(false);
    }

    public void ShowDamageAnimation(BattlePlayer target, int damage)
    {
        StartCoroutine(DamageAnimationCoroutine(target, damage));
    }

    IEnumerator DamageAnimationCoroutine(BattlePlayer target, int damage)
    {
        PlayerCard targetCard = GetPlayerCard(target);
        
        if (targetCard != null)
        {
            Image bg = targetCard.backgroundImage;
            Color originalColor = bg.color;
            
            for (int i = 0; i < 3; i++)
            {
                bg.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                bg.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }

            UpdatePlayerCard(targetCard);
        }

        AddBattleLog($"{target.name} takes {damage} damage!");
        
        if (target.isEliminated)
        {
            AddBattleLog($"{target.name} is eliminated!");
        }
    }

    public void ShowBattleEnd(BattlePlayer winner)
    {
        if (winner != null)
        {
            AddBattleLog($"BATTLE END! {winner.name} wins!");
            
            PlayerCard winnerCard = GetPlayerCard(winner);
            if (winnerCard?.backgroundImage != null)
            {
                winnerCard.backgroundImage.color = Color.green;
            }
        }
        else
        {
            AddBattleLog("BATTLE END! No survivors!");
        }
    }

    PlayerCard GetPlayerCard(BattlePlayer player)
    {
        foreach (PlayerCard card in playerCards)
        {
            if (card.player == player)
                return card;
        }
        return null;
    }

    public void AddBattleLog(string message)
    {
        battleLog = battleLog + message + "\n";
    
        if (battleLogText != null)
        {
            battleLogText.text = battleLog;
        
            if (battleLog.Length > 2000)
            {
                battleLog = battleLog.Substring(battleLog.Length - 2000);
                battleLogText.text = battleLog;
            }
        
            battleLogText.ForceMeshUpdate();
        
            float textHeight = battleLogText.preferredHeight;
        
            RectTransform logTextRect = battleLogText.GetComponent<RectTransform>();
            logTextRect.sizeDelta = new Vector2(logTextRect.sizeDelta.x, textHeight);
        
            RectTransform contentRect = logTextRect.parent.GetComponent<RectTransform>();
            if (contentRect != null)
            {
                contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, textHeight + 20);
            }
        }
    
        if (logScrollRect != null)
        {
            StartCoroutine(ScrollToBottomNextFrame());
        }
    }

    IEnumerator ScrollToBottomNextFrame()
    {
        yield return new WaitForEndOfFrame();
    
        if (logScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
        
            if (logScrollRect.content != null)
            {
                float contentHeight = logScrollRect.content.rect.height;
                float viewportHeight = logScrollRect.viewport.rect.height;
            
                if (contentHeight > viewportHeight)
                {
                    logScrollRect.verticalNormalizedPosition = 0f;
                }
            }
        }
    }

    void ClearPlayerCards()
    {
        foreach (PlayerCard card in playerCards)
        {
            if (card.cardObject != null)
                Destroy(card.cardObject);
        }
        playerCards.Clear();
    }

    void OnDestroy()
    {
        ClearPlayerCards();
    }
}