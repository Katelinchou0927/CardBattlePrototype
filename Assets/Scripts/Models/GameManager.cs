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
    public Sprite[] cardSprites; // All card images
    public Sprite cardBack;      // Card back
    public Button confirmButton;

    [Header("Hand Areas")]
    public Transform player1HandArea;
    public Transform player2HandArea;

    private List<CardData> player1Cards = new List<CardData>();
    private List<CardData> player2Cards = new List<CardData>();

    private CardData selectedHpCard = null;
    private CardData selectedAtkCard = null;

    private bool atkConfirmed = false;
    private bool hpConfirmed = false;

    void Awake()
    {
        Instance = this;
    }

    public void Start() { }

    public void StartGame()
    {
        Debug.Log("[GameManager] StartGame called, initializing");
        
        // Clear existing cards
        foreach (Transform child in player1HandArea) Destroy(child.gameObject);
        foreach (Transform child in player2HandArea) Destroy(child.gameObject);
        player1Cards.Clear();
        player2Cards.Clear();
        
        // Deal cards logic
        DealCards("Player1", "club", player1HandArea, player1Cards);
        DealCards("Player2", "diamond", player2HandArea, player2Cards);

        // Show confirm button
        atkConfirmed = false;
        hpConfirmed = false;
        selectedAtkCard = null;
        selectedHpCard = null;

        confirmButton.gameObject.SetActive(true);
        confirmButton.interactable = false;
        
        // Setup button
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirmClicked);
        confirmButton.GetComponentInChildren<TMP_Text>().text = "Confirm ATK";
    }

    void DealCards(string ownerId, string suit, Transform handArea, List<CardData> cardList)
    {
        List<int> numbers = new List<int>();
        for (int i = 1; i <= 10; i++) numbers.Add(i);
        Shuffle(numbers);

        for (int i = 0; i < numbers.Count; i++)
        {
            GameObject cardGO = Instantiate(cardPrefab, handArea);
            CardDisplay display = cardGO.GetComponent<CardDisplay>();

            int number = numbers[i];
            Sprite cardSprite = GetCardSprite(number, suit);
            
            display.SetCardFace(cardSprite);

            // Show front for Player1, back for others
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
    }

    void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    Sprite GetCardSprite(int number, string suit)
    {
        // Fixed method to work with your poker card sprites
        string cardValue = number.ToString();
        string suitName = suit == "club" ? "Clubs" : "Diamonds";
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
        
        // Fallback: use index-based approach
        int index = (number - 1) + (suit == "diamond" ? 13 : 0);
        if (index >= 0 && index < cardSprites.Length)
        {
            Debug.Log($"[GameManager] Using fallback index {index}: {cardSprites[index].name}");
            return cardSprites[index];
        }
        
        // Last resort: return first sprite to avoid null
        return cardSprites.Length > 0 ? cardSprites[0] : null;
    }

    public void OnCardClicked(CardDisplay clicked)
    {
        CardData data = clicked.GetData();
        if (data.used) return;

        // Phase 1: Select attack card
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
                    selectedAtkCard.cardType = CardType.Normal;

                selectedAtkCard = data;
                data.cardType = CardType.Attack;
            }
        }
        // Phase 2: Select HP card
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
        
        Debug.Log($"Clicked card {data.number}, current type: {data.cardType}");

        // Refresh all cards
        foreach (var card in player1Cards)
            card.GetComponent<CardDisplay>().RefreshUI();

        // Update button state
        confirmButton.interactable = (!atkConfirmed && selectedAtkCard != null)
                                  || (atkConfirmed && !hpConfirmed && selectedHpCard != null);
    }

    public void OnConfirmClicked()
    {
        if (!atkConfirmed && selectedAtkCard != null)
        {
            selectedAtkCard.used = true;
            atkConfirmed = true;
            confirmButton.interactable = false;

            Debug.Log($"Attack confirmed: ATK = {selectedAtkCard.number}");
            confirmButton.GetComponentInChildren<TMP_Text>().text = "Confirm HP";
        }
        else if (atkConfirmed && !hpConfirmed && selectedHpCard != null)
        {
            selectedHpCard.used = true;
            hpConfirmed = true;
            confirmButton.interactable = true;
            
            Debug.Log($"HP confirmed: HP = {selectedHpCard.number}");
            confirmButton.GetComponentInChildren<TMP_Text>().text = "Start Battle";

            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(StartBattle);
        }

        foreach (var card in player1Cards)
            card.GetComponent<CardDisplay>().RefreshUI();
    }

    void StartBattle()
    {
        confirmButton.gameObject.SetActive(false);
        Debug.Log($"Battle starts! ATK = {selectedAtkCard.number}, HP = {selectedHpCard.number}");
        
        // TODO: This is where we'll integrate task 2
        // For now, just prepare the data for battle system
        int playerHP = selectedHpCard.number;
        int playerATK = selectedAtkCard.number;
        
        // Call battle system initialization
        InitializeBattleSystem(playerHP, playerATK);
    }

    // Method to get selected values (for integration with task 2)
    public (int hp, int atk) GetSelectedValues()
    {
        int hp = selectedHpCard != null ? selectedHpCard.number : 0;
        int atk = selectedAtkCard != null ? selectedAtkCard.number : 0;
        return (hp, atk);
    }

    public bool IsSelectionComplete()
    {
        return atkConfirmed && hpConfirmed;
    }

    void InitializeBattleSystem(int playerHP, int playerATK)
    {
        Debug.Log($"[GameManager] Initializing battle with HP: {playerHP}, ATK: {playerATK}");
    
        // 隐藏卡牌选择界面
        if (player1HandArea.parent != null)
            player1HandArea.parent.gameObject.SetActive(false);
    
        // Find or create battle system
        SimpleBattleSystem battleSystem = FindObjectOfType<SimpleBattleSystem>();
        if (battleSystem == null)
        {
            GameObject battleObj = new GameObject("SimpleBattleSystem");
            battleSystem = battleObj.AddComponent<SimpleBattleSystem>();
        }
    
        // Start the battle
        battleSystem.InitializeBattle(playerHP, playerATK);
    }
}