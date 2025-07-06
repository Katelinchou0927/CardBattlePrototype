using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI 组件")]
    public TMP_Text roundText;
    public TMP_Text turnText;
    public TMP_Text logText;
    public GameObject playerUIPrefab;
    public Transform playersPanel;
    public GameObject damagePopupPrefab;

    [HideInInspector]
    public List<PlayerUI> playerUIs = new List<PlayerUI>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (playersPanel == null)
            {
                Debug.LogWarning("UIManager: playersPanel is not assigned on Awake");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 订阅事件
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnTargetSelected += HandleTargetSelected;
            BattleManager.Instance.OnPlayerAttack += HandlePlayerAttack;
            BattleManager.Instance.OnDamageCalculated += HandleDamageCalculated;
            BattleManager.Instance.OnPlayerEliminated += HandlePlayerEliminated;
            BattleManager.Instance.OnTurnEnd += HandleTurnEnd;
        }
    }

    private void OnDestroy()
    {
        // 取消订阅
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnTargetSelected -= HandleTargetSelected;
            BattleManager.Instance.OnPlayerAttack -= HandlePlayerAttack;
            BattleManager.Instance.OnDamageCalculated -= HandleDamageCalculated;
            BattleManager.Instance.OnPlayerEliminated -= HandlePlayerEliminated;
            BattleManager.Instance.OnTurnEnd -= HandleTurnEnd;
        }
    }

    // 设置玩家UI
    public void SetupPlayerUI(List<Player> players)
    {
        // 添加空值检查
        if (playersPanel == null)
        {
            Debug.LogError("UIManager: PlayersPanel is not assigned! Please assign in Inspector.");
            return;
        }

        // 清除现有UI
        foreach (Transform child in playersPanel)
        {
            Destroy(child.gameObject);
        }
        playerUIs = new List<PlayerUI>();

        // 添加空值检查
        if (playerUIPrefab == null)
        {
            Debug.LogError("UIManager: PlayerUIPrefab is not assigned!");
        }
        // 创建新UI
        foreach (Player player in players)
        {
            GameObject uiObj = Instantiate(playerUIPrefab, playersPanel);
            PlayerUI playerUI = uiObj.GetComponent<PlayerUI>();
            playerUI.Initialize(player);
            playerUIs.Add(playerUI);
        }
    }

    private IEnumerator AnimateText(TMP_Text textElement)
    {
        Color originalColor = textElement.color;
        Vector3 originalScale = textElement.transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            textElement.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            textElement.color = Color.Lerp(originalColor, Color.yellow, t);

            yield return null;
        }

        textElement.transform.localScale = originalScale;
        textElement.color = originalColor;
    }

    // 更新回合信息
    public void UpdateRoundInfo(int round, int turn)
    {
        if (roundText)
        {
            roundText.text = $"回合: {round}";
            StartCoroutine(AnimateText(roundText));
        }

        if (turnText)
        {
            turnText.text = $"轮次: {turn}";
            StartCoroutine(AnimateText(turnText));
        }
    }

    // 添加日志
    public void AddLog(string message)
    {
        if (logText)
        {
            logText.text = message + "\n" + logText.text;

            if (logText.text.Length > 1000)
            {
                logText.text = logText.text.Substring(0, 1000);
            }
        }
    }

    // 高亮当前攻击者
    public void HighlightAttacker(Player attacker)
    {
        foreach (PlayerUI ui in playerUIs)
        {
            if (ui.Player != null)
            {
                ui.SetAttackerHighlight(ui.Player == attacker);
            }
        }
    }

    // 高亮当前目标
    public void HighlightTarget(Player target)
    {
        foreach (PlayerUI ui in playerUIs)
        {
            if (ui.Player != null)
            {
                ui.SetTargetHighlight(ui.Player == target);
            }
        }
    }

    // 显示胜利者
    public void ShowWinner(Player winner)
    {
        if (logText)
        {
            logText.text = $"<color=green><b>胜利者: {winner.PlayerName}</b></color>\n" + logText.text;
        }
    }

    // 更新单个玩家UI
    public void UpdatePlayerUI(Player player)
    {
        foreach (PlayerUI ui in playerUIs)
        {
            if (ui.Player == player)
            {
                ui.UpdateUI(player);
                return;
            }
        }
    }

    // 显示伤害弹出效果
    public void ShowDamagePopup(Vector3 position, int damage)
    {
        if (damagePopupPrefab == null) return;

        GameObject popup = Instantiate(damagePopupPrefab, position, Quaternion.identity);
        TMP_Text damageText = popup.GetComponentInChildren<TMP_Text>();

        if (damageText != null)
        {
            damageText.text = $"-{damage}";
            StartCoroutine(DamagePopupAnimation(popup, damageText));
        }
    }

    private IEnumerator DamagePopupAnimation(GameObject popup, TMP_Text text)
    {
        float duration = 1.5f;
        float elapsed = 0f;
        Vector3 startPos = popup.transform.position;
        Vector3 endPos = startPos + Vector3.up * 2f;
        Color startColor = text.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            popup.transform.position = Vector3.Lerp(startPos, endPos, t);
            text.color = Color.Lerp(startColor, endColor, t);

            yield return null;
        }

        Destroy(popup);
    }

    // 事件处理
    private void HandleTargetSelected(Player attacker, Player target)
    {
        HighlightAttacker(attacker);
        HighlightTarget(target);
        AddLog($"<color=#FFA500>{attacker.PlayerName} 锁定目标: {target.PlayerName}</color>");
    }

    private void HandlePlayerAttack(Player attacker, Player target)
    {
        AddLog($"<color=#FFD700>{attacker.PlayerName} 攻击了 {target.PlayerName}!</color>");
    }

    private void HandleDamageCalculated(Player attacker, Player target, int damage)
    {
        AddLog($"<color=#FF0000>{attacker.PlayerName} 造成 {damage} 点伤害!</color>");
        ShowDamagePopup(target.transform.position + Vector3.up * 2, damage);
    }

    private void HandlePlayerEliminated(Player player)
    {
        AddLog($"<color=#8B0000>{player.PlayerName} 被淘汰!</color>");
    }

    private void HandleTurnEnd(Player attacker)
    {
        HighlightAttacker(null);
        HighlightTarget(null);
    }
}