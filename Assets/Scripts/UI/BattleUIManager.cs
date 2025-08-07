using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject battlePanel;           // 战斗主面板
    public GameObject playerCardPrefab;      // 玩家卡片预制体
    public Transform playersContainer;       // 玩家容器
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
        public Image attackHighlight;
        public Image targetHighlight;
    }

    void Awake()
    {
        // 确保战斗面板初始时隐藏
        if (battlePanel != null)
            battlePanel.SetActive(false);
    }

    public void ShowBattleUI()
    {
        if (battlePanel != null)
            battlePanel.SetActive(true);
        
        Debug.Log("[BattleUI] Battle UI shown");
    }

    public void HideBattleUI()
    {
        if (battlePanel != null)
            battlePanel.SetActive(false);
        
        Debug.Log("[BattleUI] Battle UI hidden");
    }

    public void InitializePlayers(List<BattlePlayer> players)
    {
        Debug.Log($"[BattleUI] Initializing UI for {players.Count} players");
        
        // 清除现有玩家卡片
        ClearPlayerCards();

        // 为每个玩家创建UI卡片
        foreach (BattlePlayer player in players)
        {
            CreatePlayerCard(player);
        }

        // 显示战斗UI
        ShowBattleUI();
        
        // 初始化战斗信息
        UpdateRoundInfo(1, 0);
        AddBattleLog("Battle Start!");
        AddBattleLog($"{players.Count} players enter the battlefield!");
    }

    void CreatePlayerCard(BattlePlayer player)
    {
        if (playerCardPrefab == null || playersContainer == null)
        {
            Debug.LogError("[BattleUI] Player card prefab or container not assigned!");
            return;
        }

        GameObject cardObj = Instantiate(playerCardPrefab, playersContainer);
        PlayerCard playerCard = new PlayerCard
        {
            player = player,
            cardObject = cardObj
        };

        // 查找UI组件
        playerCard.nameText = cardObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        playerCard.hpText = cardObj.transform.Find("HPText")?.GetComponent<TextMeshProUGUI>();
        playerCard.attackText = cardObj.transform.Find("AttackText")?.GetComponent<TextMeshProUGUI>();
        playerCard.hpSlider = cardObj.transform.Find("HPSlider")?.GetComponent<Slider>();
        playerCard.backgroundImage = cardObj.GetComponent<Image>();
        playerCard.attackHighlight = cardObj.transform.Find("AttackHighlight")?.GetComponent<Image>();
        playerCard.targetHighlight = cardObj.transform.Find("TargetHighlight")?.GetComponent<Image>();

        // 初始化UI显示
        UpdatePlayerCard(playerCard);
        
        // 添加到列表
        playerCards.Add(playerCard);
        
        Debug.Log($"[BattleUI] Created card for {player.name}");
    }

    void UpdatePlayerCard(PlayerCard playerCard)
    {
        BattlePlayer player = playerCard.player;

        // 更新文本
        if (playerCard.nameText != null)
            playerCard.nameText.text = player.name;

        if (playerCard.hpText != null)
            playerCard.hpText.text = $"HP: {player.currentHP}/{player.maxHP}";

        if (playerCard.attackText != null)
            playerCard.attackText.text = $"ATK: {player.attack}";

        // 更新血量条
        if (playerCard.hpSlider != null)
        {
            playerCard.hpSlider.maxValue = player.maxHP;
            playerCard.hpSlider.value = player.currentHP;
        }

        // 更新背景颜色表示状态
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

        // 隐藏高亮效果
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

        // 显示攻击者高亮
        if (attackerCard?.attackHighlight != null)
        {
            attackerCard.attackHighlight.gameObject.SetActive(true);
            attackerCard.attackHighlight.color = Color.red;
        }

        // 显示目标高亮
        if (targetCard?.targetHighlight != null)
        {
            targetCard.targetHighlight.gameObject.SetActive(true);
            targetCard.targetHighlight.color = Color.blue;
        }

        AddBattleLog($"{attacker.name} attacks {target.name}!");

        yield return new WaitForSeconds(attackAnimationDuration);

        // 隐藏高亮
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
            // 闪烁效果
            Image bg = targetCard.backgroundImage;
            Color originalColor = bg.color;
            
            for (int i = 0; i < 3; i++)
            {
                bg.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                bg.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }

            // 更新UI
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
            
            // 高亮胜利者
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
        battleLog = message + "\n" + battleLog;
        
        if (battleLogText != null)
        {
            battleLogText.text = battleLog;
            
            // 限制日志长度
            if (battleLog.Length > 1000)
            {
                battleLog = battleLog.Substring(0, 1000);
                battleLogText.text = battleLog;
            }
        }

        // 自动滚动到底部
        if (logScrollRect != null)
        {
            StartCoroutine(ScrollToBottom());
        }

        Debug.Log($"[BattleUI] {message}");
    }

    IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        logScrollRect.verticalNormalizedPosition = 0f;
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