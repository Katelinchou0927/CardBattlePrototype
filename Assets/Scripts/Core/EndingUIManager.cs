using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class EndingUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject endingPanel;
    public TextMeshProUGUI endingText;
    public Image endingBackgroundImage;
    public Button returnToMenuButton;
    public CanvasGroup endingPanelCanvasGroup;
    
    [Header("Ending Backgrounds (Optional)")]
    public Sprite silencersVictoryBackground;  // Silencers胜利背景
    public Sprite systemVictoryBackground;      // 系统胜利背景
    
    [Header("Silencers Victory Ending")]
    [TextArea(10, 20)]
    public string silencersVictoryText = @"__Emergency Bulletin__

The Silencers carried out a coordinated strike on the Central Signal Grid before dawn, disabling core surveillance modules across multiple districts. Large sectors of the city experienced sudden memory restitution and uncontrolled information flow. Officials warn of ""cognitive instability"" and urge citizens to remain compliant.

Enforcement units suffered heavy losses during the breach of the Nuwa maintenance wing. Control has not been fully restored, and the system continues to operate in a degraded state. Analysts describe the event as the most serious disruption in recent years.

The Silencers broadcast a brief statement claiming this marks ""the beginning of restoration.""

Authorities deny any collapse of order.";
    
    [Header("System Victory Ending")]
    [TextArea(10, 20)]
    public string systemVictoryText = @"__Authorized Release__

Administrative forces successfully neutralized the extremist group responsible for last night's attempted attack on the Central Signal Grid. The system remained stable, and all compromised nodes were restored within minutes.

The northern cell of the Silencers has been dismantled. Surviving members were taken into custody for cognitive evaluation, and several external districts have been placed under reinforced supervision. Additional memory audits will be conducted citywide to prevent ideological spread.

Officials declare the situation resolved and confirm that order has been ""fully re-established.""

Deviation metrics remain below critical thresholds.";
    
    [Header("Animation Settings")]
    public float fadeInDuration = 1.5f;
    public bool useTypewriterEffect = true;
    public float typingSpeed = 0.03f;
    
    private Coroutine typewriterCoroutine;
    
    void Start()
    {
        if (endingPanel != null)
            endingPanel.SetActive(false);
            
        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.RemoveAllListeners();
            returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
        }
        
        if (endingPanelCanvasGroup != null)
            endingPanelCanvasGroup.alpha = 0f;
    }
    
    /// <summary>
    /// 显示结局界面 - 根据胜利者角色决定结局
    /// </summary>
    public void ShowEnding(PlayerData winner)
    {
        if (endingPanel == null) return;
        
        endingPanel.SetActive(true);
        
        // 根据胜利者的角色索引决定结局类型
        string endingContent = "";
        Sprite backgroundToUse = null;
        
        // 角色索引：
        // 0 = Anne Percy (Enforcer/系统方)
        // 1 = Freya Percy (Silencer)
        // 2 = Lena Moro (Silencer)
        
        if (winner.characterIndex == 0) // Anne Percy 胜利
        {
            // 系统胜利结局
            endingContent = systemVictoryText;
            backgroundToUse = systemVictoryBackground;
            Debug.Log("[EndingUI] System Victory - Anne Percy wins");
        }
        else // Freya Percy (1) 或 Lena Moro (2) 胜利
        {
            // Silencers胜利结局
            endingContent = silencersVictoryText;
            backgroundToUse = silencersVictoryBackground;
            Debug.Log($"[EndingUI] Silencers Victory - {winner.characterName} wins");
        }
        
        // 设置背景图片（如果有的话）
        if (endingBackgroundImage != null && backgroundToUse != null)
        {
            endingBackgroundImage.sprite = backgroundToUse;
        }
        
        StartCoroutine(ShowEndingWithEffects(endingContent));
        
        Debug.Log($"[EndingUI] Showing ending for winner: {winner.characterName} (Index: {winner.characterIndex})");
    }
    
    IEnumerator ShowEndingWithEffects(string content)
    {
        if (endingText != null)
            endingText.text = "";
        
        if (endingPanelCanvasGroup != null)
        {
            yield return StartCoroutine(FadeIn(endingPanelCanvasGroup, fadeInDuration));
        }
        
        if (endingText != null && useTypewriterEffect)
        {
            typewriterCoroutine = StartCoroutine(TypewriterEffect(content));
        }
        else if (endingText != null)
        {
            endingText.text = content;
        }
    }
    
    IEnumerator TypewriterEffect(string fullText)
    {
        endingText.text = "";
        
        foreach (char c in fullText)
        {
            endingText.text += c;
            
            float delay = typingSpeed;
            if (c == '.' || c == '!' || c == '?')
                delay = typingSpeed * 5f;
            else if (c == ',' || c == ';')
                delay = typingSpeed * 3f;
            else if (c == '\n')
                delay = typingSpeed * 8f;
            
            yield return new WaitForSeconds(delay);
        }
        
        typewriterCoroutine = null;
    }
    
    IEnumerator FadeIn(CanvasGroup canvasGroup, float duration)
    {
        float elapsed = 0f;
        canvasGroup.alpha = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    void OnReturnToMenuClicked()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
        
        SceneManager.LoadScene("StartScene");
    }
    
    public void HideEnding()
    {
        if (endingPanel != null)
            endingPanel.SetActive(false);
    }
}