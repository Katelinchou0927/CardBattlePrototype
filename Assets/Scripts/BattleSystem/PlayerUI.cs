using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PlayerUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image avatar;
    public TMP_Text nameText;
    public TMP_Text hpText;
    public TMP_Text attackText;
    public GameObject eliminatedOverlay;

    [Header("状态指示器")]
    public Image attackerHighlight;
    public Image targetHighlight;
    public GameObject damageEffect;

    [Header("Color Settings")]
    public Color highHPColor = Color.green;
    public Color mediumHPColor = Color.yellow;
    public Color lowHPColor = Color.red;

    // 玩家引用
    public Player Player { get; private set; }

    public void Initialize(Player player)
    {
        // 设置玩家引用
        Player = player;
        UpdateUI(player);
    }

    public void UpdateUI(Player player)
    {
        // 更新玩家引用
        Player = player;

        nameText.text = player.PlayerName;
        hpText.text = $"HP: {player.CurrentHP}";
        attackText.text = $"ATK: {player.AttackPower}";

        // 根据血量设置颜色
        if (player.CurrentHP > 50)
        {
            hpText.color = highHPColor;
        }
        else if (player.CurrentHP > 20)
        {
            hpText.color = mediumHPColor;
        }
        else
        {
            hpText.color = lowHPColor;
        }

        // 更新淘汰状态
        if (eliminatedOverlay != null)
        {
            eliminatedOverlay.SetActive(player.IsEliminated);
        }
    }

    public void SetAttackerHighlight(bool active)
    {
        if (attackerHighlight != null)
        {
            attackerHighlight.gameObject.SetActive(active);
        }
    }

    public void SetTargetHighlight(bool active)
    {
        if (targetHighlight != null)
        {
            targetHighlight.gameObject.SetActive(active);
        }
    }

    public void PlayDamageEffect()
    {
        StartCoroutine(DamageAnimation());
    }

    private IEnumerator DamageAnimation()
    {
        Image panel = GetComponent<Image>();
        if (panel == null) yield break;

        Color originalColor = panel.color;

        // 闪烁效果
        for (int i = 0; i < 2; i++)
        {
            panel.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            panel.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void PlayEliminatedEffect()
    {
        StartCoroutine(EliminatedAnimation());
    }

    private IEnumerator EliminatedAnimation()
    {
        if (eliminatedOverlay == null) yield break;

        eliminatedOverlay.SetActive(true);
        Image overlayImage = eliminatedOverlay.GetComponent<Image>();
        TMP_Text text = eliminatedOverlay.GetComponentInChildren<TMP_Text>();

        if (overlayImage == null || text == null) yield break;

        Color targetColor = overlayImage.color;
        Color transparent = new Color(targetColor.r, targetColor.g, targetColor.b, 0);

        float duration = 1.0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            overlayImage.color = Color.Lerp(transparent, targetColor, t);
            text.color = new Color(1, 0, 0, t);
            text.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);

            yield return null;
        }
    }
}