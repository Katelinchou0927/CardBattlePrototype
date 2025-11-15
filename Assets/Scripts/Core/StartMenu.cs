using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StartMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject startButton;
    public GameObject quitButton;        // 新增这行
    public CharacterSelectionUI characterSelectionUI;
    
    [Header("Story Intro Panel")]
    public GameObject storyIntroPanel;
    public Button continueButton;
    public TextMeshProUGUI introText;
    public CanvasGroup storyPanelCanvasGroup;
    
    [Header("Story Pages")]
    [TextArea(5, 10)]
    public string[] storyPages;  // 多页文字数组
    
    [Header("Animation Settings")]
    public float fadeInDuration = 1.0f;
    public float fadeOutDuration = 0.5f;
    
    [Header("Typewriter Settings")]
    public float typingSpeed = 0.05f;
    public bool useTypewriterEffect = true;
    public AudioSource typingSound;
    
    private int currentPageIndex = 0;
    private Coroutine typewriterCoroutine;
    private bool isTyping = false;
    
    void Start()
    {
        ShowStartButton();
        
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueClicked);
        }
        
        if (storyPanelCanvasGroup != null)
        {
            storyPanelCanvasGroup.alpha = 0f;
        }
    }
    
    void ShowStartButton()
    {
        if (startButton != null)
            startButton.SetActive(true);
    
        if (quitButton != null)              // 新增
            quitButton.SetActive(true);      // 新增
        
        if (storyIntroPanel != null)
            storyIntroPanel.SetActive(false);
        
        if (characterSelectionUI != null)
            characterSelectionUI.HideCharacterSelection();
        
        currentPageIndex = 0;
    }
    
    public void StartGame()
    {
        Debug.Log("[StartMenu] Start Game clicked - showing story intro");
    
        if (startButton != null)
            startButton.SetActive(false);
    
        if (quitButton != null)              // 新增
            quitButton.SetActive(false);     // 新增
            
        if (storyIntroPanel != null && storyPages != null && storyPages.Length > 0)
        {
            storyIntroPanel.SetActive(true);
            currentPageIndex = 0;
            StartCoroutine(ShowStoryIntroWithEffects());
        }
        else
        {
            Debug.LogWarning("[StartMenu] StoryIntroPanel or storyPages not assigned, skipping to character selection");
            ShowCharacterSelection();
        }
    }
    
    IEnumerator ShowStoryIntroWithEffects()
    {
        if (introText != null)
        {
            introText.text = "";
        }
        
        if (storyPanelCanvasGroup != null)
        {
            yield return StartCoroutine(FadeIn(storyPanelCanvasGroup, fadeInDuration));
        }
        
        ShowCurrentPage();
    }
    
    void ShowCurrentPage()
    {
        if (introText == null || storyPages == null || currentPageIndex >= storyPages.Length)
            return;
        
        string currentText = storyPages[currentPageIndex];
        
        if (useTypewriterEffect)
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
            }
            typewriterCoroutine = StartCoroutine(TypewriterEffect(currentText));
        }
        else
        {
            introText.text = currentText;
        }
        
        Debug.Log($"[StartMenu] Showing page {currentPageIndex + 1} of {storyPages.Length}");
    }
    
    IEnumerator TypewriterEffect(string fullText)
    {
        isTyping = true;
        introText.text = "";
        
        foreach (char c in fullText)
        {
            introText.text += c;
            
            if (typingSound != null && c != ' ' && c != '\n')
            {
                typingSound.Play();
            }
            
            float delay = typingSpeed;
            if (c == '.' || c == '!' || c == '?')
            {
                delay = typingSpeed * 5f;
            }
            else if (c == ',' || c == ';' || c == ':')
            {
                delay = typingSpeed * 3f;
            }
            else if (c == '\n')
            {
                delay = typingSpeed * 8f;
            }
            
            yield return new WaitForSeconds(delay);
        }
        
        isTyping = false;
        typewriterCoroutine = null;
    }
    
    void OnContinueClicked()
    {
        // 如果正在打字，先完成当前页面
        if (isTyping && typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
            isTyping = false;
            
            if (introText != null && storyPages != null && currentPageIndex < storyPages.Length)
            {
                introText.text = storyPages[currentPageIndex];
            }
            return;  // 第一次点击只是完成打字，不翻页
        }
        
        // 检查是否还有下一页
        if (storyPages != null && currentPageIndex < storyPages.Length - 1)
        {
            // 显示下一页
            currentPageIndex++;
            ShowCurrentPage();
            Debug.Log($"[StartMenu] Moving to page {currentPageIndex + 1}");
        }
        else
        {
            // 最后一页，进入角色选择
            Debug.Log("[StartMenu] Last page finished - showing character selection");
            StartCoroutine(FadeOutAndShowCharacterSelection());
        }
    }
    
    IEnumerator FadeOutAndShowCharacterSelection()
    {
        if (storyPanelCanvasGroup != null)
        {
            yield return StartCoroutine(FadeOut(storyPanelCanvasGroup, fadeOutDuration));
        }
        
        if (storyIntroPanel != null)
            storyIntroPanel.SetActive(false);
            
        ShowCharacterSelection();
    }
    
    IEnumerator FadeIn(CanvasGroup canvasGroup, float duration)
    {
        if (canvasGroup == null) yield break;
        
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
    
    IEnumerator FadeOut(CanvasGroup canvasGroup, float duration)
    {
        if (canvasGroup == null) yield break;
        
        float elapsed = 0f;
        canvasGroup.alpha = 1f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
    }
    
    void ShowCharacterSelection()
    {
        if (characterSelectionUI != null)
        {
            characterSelectionUI.ShowCharacterSelection();
        }
        else
        {
            Debug.LogError("[StartMenu] CharacterSelectionUI reference not assigned!");
            SceneManager.LoadScene("MainBattle");
        }
    }
    
    public void BackToStartMenu()
    {
        ShowStartButton();
    }
    
    public void QuitGame()
    {
        Debug.Log("[StartMenu] Quit Game clicked");
        Application.Quit();
    }
    
    public static int GetSelectedCharacterIndex()
    {
        return PlayerPrefs.GetInt("SelectedCharacterIndex", 0);
    }
    
    public static bool SelectedCharacterHasSkills()
    {
        return PlayerPrefs.GetInt("HasSkills", 0) == 1;
    }
}